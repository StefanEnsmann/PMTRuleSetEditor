using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

namespace PokemonTrackerEditor.Model {
    abstract class TreeModelBaseClass : IEquatable<TreeModelBaseClass> {
        private string id;
        public string Id { get => id; set { id = value; RuleSet.ReportChange(); } }
        public TreeIter Iter { get; set; }
        public RuleSet RuleSet { get; private set; }

        public TreeModelBaseClass(string id, RuleSet ruleSet) {
            RuleSet = ruleSet;
            this.id = id;
        }

        public override string ToString() {
            return $"{GetType()}: {id}";
        }

        public override bool Equals(object obj) {
            return obj != null && obj is TreeModelBaseClass other && Equals(other);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public bool Equals(TreeModelBaseClass other) {
            return Id.Equals(other.Id);
        }

        public static int Compare(TreeModelBaseClass a, TreeModelBaseClass b) {
            return a != null && b != null ? string.Compare(a.Id, b.Id) : a != null ? -1 : b != null ? 1 : 0;
        }

        public static int Compare(TreeModel model, TreeIter a, TreeIter b) {
            return Compare((TreeModelBaseClass)model.GetValue(a, 0), (TreeModelBaseClass)model.GetValue(b, 0));
        }
    }

    class LocationCategory : TreeModelBaseClass {
        public Location Parent { get; private set; }
        public Check.CheckType Type { get; private set; }
        public int CheckCount {
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

    abstract class DependencyEntry : TreeModelBaseClass {
        private List<Check> itemsConditions;
        private List<Check> pokemonConditions;
        private List<Check> tradesConditions;
        private List<Check> trainersConditions;
        public StoryItemsConditions StoryItemsConditions { get; private set; }
        private Dictionary<string, string> localization;
        private TreeStore itemsTreeStore;
        private TreeStore pokemonTreeStore;
        private TreeStore tradesTreeStore;
        private TreeStore trainersTreeStore;
        public TreeModelSort ItemsModel { get; private set; }
        public TreeModelSort PokemonModel { get; private set; }
        public TreeModelSort TradesModel { get; private set; }
        public TreeModelSort TrainersModel { get; private set; }

        public int ConditionCount => ItemCondCount + PokemonCondCount + TradeCondCount + TrainerCondCount + StoryItemsCondCount;
        public int ItemCondCount => itemsConditions.Count;
        public int PokemonCondCount => pokemonConditions.Count;
        public int TradeCondCount => tradesConditions.Count;
        public int TrainerCondCount => trainersConditions.Count;
        public int StoryItemsCondCount => StoryItemsConditions.CountActive;

        public DependencyEntry(string id, RuleSet ruleSet) : base(id, ruleSet) {
            itemsConditions = new List<Check>();
            pokemonConditions = new List<Check>();
            tradesConditions = new List<Check>();
            trainersConditions = new List<Check>();
            StoryItemsConditions = new StoryItemsConditions(ruleSet.StoryItems);
            localization = new Dictionary<string, string>();
            itemsTreeStore = new TreeStore(typeof(Check));
            ItemsModel = new TreeModelSort(itemsTreeStore);
            pokemonTreeStore = new TreeStore(typeof(Check));
            PokemonModel = new TreeModelSort(pokemonTreeStore);
            tradesTreeStore = new TreeStore(typeof(Check));
            TradesModel = new TreeModelSort(tradesTreeStore);
            trainersTreeStore = new TreeStore(typeof(Check));
            TrainersModel = new TreeModelSort(trainersTreeStore);
        }

        virtual public void Cleanup() {
            foreach (List<Check> conditions in new List<List<Check>>() { itemsConditions, pokemonConditions, tradesConditions, trainersConditions }) {
                conditions.Clear();
            }
            localization.Clear();
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
    }

    class Location : DependencyEntry {
        private List<Check> items;
        private List<Check> pokemon;
        private List<Check> trades;
        private List<Check> trainers;

        public int CheckCount => ItemCount + PokemonCount + TradeCount + TrainerCount;
        public int ItemCount => items.Count;
        public int PokemonCount => pokemon.Count;
        public int TradeCount => trades.Count;
        public int TrainerCount => trainers.Count;

        public TreeIter ItemIter { get; set; }
        public TreeIter PokemonIter { get; set; }
        public TreeIter TradeIter { get; set; }
        public TreeIter TrainerIter { get; set; }

        public Location(string id, RuleSet ruleSet) : base(id, ruleSet) {
            items = new List<Check>();
            pokemon = new List<Check>();
            trades = new List<Check>();
            trainers = new List<Check>();
        }

        override public void Cleanup() {
            base.Cleanup();
            foreach (List<Check> checks in new List<List<Check>>() { items, pokemon, trades, trainers }) {
                foreach (Check check in checks) {
                    check.Cleanup();
                }
                checks.Clear();
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
            Console.WriteLine($"Location.AddCheck({check.Id})");
            List<Check> list = GetListForCheck(check);
            if (!list.Contains(check)) {
                check.location = this;
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
        }
    }

    class Check : DependencyEntry {
        public enum CheckType {
            ITEM, POKEMON, TRADE, TRAINER
        }

        public Location location;
        public CheckType Type { get; private set; }
        private Dictionary<DependencyEntry, TreeIter> dependingEntries;
        public int DependingEntryCount => dependingEntries.Count;

        public Check(string id, RuleSet ruleSet, CheckType type) : base(id, ruleSet) {
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
        }

        public TreeIter GetDependencyIter(DependencyEntry entry) {
            return dependingEntries[entry];
        }

        public void RemoveDependingEntry(DependencyEntry entry) {
            while (dependingEntries.Remove(entry)) ;
        }
    }
}
