using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {
    abstract class StoryItemConditionBase : IEquatable<StoryItemConditionBase> {
        virtual public string Id { get; protected set; }
        public TreeIter Iter { get; set; }

        public StoryItemConditionCollection Container { get; private set; }

        public StoryItemConditionBase(StoryItemConditionCollection container) {
            Container = container;
        }

        public virtual void Cleanup() {
            Container = null;
        }

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

    [JsonConverter(typeof(StoryItemConditionConverter))]
    class StoryItemCondition : StoryItemConditionBase {
        override public string Id => StoryItem.Category.Id + "/" + StoryItem.Id;

        public StoryItem StoryItem { get; private set; }

        public StoryItemCondition(StoryItem storyItem, StoryItemConditionCollection container) : base(container) {
            StoryItem = storyItem;
            StoryItem.AddDependency(this);
        }

        public override void Cleanup() {
            base.Cleanup();
            StoryItem.RemoveDependency(this);
            StoryItem = null;
        }
    }

    abstract class StoryItemConditionCollection : StoryItemConditionBase {
        protected List<StoryItemConditionBase> conditions;
        public List<StoryItemConditionBase> Conditions => new List<StoryItemConditionBase>(conditions);
        public virtual bool CanAddCondition => true;
        public virtual string Type => "NONE";
        public int Count {
            get {
                int ret = 0;
                foreach (StoryItemConditionBase cond in conditions) {
                    if (cond is StoryItemConditionCollection cont) {
                        ret += cont.Count;
                    }
                    else {
                        ++ret;
                    }
                }
                return ret;
            }
        }

        public StoryItemConditionCollection(string id, StoryItemConditionCollection container) : base(container) {
            Id = id;
            conditions = new List<StoryItemConditionBase>();
        }

        public void AddStoryItemCondition(StoryItem storyItem) {
            if (CanAddCondition) {
                StoryItemCondition cond = new StoryItemCondition(storyItem, this);
                if (!conditions.Contains(cond)) {
                    conditions.Add(cond);
                    InsertConditionToTree(cond, Iter);
                }
                else {
                    cond.Cleanup();
                }
            }
        }

        public void AddANDCollection() {
            if (CanAddCondition) {
                string template = "AND ";
                int value = 0;
                StoryItemANDCondition cond = new StoryItemANDCondition(template + value, this);
                while (conditions.Contains(cond)) {
                    ++value;
                    cond = new StoryItemANDCondition(template + value, this);
                }
                conditions.Add(cond);
                InsertConditionToTree(cond, Iter);
            }
        }

        public void AddORCollection() {
            if (CanAddCondition) {
                string template = "OR ";
                int value = 0;
                StoryItemORCondition cond = new StoryItemORCondition(template + value, this);
                while (conditions.Contains(cond)) {
                    ++value;
                    cond = new StoryItemORCondition(template + value, this);
                }
                conditions.Add(cond);
                InsertConditionToTree(cond, Iter);
            }
        }

        public void AddNOTCollection() {
            if (CanAddCondition) {
                string template = "NOT ";
                int value = 0;
                StoryItemNOTCondition cond = new StoryItemNOTCondition(template + value, this);
                while (conditions.Contains(cond)) {
                    ++value;
                    cond = new StoryItemNOTCondition(template + value, this);
                }
                conditions.Add(cond);
                InsertConditionToTree(cond, Iter);
            }
        }

        virtual protected void ReportChange() {
            Container.ReportChange();
        }

        public void RemoveStoryItemCondition(StoryItemConditionBase cond) {
            conditions.Remove(cond);
            RemoveConditionFromTree(cond);
            cond.Cleanup();
            if (conditions.Count == 0) {
                if (Container != null) {
                    Container.RemoveStoryItemCondition(this);
                }
            }
        }

        virtual protected void InsertConditionToTree(StoryItemConditionBase cond, TreeIter iter) {
            Container.InsertConditionToTree(cond, iter);
        }

        virtual protected void RemoveConditionFromTree(StoryItemConditionBase cond) {
            Container.RemoveConditionFromTree(cond);
        }

        public override void Cleanup() {
            base.Cleanup();
            foreach (StoryItemConditionBase cond in conditions) {
                cond.Cleanup();
            }
            conditions.Clear();
            conditions = null;
        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    class StoryItemANDCondition : StoryItemConditionCollection {
        public override string Type => "AND";
        public StoryItemANDCondition(string id, StoryItemConditionCollection container) : base(id, container) {

        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    class StoryItemORCondition : StoryItemConditionCollection {
        public override string Type => "OR";
        public StoryItemORCondition(string id, StoryItemConditionCollection container) : base(id, container) {

        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    class StoryItemNOTCondition : StoryItemConditionCollection {
        public override string Type => "NOT";
        public override bool CanAddCondition => conditions.Count == 0;
        public StoryItemNOTCondition(string id, StoryItemConditionCollection container) : base(id, container) {

        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    class StoryItemsConditions : StoryItemConditionCollection {
        public DependencyEntry Entry { get; private set; }
        public TreeStore TreeStore { get; private set; }
        public TreeModelSort Model { get; private set; }

        public StoryItemsConditions(DependencyEntry entry) : base("Conditions", null) {
            Entry = entry;
            TreeStore = new TreeStore(typeof(StoryItemConditionBase));
            Model = new TreeModelSort(TreeStore);
            Model.SetSortFunc(0, Compare);
            Model.SetSortColumnId(0, SortType.Ascending);
            Iter = TreeStore.AppendValues(this);
        }

        protected override void ReportChange() {
            Entry.RuleSet.ReportChange();
        }

        protected override void InsertConditionToTree(StoryItemConditionBase cond, TreeIter iter) {
            cond.Iter = TreeStore.AppendValues(iter, cond);
        }

        protected override void RemoveConditionFromTree(StoryItemConditionBase cond) {
            TreeIter iter = cond.Iter;
            TreeStore.Remove(ref iter);
        }

        public void RemoveAll() {
            StoryItemConditionBase[] copy = new StoryItemConditionBase[conditions.Count];
            conditions.CopyTo(copy);
            foreach (StoryItemConditionBase cond in copy) {
                RemoveStoryItemCondition(cond);
            }
        }

        override public void Cleanup() {
            base.Cleanup();
            Entry = null;
            TreeStore.Clear();
            Model.Dispose();
            Model = null;
            TreeStore.Dispose();
            TreeStore = null;
        }
    }
}
