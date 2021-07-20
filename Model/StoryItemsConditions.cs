using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

namespace PokemonTrackerEditor.Model {
    abstract class StoryItemConditionBase : IEquatable<StoryItemConditionBase> {
        abstract public string Id { get; }
        abstract public bool Active { get; set; }
        public TreeIter Iter { get; set; }

        abstract public void Cleanup();

        public override bool Equals(object obj) {
            return obj != null && obj is StoryItemConditionBase other && Equals(other);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public bool Equals(StoryItemConditionBase other) {
            return Id.Equals(other.Id);
        }

        public static int Compare(StoryItemConditionBase a, StoryItemConditionBase b) {
            return a != null && b != null ? string.Compare(a.Id, b.Id) : a != null ? -1 : b != null ? 1 : 0;
        }

        public static int Compare(TreeModel model, TreeIter a, TreeIter b) {
            return Compare((StoryItemConditionBase)model.GetValue(a, 0), (StoryItemConditionBase)model.GetValue(b, 0));
        }
    }

    class StoryItemCondition : StoryItemConditionBase {
        override public string Id => StoryItem.Id;

        private bool active;
        override public bool Active { get => active; set { active = value; Category.Parent.StoryItems.RuleSet.ReportChange(); } }

        public StoryItem StoryItem { get; private set; }
        public StoryItemCategoryCondition Category { get; private set; }

        public StoryItemCondition(StoryItem storyItem, StoryItemCategoryCondition category) {
            active = false;
            StoryItem = storyItem;
            Category = category;
        }

        public override void Cleanup() {
            StoryItem = null;
            Category = null;
        }
    }

    class StoryItemCategoryCondition : StoryItemConditionBase {
        override public string Id => StoryItemCategory.Id;

        override public bool Active {
            get {
                foreach (StoryItemCondition cond in storyItemConditions)
                    if (!cond.Active)
                        return false;
                return true;
            }
            set {
                foreach (StoryItemCondition cond in storyItemConditions)
                    cond.Active = value;
                Parent.StoryItems.RuleSet.ReportChange();
            }
        }

        public int CountActive {
            get {
                int ret = 0;
                foreach (StoryItemCondition cond in storyItemConditions) {
                    if (cond.Active)
                        ++ret;
                }
                return ret;
            }
        }

        public StoryItemsConditions Parent { get; private set; }
        public StoryItemCategory StoryItemCategory { get; private set; }
        private List<StoryItemCondition> storyItemConditions;

        public StoryItemCategoryCondition(StoryItemCategory storyItemCategory, StoryItemsConditions parent) {
            Parent = parent;
            StoryItemCategory = storyItemCategory;
            storyItemConditions = new List<StoryItemCondition>();
        }

        public void AddStoryItem(StoryItem storyItem) {
            StoryItemCondition cond = new StoryItemCondition(storyItem, this);
            Parent.TreeStore.AppendValues(Iter, cond);
            storyItemConditions.Add(cond);
        }

        public void RemoveStoryItem(StoryItem storyItem) {
            StoryItemCondition fittingCond = null;
            foreach (StoryItemCondition cond in storyItemConditions) {
                if (cond.StoryItem == storyItem) {
                    fittingCond = cond;
                    break;
                }
            }
            if (fittingCond != null) {
                TreeIter iter = fittingCond.Iter;
                Parent.TreeStore.Remove(ref iter);
                fittingCond.Cleanup();
                storyItemConditions.Remove(fittingCond);
            }
            else {
                Console.WriteLine($"NO CONDITIONS FOUND FOR STORY ITEM WITH ID {storyItem.Id}");
            }
        }

        public override void Cleanup() {
            foreach (StoryItemCondition cond in storyItemConditions) {
                cond.Cleanup();
            }
            storyItemConditions.Clear();
            storyItemConditions = null;
            StoryItemCategory = null;
        }
    }

    class StoryItemsConditions {
        private List<StoryItemCategoryCondition> categories;
        public StoryItems StoryItems { get; private set; }
        public TreeStore TreeStore { get; set; }
        public TreeModelSort Model { get; private set; }
        public int CountActive {
            get {
                int ret = 0;
                foreach (StoryItemCategoryCondition cond in categories) {
                    ret += cond.CountActive;
                }
                return ret;
            }
        }

        public StoryItemsConditions(StoryItems storyItems) {
            categories = new List<StoryItemCategoryCondition>();
            StoryItems = storyItems;
            StoryItems.RegisterConditions(this);
            TreeStore = new TreeStore(typeof(StoryItemConditionBase));
            Model = new TreeModelSort(TreeStore);
        }

        public void AddStoryItemCategory(StoryItemCategory category) {
            StoryItemCategoryCondition cond = new StoryItemCategoryCondition(category, this);
            cond.Iter = TreeStore.AppendValues(cond);
            categories.Add(cond);
        }

        public void RemoveStoryItemCategory(StoryItemCategory category) {
            StoryItemCategoryCondition fittingCond = null;
            foreach (StoryItemCategoryCondition cond in categories) {
                if (cond.StoryItemCategory == category) {
                    fittingCond = cond;
                    break;
                }
            }
            if (fittingCond != null) {
                TreeIter iter = fittingCond.Iter;
                TreeStore.Remove(ref iter);
                fittingCond.Cleanup();
                categories.Remove(fittingCond);
            }
            else {
                Console.WriteLine($"NO CONDITIONS FOUND FOR STORY ITEMS CATEGORY WITH ID {category.Id}");
            }
        }

        public void Cleanup() {
            foreach (StoryItemCategoryCondition cond in categories) {
                cond.Cleanup();
            }
            categories.Clear();
            categories = null;
            StoryItems = null;
            TreeStore.Clear();
            Model.Dispose();
            Model = null;
            TreeStore.Dispose();
            TreeStore = null;
        }
    }
}
