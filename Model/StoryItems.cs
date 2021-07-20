using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

namespace PokemonTrackerEditor.Model {
    abstract class StoryItemBase : IEquatable<StoryItemBase> {
        protected string id;
        abstract public string Id { get; set; }
        public TreeIter Iter { get; set; }

        public StoryItemBase(string id) {
            this.id = id;
        }

        abstract public void Cleanup();

        public override bool Equals(object obj) {
            return obj != null && obj is StoryItemBase other && Equals(other);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public bool Equals(StoryItemBase other) {
            return Id.Equals(other.Id);
        }

        public static int Compare(StoryItemBase a, StoryItemBase b) {
            return a != null && b != null ? string.Compare(a.Id, b.Id) : a != null ? -1 : b != null ? 1 : 0;
        }

        public static int Compare(TreeModel model, TreeIter a, TreeIter b) {
            return Compare((StoryItemBase)model.GetValue(a, 0), (StoryItemBase)model.GetValue(b, 0));
        }
    }

    class StoryItem : StoryItemBase {

        override public string Id { get => id; set { id = value; Category.Parent.RuleSet.ReportChange(); } }
        public StoryItemCategory Category { get; private set; }
        public StoryItem(string id, StoryItemCategory category) : base(id) {
            Category = category;
        }

        public override void Cleanup() {
            Category = null;
        }
    }

    class StoryItemCategory : StoryItemBase {
        override public string Id { get => id; set { id = value; Parent.RuleSet.ReportChange(); } }

        public StoryItems Parent { get; private set; }

        private List<StoryItem> items;
        private List<StoryItemCategoryCondition> conditions;

        public StoryItemCategory(string id, StoryItems parent) : base(id) {
            Parent = parent;
            items = new List<StoryItem>();
            conditions = new List<StoryItemCategoryCondition>();
        }

        public void RegisterConditions(StoryItemCategoryCondition cond) {
            conditions.Add(cond);
        }

        public void UnregisterConditions(StoryItemCategoryCondition cond) {
            conditions.Remove(cond);
        }

        public bool AddStoryItem(string storyItem) {
            StoryItem item = new StoryItem(storyItem, this);
            if (!items.Contains(item)) {
                items.Add(item);
                foreach (StoryItemCategoryCondition cond in conditions) {
                    cond.AddStoryItem(item);
                }
                Parent.RuleSet.ReportChange();
                return true;
            }
            return false;
        }

        public void RemoveStoryItem(StoryItem item) {
            foreach (StoryItemCategoryCondition cond in conditions) {
                cond.RemoveStoryItem(item);
            }
            items.Remove(item);
            item.Cleanup();
            Parent.RuleSet.ReportChange();
        }

        override public void Cleanup() {
            conditions.Clear();
            conditions = null;
            foreach (StoryItem item in items) {
                item.Cleanup();
            }
            items.Clear();
        }
    }

    class StoryItems {
        private List<StoryItemCategory> categories;
        private List<StoryItemsConditions> conditions;
        public TreeStore TreeStore { get; private set; }
        public TreeModelSort Model { get; private set; }
        public RuleSet RuleSet { get; private set; }

        public StoryItems(RuleSet ruleSet) {
            RuleSet = ruleSet;
            TreeStore = new TreeStore(typeof(StoryItemBase));
            Model = new TreeModelSort(TreeStore);
            Model.SetSortFunc(0, StoryItemBase.Compare);
            Model.SetSortColumnId(0, SortType.Ascending);
            categories = new List<StoryItemCategory>();
            conditions = new List<StoryItemsConditions>();
        }

        public void RegisterConditions(StoryItemsConditions conditionInstance) {
            if (!conditions.Contains(conditionInstance)) {
                conditions.Add(conditionInstance);
            }
        }

        public void UnregisterConditions(StoryItemsConditions conditionInstance) {
            conditions.Remove(conditionInstance);
        }

        public bool AddStoryItemCategory(string category) {
            StoryItemCategory newCat = new StoryItemCategory(category, this);
            if (!categories.Contains(newCat)) {
                categories.Add(newCat);
                foreach (StoryItemsConditions conditionInstance in conditions) {
                    conditionInstance.AddStoryItemCategory(newCat);
                }
                RuleSet.ReportChange();
                return true;
            }
            return false;
        }

        public void RemoveStoryItemCategory(StoryItemCategory category) {
            foreach (StoryItemsConditions conditionInstance in conditions) {
                conditionInstance.RemoveStoryItemCategory(category);
            }
            TreeIter iter = category.Iter;
            TreeStore.Remove(ref iter);
            categories.Remove(category);
            category.Cleanup();
            RuleSet.ReportChange();
        }

        public void Cleanup() {
            conditions.Clear();
            conditions = null;
            foreach (StoryItemCategory category in categories) {
                category.Cleanup();
            }
            categories.Clear();
            categories = null;
            TreeStore.Clear();
            Model.Dispose();
            Model = null;
            TreeStore.Dispose();
            TreeStore = null;
            RuleSet = null;
        }
    }
}
