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
        public Localization Localization { get; set; }
        public Conditions Conditions { get; set; }

        public ILocationContainer Parent { get; private set; }
        public override RuleSet Rules => Parent.Rules;

        public abstract string Path { get; }

        public DependencyEntry(string id, ILocationContainer parent) : base(id) {
            Parent = parent;
            Localization = new Localization(this);
            Conditions = new Conditions(this);
        }

        virtual public void Cleanup() {
            Conditions.InvokeRemove();
            Localization.Cleanup();
            Parent = null;
        }

        virtual public void InvokeRemove() {
            Rules.RemoveDependencyIter(Iter);
            Parent.ChildWasRemoved(this);
            Rules.ReportChange();
            Cleanup();
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

        public override string Path => this.LocationPath();

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
    public class Check : DependencyEntry, IConditionable {
        public enum Type {
            ITEM, POKEMON, TRADE, TRAINER
        }

        public Type CheckType { get; private set; }

        private readonly List<Condition> dependingEntries;
        public int DependingEntryCount => dependingEntries.Count;
        public override string Path => (Parent as Location).LocationPath() + "." + CheckType.ToString().ToLower() + (CheckType == Type.POKEMON ? "." : "s.") + Id;

        public Check(string id, Type type, Location parent) : base(id, parent) {
            CheckType = type;
            dependingEntries = new List<Condition>();
        }

        public override void Cleanup() {
            foreach (Condition dependingEntry in dependingEntries) {
                dependingEntry.ConditionableWasRemoved();
            }
            dependingEntries.Clear();
            base.Cleanup();
        }

        public void AddDependency(Condition condition) {
            dependingEntries.Add(condition);
            Rules.ReportChange();
        }

        public void RemoveDependency(Condition condition) {
            while (dependingEntries.Remove(condition)) ;
            Rules.ReportChange();
        }
    }
}
