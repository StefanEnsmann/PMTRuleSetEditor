using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {

    interface ILocationContainer : IMovableItems<DependencyEntry> {
        ILocationContainer Parent { get; set; }
        Location AddLocation(string location);
        void RemoveLocation(Location location);
        bool LocationNameAvailable(string name);
    }

    abstract class DependencyEntryBase : IEquatable<DependencyEntryBase>, IComparable<DependencyEntryBase> {
        private string id;
        public string Id { get => id; set { id = value; RuleSet.ReportChange(); } }
        public TreeIter Iter { get; set; }
        public RuleSet RuleSet { get; private set; }

        public DependencyEntryBase(string id, RuleSet ruleSet) {
            RuleSet = ruleSet;
            this.id = id;
        }

        public override string ToString() {
            return $"{GetType()}: {id}";
        }

        public override bool Equals(object obj) {
            return obj != null && obj is DependencyEntryBase other && Equals(other);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public bool Equals(DependencyEntryBase other) {
            return Id.Equals(other.Id);
        }

        public static int Compare(DependencyEntryBase a, DependencyEntryBase b) {
            return a != null && b != null ? string.Compare(a.Id, b.Id) : a != null ? -1 : b != null ? 1 : 0;
        }

        public static int Compare(TreeModel model, TreeIter a, TreeIter b) {
            return Compare((DependencyEntryBase)model.GetValue(a, 0), (DependencyEntryBase)model.GetValue(b, 0));
        }

        public int CompareTo(DependencyEntryBase other) {
            return Compare(this, other);
        }
    }

    class LocationCategory : DependencyEntryBase {
        public Location Parent { get; private set; }
        public Check.CheckType Type { get; private set; }
        public virtual int CheckCount {
            get {
                switch(Type) {
                    case Check.CheckType.ITEM:
                        return Parent.ItemCount;
                    case Check.CheckType.POKEMON:
                        return Parent.PokemonCount;
                    case Check.CheckType.TRADE:
                        return Parent.TradeCount;
                    case Check.CheckType.TRAINER:
                        return Parent.TrainerCount;
                }
                return 0;
            }
        }
        public LocationCategory(string id, Location parent, Check.CheckType type, RuleSet ruleSet) : base(id, ruleSet) {
            Parent = parent;
            Type = type;
        }
    }

    class LocationCategoryForLocations : LocationCategory {
        public override int CheckCount => Parent.LocationCount;
        public LocationCategoryForLocations(string id, Location parent, RuleSet ruleSet) : base(id, parent, Check.CheckType.ITEM, ruleSet) {

        }
    }

    abstract class DependencyEntry : DependencyEntryBase, IMovable, ILocalizable {
        private readonly List<Check> itemsConditions;
        private readonly List<Check> pokemonConditions;
        private readonly List<Check> tradesConditions;
        private readonly List<Check> trainersConditions;
        public List<Check> ItemsConditions => new List<Check>(itemsConditions);
        public List<Check> PokemonConditions => new List<Check>(pokemonConditions);
        public List<Check> TradesConditions => new List<Check>(tradesConditions);
        public List<Check> TrainersConditions => new List<Check>(trainersConditions);
        public StoryItemsConditions StoryItemsConditions { get; private set; }
        public Localization Localization { get; set; }
        private readonly TreeStore itemsTreeStore;
        private readonly TreeStore pokemonTreeStore;
        private readonly TreeStore tradesTreeStore;
        private readonly TreeStore trainersTreeStore;
        public TreeModelSort ItemsModel { get; private set; }
        public TreeModelSort PokemonModel { get; private set; }
        public TreeModelSort TradesModel { get; private set; }
        public TreeModelSort TrainersModel { get; private set; }

        public int ConditionCount => ItemCondCount + PokemonCondCount + TradeCondCount + TrainerCondCount + StoryItemsCondCount;
        public int ItemCondCount => itemsConditions.Count;
        public int PokemonCondCount => pokemonConditions.Count;
        public int TradeCondCount => tradesConditions.Count;
        public int TrainerCondCount => trainersConditions.Count;
        public int StoryItemsCondCount => StoryItemsConditions.Count;

        public ILocationContainer Parent { get; set; }

        public virtual string FullPath => "";

        public DependencyEntry(string id, RuleSet ruleSet, ILocationContainer parent = null) : base(id, ruleSet) {
            itemsConditions = new List<Check>();
            pokemonConditions = new List<Check>();
            tradesConditions = new List<Check>();
            trainersConditions = new List<Check>();
            StoryItemsConditions = new StoryItemsConditions(this);
            itemsTreeStore = new TreeStore(typeof(Check));
            ItemsModel = new TreeModelSort(itemsTreeStore);
            pokemonTreeStore = new TreeStore(typeof(Check));
            PokemonModel = new TreeModelSort(pokemonTreeStore);
            tradesTreeStore = new TreeStore(typeof(Check));
            TradesModel = new TreeModelSort(tradesTreeStore);
            trainersTreeStore = new TreeStore(typeof(Check));
            TrainersModel = new TreeModelSort(trainersTreeStore);
            Localization = new Localization(RuleSet, MainProg.SupportedLanguages.Keys.ToList(), ruleSet.ActiveLanguages);
            Parent = parent ?? MainProg.RuleSet;
        }

        virtual public void Cleanup() {
            foreach (List<Check> conditions in new List<List<Check>>() { itemsConditions, pokemonConditions, tradesConditions, trainersConditions }) {
                conditions.Clear();
            }
            StoryItemsConditions.Cleanup();
        }

        private List<Check> GetListForCondition(Check condition) {
            switch (condition.Type) {
                case Check.CheckType.ITEM:
                    return itemsConditions;
                case Check.CheckType.POKEMON:
                    return pokemonConditions;
                case Check.CheckType.TRADE:
                    return tradesConditions;
                case Check.CheckType.TRAINER:
                    return trainersConditions;
                default:
                    return null;
            }
        }

        private TreeStore GetTreeStoreForCondition(Check condition) {
            switch (condition.Type) {
                case Check.CheckType.ITEM:
                    return itemsTreeStore;
                case Check.CheckType.POKEMON:
                    return pokemonTreeStore;
                case Check.CheckType.TRADE:
                    return tradesTreeStore;
                case Check.CheckType.TRAINER:
                    return trainersTreeStore;
                default:
                    return null;
            }
        }

        public bool AddCondition(Check condition) {
            List<Check> conditionList = GetListForCondition(condition);
            if (!conditionList.Contains(condition)) {
                conditionList.Add(condition);
                TreeStore treeStore = GetTreeStoreForCondition(condition);
                condition.AddDependingEntry(this, treeStore.AppendValues(condition));
                RuleSet.ReportChange();
                return true;
            }
            return false;
        }

        public void RemoveCondition(Check condition) {
            List<Check> conditionList = GetListForCondition(condition);
            while (conditionList.Remove(condition)) ;
            TreeStore treeStore = GetTreeStoreForCondition(condition);
            TreeIter iter = condition.GetDependencyIter(this);
            treeStore.Remove(ref iter);
            condition.RemoveDependingEntry(this);
            RuleSet.ReportChange();
        }

        public virtual void SetLanguageActive(string language, bool active=true) {
            Localization.SetLanguageActive(language, active);
        }
    }

    [JsonConverter(typeof(LocationConverter))]
    class Location : DependencyEntry, ILocationContainer {
        private readonly List<Check> items;
        public List<Check> Items => new List<Check>(items);
        private readonly List<Check> pokemon;
        public List<Check> Pokemon => new List<Check>(pokemon);
        private readonly List<Check> trades;
        public List<Check> Trades => new List<Check>(trades);
        private readonly List<Check> trainers;
        public List<Check> Trainers => new List<Check>(trainers);
        private readonly List<Location> locations;
        public List<Location> Locations => new List<Location>(locations);

        public int CheckCount => ItemCount + PokemonCount + TradeCount + TrainerCount + SumLocationChecks();
        public int ItemCount => items.Count;
        public int PokemonCount => pokemon.Count;
        public int TradeCount => trades.Count;
        public int TrainerCount => trainers.Count;
        public int LocationCount => locations.Count;

        public TreeIter ItemIter { get; set; }
        public TreeIter PokemonIter { get; set; }
        public TreeIter TradeIter { get; set; }
        public TreeIter TrainerIter { get; set; }
        public TreeIter LocationIter { get; set; }
        public string LocationPath { get {
                string ret = Id;
                ILocationContainer currentLoc = this;
                while (!(currentLoc.Parent is RuleSet)) {
                    currentLoc = currentLoc.Parent;
                    ret = (currentLoc as Location).Id + "." + ret;
                }
                return ret;
            }
        }
        public override string FullPath => LocationPath;

        public Location(string id, RuleSet ruleSet, ILocationContainer parent = null) : base(id, ruleSet, parent) {
            items = new List<Check>();
            pokemon = new List<Check>();
            trades = new List<Check>();
            trainers = new List<Check>();
            locations = new List<Location>();
        }

        private int SumLocationChecks() {
            int ret = 0;
            foreach (Location loc in locations) {
                ret += loc.CheckCount;
            }
            return ret;
        }

        override public void Cleanup() {
            base.Cleanup();
            foreach (List<Check> checks in new List<List<Check>>() { items, pokemon, trades, trainers }) {
                foreach (Check check in checks) {
                    check.Cleanup();
                }
                checks.Clear();
            }
            foreach (Location loc in locations) {
                loc.Cleanup();
            }
            locations.Clear();
            Parent = null;
        }

        public override void SetLanguageActive(string language, bool active = true) {
            base.SetLanguageActive(language, active);
            foreach (List<Check> checks in new List<List<Check>>() { items, pokemon, trades, trainers }) {
                foreach (Check check in checks) {
                    check.SetLanguageActive(language, active);
                }
            }
            foreach (Location loc in locations) {
                loc.SetLanguageActive(language, active);
            }
        }

        public bool CheckNameAvailable(string checkName, Check.CheckType type) {
            Check check = new Check(checkName, RuleSet, type);
            List<Check> list = GetListForCheck(check);
            bool available = true;
            if (list.Contains(check)) {
                available = false;
            }
            check.Cleanup();
            return available;
        }

        public bool LocationNameAvailable(string locationName) {
            if (!(new string[] { "items", "pokemon", "trades", "trainers" }).Contains(locationName)) {
                Location location = new Location(locationName, RuleSet);
                bool available = true;
                if (locations.Contains(location)) {
                    available = false;
                }
                location.Cleanup();
                return available;
            }
            return false;
        }

        private List<Check> GetListForCheck(Check check) {
            switch (check.Type) {
                case Check.CheckType.ITEM:
                    return items;
                case Check.CheckType.POKEMON:
                    return pokemon;
                case Check.CheckType.TRADE:
                    return trades;
                case Check.CheckType.TRAINER:
                    return trainers;
                default:
                    return null;
            }
        }

        public bool AddCheck(Check check) {
            List<Check> list = GetListForCheck(check);
            if (!list.Contains(check)) {
                check.Parent = this;
                list.Add(check);
                RuleSet.ReportChange();
                return true;
            }
            return false;
        }

        public void RemoveCheck(Check check) {
            List<Check> list = GetListForCheck(check);
            check.Cleanup();
            while (list.Remove(check)) ;
            RuleSet.ReportChange();
        }

        public bool MoveUp(DependencyEntry movable) {
            return MoveInternal(movable, true);
        }

        public bool MoveDown(DependencyEntry movable) {
            return MoveInternal(movable, false);
        }

        private bool MoveInternal(DependencyEntry movable, bool up) {
            if (movable is Check check) {
                switch (check.Type) {
                    case Check.CheckType.ITEM:
                        return InterfaceHelpers.SwapItems(items, check, up);
                    case Check.CheckType.POKEMON:
                        return InterfaceHelpers.SwapItems(pokemon, check, up);
                    case Check.CheckType.TRADE:
                        return InterfaceHelpers.SwapItems(trades, check, up);
                    case Check.CheckType.TRAINER:
                        return InterfaceHelpers.SwapItems(trainers, check, up);
                    default:
                        return false;
                }
            }
            else {
                return movable is Location location && InterfaceHelpers.SwapItems(locations, location, up);
            }
        }

        public Location AddLocation(string location) {
            Location loc = new Location(location, RuleSet, this);
            if (!locations.Contains(loc)) {
                locations.Add(loc);
                RuleSet.MergeLocationToModel(loc);
                RuleSet.ReportChange();
                return loc;
            }
            else {
                loc.Cleanup();
                return null;
            }
        }

        public void RemoveLocation(Location location) {
            location.Cleanup();
            while (locations.Remove(location)) ;
            RuleSet.RemoveLocationIter(location);
            RuleSet.ReportChange();
        }
    }

    [JsonConverter(typeof(CheckConverter))]
    class Check : DependencyEntry {
        public enum CheckType {
            ITEM, POKEMON, TRADE, TRAINER
        }

        public CheckType Type { get; private set; }
        private readonly Dictionary<DependencyEntry, TreeIter> dependingEntries;
        public int DependingEntryCount => dependingEntries.Count;
        public override string FullPath => (Parent as Location).LocationPath + "." + Type.ToString().ToLower() + (Type == CheckType.POKEMON ? "." : "s.") + Id;

        public Check(string id, RuleSet ruleSet, CheckType type, Location parent = null) : base(id, ruleSet, parent) {
            Type = type;
            dependingEntries = new Dictionary<DependencyEntry, TreeIter>();
        }

        public override void Cleanup() {
            base.Cleanup();
            Dictionary<DependencyEntry, TreeIter> copy = new Dictionary<DependencyEntry, TreeIter>(dependingEntries);
            foreach (DependencyEntry dependingEntry in copy.Keys) {
                dependingEntry.RemoveCondition(this);
            }
            copy.Clear();
            dependingEntries.Clear();
        }

        public void AddDependingEntry(DependencyEntry entry, TreeIter iter) {
            dependingEntries.Add(entry, iter);
            RuleSet.ReportChange();
        }

        public TreeIter GetDependencyIter(DependencyEntry entry) {
            return dependingEntries[entry];
        }

        public void RemoveDependingEntry(DependencyEntry entry) {
            while (dependingEntries.Remove(entry)) ;
            RuleSet.ReportChange();
        }
    }
}
