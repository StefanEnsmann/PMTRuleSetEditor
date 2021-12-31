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
    class RuleSet : IMovableItems<DependencyEntry> {
        public string Name { get; set; }
        public string Game { get; set; }

        private TreeStore treeStore;
        public TreeStore Model => treeStore;
        private List<Location> locations;
        public List<Location> Locations => new List<Location>(locations);
        public StoryItems StoryItems { get; private set; }
        private List<string> activeLanguages;
        public List<string> ActiveLanguages => new List<string>(activeLanguages);
        public bool HasChanged { get; private set; }

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
            bool available = true;
            Location loc = new Location(location, this);
            if (locations.Contains(loc)) {
                available = false;
            }
            loc.Cleanup();
            return available;
        }

        public Location AddLocation(string location, Location parent = null) {
            Location loc = new Location(location, this, parent);
            if (!(parent != null ? !parent.LocationNameAvailable(location) : locations.Contains(loc))) {
                if (parent == null) {
                    locations.Add(loc);
                    loc.Iter = treeStore.AppendValues(loc);
                }
                else {
                    parent.AddLocation(loc);
                    loc.Iter = treeStore.AppendValues(parent.LocationIter, loc);
                }
                loc.ItemIter = treeStore.AppendValues(loc.Iter, new LocationCategory("Items", loc, Check.CheckType.ITEM, this));
                loc.PokemonIter = treeStore.AppendValues(loc.Iter, new LocationCategory("Pokémon", loc, Check.CheckType.POKEMON, this));
                loc.TradeIter = treeStore.AppendValues(loc.Iter, new LocationCategory("Trades", loc, Check.CheckType.TRADE, this));
                loc.TrainerIter = treeStore.AppendValues(loc.Iter, new LocationCategory("Trainers", loc, Check.CheckType.TRAINER, this));
                loc.LocationIter = treeStore.AppendValues(loc.Iter, new LocationCategoryForLocations("Locations", loc, this));
                foreach (string activeLanguage in activeLanguages) {
                    loc.SetLanguageActive(activeLanguage);
                }
                HasChanged = true;
                return loc;
            }
            loc.Cleanup();
            return null;
        }

        public void RemoveLocation(Location location) {
            TreeIter iter = location.Iter;
            treeStore.Remove(ref iter);
            if (location.Parent != null) {
                location.Parent.RemoveLocation(location);
            }
            else {
                locations.Remove(location);
                location.Cleanup();
            }
            HasChanged = true;
        }

        public bool MoveUp(DependencyEntry location) {
            Console.WriteLine("RuleSet: Move up " + location.Id);
            return InterfaceHelpers.SwapItems(locations, (Location)location, true);
        }

        public bool MoveDown(DependencyEntry location) {
            Console.WriteLine("RuleSet: Move down " + location.Id);
            return InterfaceHelpers.SwapItems(locations, (Location)location, false);
        }

        public Check AddCheck(Location location, string check, Check.CheckType type) {
            Check chk = new Check(check, this, type);
            TreeIter parentIter;
            switch (type) {
                case Check.CheckType.ITEM:
                    parentIter = location.ItemIter; break;
                case Check.CheckType.POKEMON:
                    parentIter = location.PokemonIter; break;
                case Check.CheckType.TRADE:
                    parentIter = location.TradeIter; break;
                case Check.CheckType.TRAINER:
                    parentIter = location.TrainerIter; break;
                default:
                    return null;
            }
            if (location.AddCheck(chk)) {
                chk.Iter = treeStore.AppendValues(parentIter, chk);
                foreach (string language in activeLanguages) {
                    chk.SetLanguageActive(language);
                }
                HasChanged = true;
                return chk;
            }
            else {
                chk.Cleanup();
                return null;
            }
        }

        public void RemoveCheck(Location location, Check check) {
            TreeIter iter = check.Iter;
            treeStore.Remove(ref iter);
            location.RemoveCheck(check);
            HasChanged = true;
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
            Console.WriteLine("RuleSet save");
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
