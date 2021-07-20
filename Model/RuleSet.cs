using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {
    class RuleSet {
        private TreeStore treeStore;
        public TreeModelSort Model { get; private set; }
        private List<Location> locations;
        public StoryItems StoryItems { get; private set; }
        private Dictionary<string, string> languages;
        public bool HasChanged { get; private set; }

        public RuleSet() {
            treeStore = new TreeStore(typeof(TreeModelBaseClass));
            Model = new TreeModelSort(treeStore);
            Model.SetSortFunc(0, TreeModelBaseClass.Compare);
            Model.SetSortColumnId(0, SortType.Ascending);
            locations = new List<Location>();
            StoryItems = new StoryItems(this);
            languages = new Dictionary<string, string>();
            HasChanged = false;
        }

        public void Cleanup() {
            foreach (Location location in locations) {
                location.Cleanup();
            }
            locations.Clear();
            StoryItems.Cleanup();
            languages.Clear();
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

        public bool AddLocation(string location) {
            Location loc = new Location(location, this);
            if (!locations.Contains(loc)) {
                locations.Add(loc);
                loc.Iter = treeStore.AppendValues(loc);
                loc.ItemIter = treeStore.AppendValues(loc.Iter, new LocationCategory("Items", loc, Check.CheckType.ITEM, this));
                loc.PokemonIter = treeStore.AppendValues(loc.Iter, new LocationCategory("Pokémon", loc, Check.CheckType.POKEMON, this));
                loc.TradeIter = treeStore.AppendValues(loc.Iter, new LocationCategory("Trades", loc, Check.CheckType.TRADE, this));
                loc.TrainerIter = treeStore.AppendValues(loc.Iter, new LocationCategory("Trainers", loc, Check.CheckType.TRAINER, this));
                HasChanged = true;
                return true;
            }
            loc.Cleanup();
            return false;
        }

        public bool AddCheck(Location location, string check, Check.CheckType type) {
            Console.WriteLine($"RuleSet.AddCheck({check})");
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
                Console.WriteLine($"Location.AddCheck({chk.Id})=true");
                chk.Iter = treeStore.AppendValues(parentIter, chk);
                return true;
            }
            else {
                Console.WriteLine($"Location.AddCheck({chk.Id})=false");
                chk.Cleanup();
                return false;
            }
        }

        public bool AddLanguage(string code, string name) {
            if (!languages.ContainsKey(code)) {
                languages.Add(code, name);
                HasChanged = true;
                return true;
            }
            return false;
        }

        public void ReportChange() {
            HasChanged = true;
        }

        public void SaveToFile(string filepath) {

        }

        public static RuleSet FromFile(string filepath) {
            RuleSet ret = new RuleSet();
            using (StreamReader file = File.OpenText(filepath)) {
                JsonSerializer serializer = new JsonSerializer();

            }
            return ret;
        }
    }
}
