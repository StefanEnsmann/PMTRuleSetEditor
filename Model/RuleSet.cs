using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {
    [JsonConverter(typeof(RuleSetConverter))]
    class RuleSet : ILocationContainer {
        public string Name { get; set; }
        public string Game { get; set; }

        private readonly TreeStore treeStore;
        public TreeStore Model => treeStore;
        private readonly List<Location> locations;
        public List<Location> Locations => new List<Location>(locations);
        public StoryItems StoryItems { get; private set; }
        private readonly List<string> activeLanguages;
        public List<string> ActiveLanguages => new List<string>(activeLanguages);
        public bool HasChanged { get; private set; }

        public ILocationContainer Parent { get { return null; } set { } }

        public RuleSet() {
            treeStore = new TreeStore(typeof(DependencyEntryBase));
            locations = new List<Location>();
            StoryItems = new StoryItems(this);
            activeLanguages = new List<string>();
            HasChanged = false;
        }

        public void Cleanup() {
            foreach (Location location in locations) {
                location.Cleanup();
            }
            locations.Clear();
            StoryItems.Cleanup();
            activeLanguages.Clear();
            treeStore.Clear();
        }

        public bool LocationNameAvailable(string location) {
            if (!(new string[] { "items", "pokemon", "trades", "trainers" }).Contains(location)) {
                bool available = true;
                Location loc = new Location(location, this);
                if (locations.Contains(loc)) {
                    available = false;
                }
                loc.Cleanup();
                return available;
            }
            return false;
        }

        public Location AddLocation(string location) {
            Location loc = new Location(location, this, this);
            if (!locations.Contains(loc)) {
                locations.Add(loc);
                MergeLocationToModel(loc);
                HasChanged = true;
                return loc;
            }
            else {
                loc.Cleanup();
                return null;
            }
        }

        public void RemoveLocation(Location location) {
            locations.Remove(location);
            RemoveLocationIter(location);
            location.Cleanup();
            HasChanged = true;
        }

        public void MergeLocationToModel(Location location) {
            location.Iter = location.Parent is Location
                ? treeStore.AppendValues((location.Parent as Location).LocationIter, location)
                : treeStore.AppendValues(location);

            location.ItemIter = treeStore.AppendValues(location.Iter, new LocationCategory("Items", location, Check.CheckType.ITEM, this));
            location.PokemonIter = treeStore.AppendValues(location.Iter, new LocationCategory("Pokémon", location, Check.CheckType.POKEMON, this));
            location.TradeIter = treeStore.AppendValues(location.Iter, new LocationCategory("Trades", location, Check.CheckType.TRADE, this));
            location.TrainerIter = treeStore.AppendValues(location.Iter, new LocationCategory("Trainers", location, Check.CheckType.TRAINER, this));
            location.LocationIter = treeStore.AppendValues(location.Iter, new LocationCategoryForLocations("Locations", location, this));
        }

        public void RemoveLocationIter(Location location) {
            TreeIter iter = location.Iter;
            treeStore.Remove(ref iter);
        }

        public void MergeCheckToModel(Check check) {
            TreeIter parentIter = TreeIter.Zero;
            Location location = check.Parent as Location;
            switch (check.Type) {
                case Check.CheckType.ITEM:
                    parentIter = location.ItemIter; break;
                case Check.CheckType.POKEMON:
                    parentIter = location.PokemonIter; break;
                case Check.CheckType.TRADE:
                    parentIter = location.TradeIter; break;
                case Check.CheckType.TRAINER:
                    parentIter = location.TrainerIter; break;
            }
            check.Iter = treeStore.AppendValues(parentIter, check);
        }

        public void RemoveCheckIter(Check check) {
            TreeIter iter = check.Iter;
            treeStore.Remove(ref iter);
        }

        public bool MoveUp(DependencyEntry location) {
            return InterfaceHelpers.SwapItems(locations, (Location)location, true);
        }

        public bool MoveDown(DependencyEntry location) {
            return InterfaceHelpers.SwapItems(locations, (Location)location, false);
        }

        public void SetLanguageActive(string language, bool active = true) {
            foreach (Location loc in locations) {
                loc.SetLanguageActive(language, active);
            }
            StoryItems.SetLanguageActive(language, active);
            if (active && !activeLanguages.Contains(language)) {
                activeLanguages.Add(language);
            }
            else {
                activeLanguages.Remove(language);
            }
        }

        public void ReportChange() {
            HasChanged = true;
        }

        public void SaveToFile(string filepath) {
            activeLanguages.Sort();
            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
            using (StreamWriter file = File.CreateText(filepath)) {
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, this);
            }
            HasChanged = false;
        }

        public static RuleSet FromFile(string filepath) {
            JsonSerializer serializer = new JsonSerializer();
            RuleSet ret;
            using (StreamReader reader = new StreamReader(filepath)) {
                ret = (RuleSet)serializer.Deserialize(reader, typeof(RuleSet));
            }
            return ret;
        }
    }
}
