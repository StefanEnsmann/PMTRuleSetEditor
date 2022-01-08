using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {

    [JsonConverter(typeof(StoryItemConditionConverter))]
    abstract public class StoryItemConditionBase : IEquatable<StoryItemConditionBase> {
        virtual public string Id { get; protected set; }
        public TreeIter Iter { get; set; }

        public virtual StoryItemConditionCollection Container { get; private set; }

        public StoryItemConditionBase(StoryItemConditionCollection container) {
            Container = container;
        }

        public virtual void Cleanup() {
            Container = null;
        }

        public virtual void InvokeRemove() {
            Container.RemoveConditionFromTree(this);
            Container.ChildWasRemoved(this);
            Container.Rules.ReportChange();
            Cleanup();
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
    public class StoryItemCondition : StoryItemConditionBase {
        override public string Id => StoryItem.Path;

        public StoryItem StoryItem { get; private set; }

        public StoryItemCondition(StoryItem storyItem, StoryItemConditionCollection container) : base(container) {
            StoryItem = storyItem;
            StoryItem.AddDependency(this);
        }

        public void StoryItemWasRemoved() {
            base.InvokeRemove();
        }

        public override void Cleanup() {
            StoryItem.RemoveDependency(this);
            StoryItem = null;
            base.Cleanup();
        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    abstract public class StoryItemConditionCollection : StoryItemConditionBase {
        public List<StoryItemConditionBase> Conditions { get; private set; }
        public virtual bool CanAddCondition => true;
        public virtual string Type => "NONE";

        public virtual RuleSet Rules => Container.Rules;
        public int Count {
            get {
                int ret = 0;
                foreach (StoryItemConditionBase cond in Conditions) {
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
            Conditions = new List<StoryItemConditionBase>();
        }

        public override void Cleanup() {
            foreach (StoryItemConditionBase cond in Conditions) {
                cond.Cleanup();
            }

            Conditions.Clear();
            Conditions = null;

            base.Cleanup();
        }

        public bool ConditionNameAvailable(string name) {
            foreach (StoryItemConditionBase condition in Conditions) {
                if (condition.Id.Equals(name)) {
                    return false;
                }
            }
            return true;
        }

        public StoryItemCondition AddStoryItemCondition(StoryItem storyItem) {
            if (CanAddCondition && ConditionNameAvailable(storyItem.Path)) {
                StoryItemCondition cond = new StoryItemCondition(storyItem, this);
                Conditions.Add(cond);
                InsertConditionToTree(cond, Iter);
                Rules.ReportChange();
                return cond;
            }
            else {
                return null;
            }
        }

        private StoryItemConditionCollection AddCollection(string type)  {
            if (CanAddCondition) {
                type = type.Trim() + " ";
                int value = 0;
                while (!ConditionNameAvailable(type + value)) {
                    ++value;
                }
                StoryItemConditionCollection cond;
                switch (type) {
                    case "AND ":
                        cond = new StoryItemANDCondition(type + value, this);
                        break;
                    case "OR ":
                        cond = new StoryItemORCondition(type + value, this);
                        break;
                    case "NOT ":
                        cond = new StoryItemNOTCondition(type + value, this);
                        break;
                    default:
                        cond = null;
                        break;
                }
                Conditions.Add(cond);
                InsertConditionToTree(cond, Iter);
                Rules.ReportChange();
                return cond;
            }
            else {
                return null;
            }
        }

        public StoryItemANDCondition AddANDCollection() {
            return AddCollection("AND") as StoryItemANDCondition;
        }

        public StoryItemORCondition AddORCollection() {
            return AddCollection("OR") as StoryItemORCondition;
        }

        public StoryItemNOTCondition AddNOTCollection() {
            return AddCollection("NOT") as StoryItemNOTCondition;
        }

        public void ChildWasRemoved(StoryItemConditionBase item) {
            if (item != this) {
                Conditions.Remove(item);
            }
        }

        virtual public void InsertConditionToTree(StoryItemConditionBase cond, TreeIter iter) {
            Container.InsertConditionToTree(cond, iter);
        }

        virtual public void RemoveConditionFromTree(StoryItemConditionBase cond) {
            Container.RemoveConditionFromTree(cond);
        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    public class StoryItemANDCondition : StoryItemConditionCollection {
        public override string Type => "AND";
        public StoryItemANDCondition(string id, StoryItemConditionCollection container) : base(id, container) {

        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    public class StoryItemORCondition : StoryItemConditionCollection {
        public override string Type => "OR";
        public StoryItemORCondition(string id, StoryItemConditionCollection container) : base(id, container) {

        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    public class StoryItemNOTCondition : StoryItemConditionCollection {
        public override string Type => "NOT";
        public override bool CanAddCondition => Conditions.Count == 0;
        public StoryItemNOTCondition(string id, StoryItemConditionCollection container) : base(id, container) {

        }
    }

    [JsonConverter(typeof(StoryItemConditionConverter))]
    public class StoryItemsConditions : StoryItemConditionCollection {
        public DependencyEntry Entry { get; private set; }
        public TreeStore TreeStore { get; private set; }
        public TreeModelSort Model { get; private set; }
        public override RuleSet Rules => Entry.Rules;
        public override StoryItemConditionCollection Container => this;

        public StoryItemsConditions(DependencyEntry entry) : base("Conditions", null) {
            Entry = entry;
            TreeStore = new TreeStore(typeof(StoryItemConditionBase));
            Model = new TreeModelSort(TreeStore);
            Model.SetSortFunc(0, Compare);
            Model.SetSortColumnId(0, SortType.Ascending);
            Iter = TreeStore.AppendValues(this);
        }

        public override void InsertConditionToTree(StoryItemConditionBase cond, TreeIter iter) {
            cond.Iter = TreeStore.AppendValues(iter, cond);
        }

        public override void RemoveConditionFromTree(StoryItemConditionBase cond) {
            TreeIter iter = cond.Iter;
            TreeStore.Remove(ref iter);
        }

        override public void Cleanup() {
            Entry = null;

            Model.Dispose();
            Model = null;

            TreeStore.Clear();
            TreeStore.Dispose();
            TreeStore = null;

            base.Cleanup();
        }
    }
}
