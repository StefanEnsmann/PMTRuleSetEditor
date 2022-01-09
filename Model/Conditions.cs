using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

namespace PokemonTrackerEditor.Model {
    public interface IConditionable {
        string Path { get; }
        void AddDependency(Condition condition);
        void RemoveDependency(Condition condition);
    }

    abstract public class ConditionBase : IEquatable<ConditionBase> {
        public virtual ConditionCollection Parent { get; private set; }
        public virtual RuleSet Rules => Parent.Rules;
        public virtual TreeIter Iter { get; set; }

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

        public abstract override int GetHashCode();

        public abstract string GetHashableString();

        public override bool Equals(object obj) {
            return obj != null && obj is ConditionBase conditionBase && Equals(conditionBase);
        }

        public abstract bool Equals(ConditionBase conditionBase);
    }

    public class Condition : ConditionBase {
        public IConditionable Conditionable { get; private set; }
        public string Id => Conditionable?.Path;

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

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public override string GetHashableString() {
            return Id;
        }

        public override bool Equals(ConditionBase conditionBase) {
            return conditionBase != null && conditionBase is Condition condition && Id.Equals(condition.Id);
        }
    }

    public class ConditionCollection : ConditionBase {
        public enum LogicalType {
            AND, OR, NOT
        }

        public List<ConditionBase> Conditions;
        public LogicalType Type { get; private set; }
        public int Index { get; private set; }
        public bool CanAddCondition => !(Type == LogicalType.NOT && Conditions.Count > 0);


        public ConditionCollection(LogicalType type, ConditionCollection parent) : base(parent) {
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
                ConditionCollection collection = new ConditionCollection(type, this);
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
                Conditions.Remove(condition);
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

        public override int GetHashCode() {
            return GetHashableString().GetHashCode();
        }

        public override string GetHashableString() {
            string ret = Index.ToString();
            foreach (ConditionBase condition in Conditions) {
                ret += condition.GetHashableString();
            }
            return ret;
        }

        public override bool Equals(ConditionBase conditionBase) {
            if (conditionBase != null && conditionBase is ConditionCollection conditionCollection && conditionCollection.Conditions.Count == Conditions.Count) {
                for (int i = 0; i < Conditions.Count; ++i) {
                    if (!Conditions[i].Equals(conditionCollection.Conditions[i])) {
                        return false;
                    }
                }
                return true;
            }
            else {
                return false;
            }
        }
    }

    public class Conditions : ConditionCollection {
        public TreeStore Model { get; private set; }

        public DependencyEntry Entry { get; private set; }

        public override RuleSet Rules => Entry.Rules;
        public override ConditionCollection Parent => this;

        public override TreeIter Iter => TreeIter.Zero;

        public Conditions(DependencyEntry entry) : base(LogicalType.AND, null) {
            Entry = entry;
            Model = new TreeStore(typeof(ConditionBase));
        }

        public override void InsertConditionToTree(ConditionBase condition, TreeIter parent) {
            if (parent.Equals(TreeIter.Zero)) {
                Model.AppendValues(condition);
            }
            else {
                Model.AppendValues(condition, parent);
            }
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
