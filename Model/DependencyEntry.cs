using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {

    abstract public class DependencyEntryBase : IEquatable<DependencyEntryBase>, IComparable<DependencyEntryBase> {
        private string id;
        public string Id { get => id; set { id = value; Rules.ReportChange(); } }

        public abstract RuleSet Rules { get; }

        public TreeIter Iter { get; set; }

        public DependencyEntryBase(string id) {
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

    public class LocationCategory : DependencyEntryBase {
        public Location Parent { get; private set; }
        public Check.Type Type { get; private set; }
        public virtual int CheckCount => Parent.GetListForCheckType(Type).Count;

        public override RuleSet Rules => Parent.Rules;

        public LocationCategory(string id, Location parent, Check.Type type) : base(id) {
            Parent = parent;
            Type = type;
        }
    }

    public class LocationCategoryForLocations : LocationCategory {
        public override int CheckCount => Parent.Locations.Count;
        public LocationCategoryForLocations(string id, Location parent) : base(id, parent, Check.Type.ITEM) {

        }
    }

    abstract public class DependencyEntry : DependencyEntryBase, IMovable, ILocalizable {
        public Dictionary<Check, TreeIter> ItemsConditions { get; private set; }
        public Dictionary<Check, TreeIter> PokemonConditions { get; private set; }
        public Dictionary<Check, TreeIter> TradesConditions { get; private set; }
        public Dictionary<Check, TreeIter> TrainersConditions { get; private set; }
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

        public int ConditionCount => ItemsConditions.Count + PokemonConditions.Count + TradesConditions.Count + TrainersConditions.Count + StoryItemsConditions.Count;

        public ILocationContainer Parent { get; private set; }
        public override RuleSet Rules => Parent.Rules;

        public abstract string FullPath { get; }

        public DependencyEntry(string id, ILocationContainer parent) : base(id) {
            ItemsConditions = new Dictionary<Check, TreeIter>();
            PokemonConditions = new Dictionary<Check, TreeIter>();
            TradesConditions = new Dictionary<Check, TreeIter>();
            TrainersConditions = new Dictionary<Check, TreeIter>();
            StoryItemsConditions = new StoryItemsConditions(this);
            itemsTreeStore = new TreeStore(typeof(Check));
            ItemsModel = new TreeModelSort(itemsTreeStore);
            pokemonTreeStore = new TreeStore(typeof(Check));
            PokemonModel = new TreeModelSort(pokemonTreeStore);
            tradesTreeStore = new TreeStore(typeof(Check));
            TradesModel = new TreeModelSort(tradesTreeStore);
            trainersTreeStore = new TreeStore(typeof(Check));
            TrainersModel = new TreeModelSort(trainersTreeStore);
            Parent = parent;
            Localization = new Localization(this);
        }

        virtual public void Cleanup() {
            foreach (Check.Type checkType in Enum.GetValues(typeof(Check.Type))) {
                Dictionary<Check, TreeIter> conditions = GetListForConditionType(checkType);
                foreach (Check condition in conditions.Keys) {
                    condition.RemoveDependingEntry(this);
                }
                TreeStore tree = GetTreeStoreForConditionType(checkType);
                tree.Clear();
                tree.Dispose();
                conditions.Clear();
            }
            StoryItemsConditions.InvokeRemove();
            Localization.Cleanup();
            Parent = null;
        }

        virtual public void InvokeRemove() {
            Rules.RemoveDependencyIter(Iter);
            Parent.ChildWasRemoved(this);
            Rules.ReportChange();
            Cleanup();
        }

        public void ConditionWasRemoved(Check check) {
            Dictionary<Check, TreeIter> list = GetListForConditionType(check.CheckType);
            TreeIter iter = list[check];
            GetTreeStoreForConditionType(check.CheckType).Remove(ref iter);
            list.Remove(check);
        }

        public Dictionary<Check, TreeIter> GetListForConditionType(Check.Type condition) {
            switch (condition) {
                case Check.Type.ITEM:
                    return ItemsConditions;
                case Check.Type.POKEMON:
                    return PokemonConditions;
                case Check.Type.TRADE:
                    return TradesConditions;
                case Check.Type.TRAINER:
                    return TrainersConditions;
                default:
                    return null;
            }
        }

        private TreeStore GetTreeStoreForConditionType(Check.Type condition) {
            switch (condition) {
                case Check.Type.ITEM:
                    return itemsTreeStore;
                case Check.Type.POKEMON:
                    return pokemonTreeStore;
                case Check.Type.TRADE:
                    return tradesTreeStore;
                case Check.Type.TRAINER:
                    return trainersTreeStore;
                default:
                    return null;
            }
        }

        public bool AddCondition(Check condition) {
            Dictionary<Check, TreeIter> conditionList = GetListForConditionType(condition.CheckType);
            if (!conditionList.ContainsKey(condition)) {
                TreeStore treeStore = GetTreeStoreForConditionType(condition.CheckType);
                conditionList.Add(condition, treeStore.AppendValues(condition));
                condition.AddDependingEntry(this);
                Rules.ReportChange();
                return true;
            }
            return false;
        }

        public void RemoveCondition(Check condition) {
            Dictionary<Check, TreeIter> conditionList = GetListForConditionType(condition.CheckType);
            TreeStore treeStore = GetTreeStoreForConditionType(condition.CheckType);
            TreeIter iter = conditionList[condition];
            treeStore.Remove(ref iter);
            condition.RemoveDependingEntry(this);
            conditionList.Remove(condition);
            Rules.ReportChange();
        }
    }

    [JsonConverter(typeof(LocationConverter))]
    public class Location : DependencyEntry, ILocationContainer {
        public List<Check> Items { get; set; }
        public List<Check> Pokemon { get; set; }
        public List<Check> Trades { get; set; }
        public List<Check> Trainers { get; set; }
        public List<Location> Locations { get; set; }

        public int CheckCount => Items.Count + Pokemon.Count + Trades.Count + Trainers.Count + SumLocationChecks();

        public TreeIter ItemIter { get; set; }
        public TreeIter PokemonIter { get; set; }
        public TreeIter TradeIter { get; set; }
        public TreeIter TrainerIter { get; set; }
        public TreeIter LocationIter { get; set; }

        public override string FullPath => this.LocationPath();

        public Location(string id, ILocationContainer parent) : base(id, parent) {
            Items = new List<Check>();
            Pokemon = new List<Check>();
            Trades = new List<Check>();
            Trainers = new List<Check>();
            Locations = new List<Location>();
        }

        private int SumLocationChecks() {
            int ret = 0;
            foreach (Location loc in Locations) {
                ret += loc.CheckCount;
            }
            return ret;
        }

        override public void Cleanup() {
            foreach (List<Check> checks in new List<List<Check>>() { Items, Pokemon, Trades, Trainers }) {
                foreach (Check check in checks) {
                    check.Cleanup();
                }
                checks.Clear();
            }
            foreach (Location loc in Locations) {
                loc.Cleanup();
            }
            Locations.Clear();
            base.Cleanup();
        }

        public bool HasChecksOfType(Check.Type type) {
            if (GetListForCheckType(type).Count > 0) {
                return true;
            }
            else {
                foreach (Location loc in Locations) {
                    if (loc.HasChecksOfType(type)) {
                        return true;
                    }
                }
                return false;
            }
        }

        public void ChildWasRemoved(DependencyEntry entry) {
            if (entry is Location location) {
                Locations.Remove(location);
            }
            else if (entry is Check check) {
                GetListForCheckType(check.CheckType).Remove(check);
            }
        }

        public List<Check> GetListForCheckType(Check.Type type) {
            switch (type) {
                case Check.Type.ITEM:
                    return Items;
                case Check.Type.POKEMON:
                    return Pokemon;
                case Check.Type.TRADE:
                    return Trades;
                case Check.Type.TRAINER:
                    return Trainers;
                default:
                    return null;
            }
        }

        public bool CheckNameAvailable(string checkName, Check.Type type) {
            List<Check> list = GetListForCheckType(type);
            foreach (Check check in list) {
                if (check.Id.Equals(checkName)) {
                    return false;
                }
            }
            return true;
        }

        public Check AddCheck(string check, Check.Type type) {
            if (CheckNameAvailable(check, type)) {
                Check chk = new Check(check, type, this);
                List<Check> list = GetListForCheckType(chk.CheckType);
                list.Add(chk);
                Rules.MergeCheckToModel(chk);
                Rules.ReportChange();
                return chk;
            }
            return null;
        }

        public bool MoveUp(DependencyEntry movable) {
            return MoveInternal(movable, true);
        }

        public bool MoveDown(DependencyEntry movable) {
            return MoveInternal(movable, false);
        }

        private bool MoveInternal(DependencyEntry movable, bool up) {
            return movable is Check check
                ? InterfaceHelpers.SwapItems(GetListForCheckType(check.CheckType), check, up)
                : movable is Location location && InterfaceHelpers.SwapItems(Locations, location, up);
        }
    }

    [JsonConverter(typeof(CheckConverter))]
    public class Check : DependencyEntry {
        public enum Type {
            ITEM, POKEMON, TRADE, TRAINER
        }

        public Type CheckType { get; private set; }

        private readonly List<DependencyEntry> dependingEntries;
        public int DependingEntryCount => dependingEntries.Count;
        public override string FullPath => (Parent as Location).LocationPath() + "." + CheckType.ToString().ToLower() + (CheckType == Type.POKEMON ? "." : "s.") + Id;

        public Check(string id, Type type, Location parent) : base(id, parent) {
            CheckType = type;
            dependingEntries = new List<DependencyEntry>();
        }

        public override void Cleanup() {
            foreach (DependencyEntry dependingEntry in dependingEntries) {
                dependingEntry.ConditionWasRemoved(this);
            }
            dependingEntries.Clear();
            base.Cleanup();
        }

        public void AddDependingEntry(DependencyEntry entry) {
            dependingEntries.Add(entry);
            Rules.ReportChange();
        }

        public void RemoveDependingEntry(DependencyEntry entry) {
            while (dependingEntries.Remove(entry)) ;
            Rules.ReportChange();
        }
    }
}
