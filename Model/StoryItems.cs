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

        abstract public int DependencyCount { get; }

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
        private List<StoryItemCondition> dependencies;
        override public int DependencyCount => dependencies.Count();
        public StoryItem(string id, StoryItemCategory category) : base(id) {
            Category = category;
            dependencies = new List<StoryItemCondition>();
        }

        public void AddDependency(StoryItemCondition dependency) {
            if (!dependencies.Contains(dependency)) {
                dependencies.Add(dependency);
            }
        }

        public void RemoveDependency(StoryItemCondition dependency) {
            dependencies.Remove(dependency);
        }

        public override void Cleanup() {
            foreach (StoryItemCondition cond in dependencies) {
                cond.Cleanup();
            }
            dependencies.Clear();
            dependencies = null;
            Category = null;
        }
    }

    class StoryItemCategory : StoryItemBase {
        override public string Id { get => id; set { id = value; Parent.RuleSet.ReportChange(); } }

        public StoryItems Parent { get; private set; }

        private List<StoryItem> items;
        public override int DependencyCount {
            get {
                int ret = 0;
                foreach (StoryItem item in items) {
                    ret += item.DependencyCount;
                }
                return ret;
            }
        }

        public StoryItemCategory(string id, StoryItems parent) : base(id) {
            Parent = parent;
            items = new List<StoryItem>();
        }

        public bool StoryItemNameAvailable(string name) {
            foreach(StoryItem item in items) {
                if (item.Id.Equals(name))
                    return false;
            }
            return true;
        }

        public bool AddStoryItem(string storyItem) {
            StoryItem item = new StoryItem(storyItem, this);
            if (!items.Contains(item)) {
                items.Add(item);
                item.Iter = Parent.Model.AppendValues(Iter, item);
                Parent.RuleSet.ReportChange();
                return true;
            }
            return false;
        }

        public void RemoveStoryItem(StoryItem item) {
            TreeIter iter = item.Iter;
            Parent.Model.Remove(ref iter);
            items.Remove(item);
            item.Cleanup();
            Parent.RuleSet.ReportChange();
        }

        override public void Cleanup() {
            foreach (StoryItem item in items) {
                item.Cleanup();
            }
            items.Clear();
        }
    }

    class StoryItems {
        private List<StoryItemCategory> categories;
        public TreeStore Model { get; private set; }
        public RuleSet RuleSet { get; private set; }

        public StoryItems(RuleSet ruleSet) {
            RuleSet = ruleSet;
            Model = new TreeStore(typeof(StoryItemBase));
            categories = new List<StoryItemCategory>();
        }

        public bool CategoryNameAvailable(string name) {
            foreach (StoryItemCategory category in categories) {
                if (category.Id.Equals(name))
                    return false;
            }
            return true;
        }

        public bool AddStoryItemCategory(string category) {
            StoryItemCategory newCat = new StoryItemCategory(category, this);
            if (!categories.Contains(newCat)) {
                newCat.Iter = Model.AppendValues(newCat);
                categories.Add(newCat);
                RuleSet.ReportChange();
                return true;
            }
            return false;
        }

        public void RemoveStoryItemCategory(StoryItemCategory category) {
            TreeIter iter = category.Iter;
            Model.Remove(ref iter);
            categories.Remove(category);
            category.Cleanup();
            RuleSet.ReportChange();
        }

        public void Cleanup() {
            foreach (StoryItemCategory category in categories) {
                category.Cleanup();
            }
            categories.Clear();
            categories = null;
            Model.Clear();
            Model.Dispose();
            Model = null;
            RuleSet = null;
        }
    }
}
