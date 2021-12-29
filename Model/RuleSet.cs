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
    class RuleSet {
        public string Name { get; private set; }
        public string Game { get; private set; }

        private TreeStore treeStore;
        public TreeModelSort Model { get; private set; }
        private List<Location> locations;
        public List<Location> Locations => new List<Location>(locations);
        public StoryItems StoryItems { get; private set; }
        private List<string> activeLanguages;
        public List<string> ActiveLanguages => new List<string>(activeLanguages);
        public bool HasChanged { get; private set; }
        public MainProg Main { get; private set; }

        public RuleSet(MainProg main) {
            Main = main;
            treeStore = new TreeStore(typeof(DependencyEntryBase));
            Model = new TreeModelSort(treeStore);
            Model.SetSortFunc(0, DependencyEntryBase.Compare);
            Model.SetSortColumnId(0, SortType.Ascending);
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

        public void SetName(string name) {
            Name = name;
        }

        public void SetGame(string game) {
            Game = game;
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

        public bool AddLocation(string location, Location parent = null) {
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
                return true;
            }
            loc.Cleanup();
            return false;
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

        public bool AddCheck(Location location, string check, Check.CheckType type) {
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
                    return false;
            }
            if (location.AddCheck(chk)) {
                chk.Iter = treeStore.AppendValues(parentIter, chk);
                foreach(string language in activeLanguages) {
                    chk.SetLanguageActive(language);
                }
                HasChanged = true;
                return true;
            }
            else {
                chk.Cleanup();
                return false;
            }
        }

        public void RemoveCheck(Location location, Check check) {
            TreeIter iter = check.Iter;
            treeStore.Remove(ref iter);
            location.RemoveCheck(check);
            HasChanged = true;
        }

        public void SetLanguageActive(string language, bool active=true) {
            foreach(Location loc in locations) {
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
            locations.Sort();
            activeLanguages.Sort();
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter file = File.CreateText(filepath)) {
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, this);
            }
            HasChanged = false;
        }

        public static RuleSet FromFile(string filepath, MainProg main) {
            return new RuleSet(main);
        }
    }
}
