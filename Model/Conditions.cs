using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

using Newtonsoft.Json;

namespace PokemonTrackerEditor.Model {
    public interface IConditionable {
        string Path { get; }
        void AddDependency(Condition condition);
        void RemoveDependency(Condition condition);
    }

    [JsonConverter(typeof(ConditionsConverter))]
    abstract public class ConditionBase : IEquatable<ConditionBase> {
        public virtual ConditionCollection Parent { get; private set; }
        public virtual RuleSet Rules => Parent.Rules;
        public virtual TreeIter Iter { get; set; }

        abstract public string Path { get; }

        public ConditionBase(ConditionCollection parent) {
            Parent = parent;
        }

        public virtual void Cleanup() {
            Parent = null;
        }

        public virtual void InvokeRemove() {
            Parent.RemoveConditionFromTree(this);
            Parent.ChildWasRemoved(this);
            Parent.Rules.ReportChange();
            Cleanup();
        }

        public override int GetHashCode() {
            return Path.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj != null && this == obj;
        }

        public bool Equals(ConditionBase conditionBase) {
            return this == conditionBase;
        }

        public IConditionable ResolvePath(string path) {
            List<string> segments = path.Split('.').ToList();
            string first = segments.First();
            segments.RemoveAt(0);
            if (first.Equals("locations")) {
                return Rules.ResolvePath(segments) as Check;
            }
            else if (first.Equals("story_items")) {
                return Rules.StoryItems.FindStoryItem(segments[0], segments[1]);
            }
            else {
                throw new ArgumentException("Could not resolve path: " + path);
            }
        }
    }

    [JsonConverter(typeof(ConditionsConverter))]
    public class Condition : ConditionBase {
        public IConditionable Conditionable { get; private set; }
        public string Id => Conditionable?.Path;
        public override string Path => Parent.Path + "." + Id;

        public Condition(IConditionable conditionable, ConditionCollection parent) : base(parent) {
            Conditionable = conditionable;
            Conditionable.AddDependency(this);
        }

        public void ConditionableWasRemoved() {
            base.InvokeRemove();
        }

        public override void Cleanup() {
            Conditionable.RemoveDependency(this);
            Conditionable = null;
            base.Cleanup();
        }
    }

    [JsonConverter(typeof(ConditionsConverter))]
    public class ConditionCollection : ConditionBase {
        public enum LogicalType {
            AND, OR, NOT
        }

        public List<ConditionBase> Conditions;
        public LogicalType Type { get; set; }
        public int Index { get; private set; }
        public bool CanAddCondition => !(Type == LogicalType.NOT && Conditions.Count > 0);
        public override string Path => Parent.Path + "." + Index;

        public int Count {
            get {
                int c = 0;
                foreach (ConditionBase conditionBase in Conditions) {
                    if (conditionBase is Condition) {
                        ++c;
                    }
                    else if (conditionBase is ConditionCollection collection) {
                        c += collection.Count;
                    }
                }
                return c;
            }
        }


        public ConditionCollection(LogicalType type, int index, ConditionCollection parent) : base(parent) {
            Index = index;
            Conditions = new List<ConditionBase>();
            Type = type;
        }

        public bool ConditionNameAvailable(string name) {
            foreach (ConditionBase conditionBase in Conditions) {
                if (conditionBase is Condition condition && condition.Id.Equals(name)) {
                    return false;
                }
            }
            return true;
        }

        public Condition AddCondition(IConditionable conditionable) {
            if (CanAddCondition && ConditionNameAvailable(conditionable.Path)) {
                Condition condition = new Condition(conditionable, this);
                Conditions.Add(condition);
                InsertConditionToTree(condition, Iter);
                Rules.ReportChange();
                return condition;
            }
            else {
                return null;
            }
        }

        public ConditionCollection AddCollection(LogicalType type) {
            if (CanAddCondition) {
                int idx = -1;
                bool blocked = true;
                while (blocked) {
                    ++idx;
                    blocked = false;
                    foreach (ConditionBase conditionBase in Conditions) {
                        if (conditionBase is ConditionCollection conditionCollection && conditionCollection.Type == type && conditionCollection.Index == idx) {
                            blocked = true;
                            break;
                        }
                    }
                }
                ConditionCollection collection = new ConditionCollection(type, idx, this);
                Conditions.Add(collection);
                InsertConditionToTree(collection, Iter);
                Rules.ReportChange();
                return collection;
            }
            else {
                return null;
            }
        }

        public virtual void InsertConditionToTree(ConditionBase condition, TreeIter parent) {
            Parent.InsertConditionToTree(condition, parent);
        }

        public virtual void RemoveConditionFromTree(ConditionBase condition) {
            Parent.RemoveConditionFromTree(condition);
        }

        public void ChildWasRemoved(ConditionBase condition) {
            if (condition != this) {
                Console.WriteLine(Conditions.Remove(condition));
            }
        }

        public override void Cleanup() {
            foreach (ConditionBase condition in Conditions) {
                condition.Cleanup();
            }
            Conditions.Clear();
            Conditions = null;

            base.Cleanup();
        }
    }

    [JsonConverter(typeof(ConditionsConverter))]
    public class Conditions : ConditionCollection {
        public TreeStore Model { get; private set; }

        public DependencyEntry Entry { get; private set; }

        public override RuleSet Rules => Entry.Rules;
        public override ConditionCollection Parent => this;
        public override string Path => "Base";

        public Conditions(DependencyEntry entry) : base(LogicalType.AND, 0, null) {
            Entry = entry;
            Model = new TreeStore(typeof(ConditionBase));
            Iter = Model.AppendValues(this);
        }

        public override void InsertConditionToTree(ConditionBase condition, TreeIter parent) {
           condition.Iter = Model.AppendValues(parent, condition);
        }

        public override void RemoveConditionFromTree(ConditionBase condition) {
            TreeIter iter = condition.Iter;
            Model.Remove(ref iter);
        }

        public override void Cleanup() {
            Entry = null;

            Model.Dispose();
            Model = null;

            base.Cleanup();
        }
    }
}
