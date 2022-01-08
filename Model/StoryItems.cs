using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

using Newtonsoft.Json;

namespace PokemonTrackerEditor.Model {
    public abstract class StoryItemBase : IEquatable<StoryItemBase>, IMovable, ILocalizable {
        protected string id;
        abstract public string Id { get; set; }
        public TreeIter Iter { get; set; }

        public Localization Localization { get; set; }

        abstract public int DependencyCount { get; }

        abstract public RuleSet Rules { get; }

        public StoryItemBase(string id) {
            this.id = id;
            Localization = new Localization(this);
        }

        public abstract void InvokeRemove();

        public virtual void Cleanup() {
            Localization.Cleanup();
        }

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

    public class StoryItem : StoryItemBase {
        override public RuleSet Rules => Category.Parent.Rules;
        override public string Id { get => id; set { id = value; Category.Parent.Rules.ReportChange(); } }
        public string Path => Category.Id + "." + Id;
        public StoryItemCategory Category { get; private set; }

        private string imageURL;
        public string ImageURL { get => imageURL; set { imageURL = value; Category.Parent.Rules.ReportChange(); } }

        public List<StoryItemCondition> Dependencies { get; private set; }
        override public int DependencyCount => Dependencies.Count();
        public StoryItem(string id, StoryItemCategory category) : base(id) {
            Category = category;
            Dependencies = new List<StoryItemCondition>();
        }

        public void AddDependency(StoryItemCondition dependency) {
            if (!Dependencies.Contains(dependency)) {
                Dependencies.Add(dependency);
            }
        }

        public void RemoveDependency(StoryItemCondition dependency) {
            Dependencies.Remove(dependency);
        }

        public override void InvokeRemove() {
            Category.Parent.RemoveItemFromModel(Iter);
            Category.ChildWasRemoved(this);
            Category.Parent.Rules.ReportChange();
            Cleanup();
        }

        public override void Cleanup() {
            foreach (StoryItemCondition cond in Dependencies) {
                cond.StoryItemWasRemoved();
            }
            Dependencies.Clear();
            Dependencies = null;

            Category = null;

            base.Cleanup();
        }
    }

    public class StoryItemCategory : StoryItemBase, IMovableItems<StoryItem> {
        override public RuleSet Rules => Parent.Rules;
        override public string Id { get => id; set { id = value; Parent.Rules.ReportChange(); } }

        public StoryItems Parent { get; private set; }

        public List<StoryItem> Items { get; set; }
        public override int DependencyCount {
            get {
                int ret = 0;
                foreach (StoryItem item in Items) {
                    ret += item.DependencyCount;
                }
                return ret;
            }
        }

        public StoryItemCategory(string id, StoryItems parent) : base(id) {
            Parent = parent;
            Items = new List<StoryItem>();
        }

        public StoryItem FindStoryItem(string item) {
            foreach (StoryItem it in Items) {
                if (it.Id.Equals(item)) {
                    return it;
                }
            }
            return null;
        }

        public bool StoryItemNameAvailable(string name) {
            return FindStoryItem(name) != null;
        }

        public StoryItem AddStoryItem(string storyItem) {
            if (StoryItemNameAvailable(storyItem)) {
                StoryItem item = new StoryItem(storyItem, this);
                Items.Add(item);
                Parent.MergeItemToModel(item);
                Parent.Rules.ReportChange();
                return item;
            }
            else {
                return null;
            }
        }

        public override void InvokeRemove() {
            Parent.RemoveItemFromModel(Iter);
            Parent.ChildWasRemoved(this);
            Parent.Rules.ReportChange();
            Cleanup();
        }

        public void ChildWasRemoved(StoryItem item) {
            Items.Remove(item);
        }

        public bool MoveUp(StoryItem item) {
            return InterfaceHelpers.SwapItems(Items, item, true);
        }

        public bool MoveDown(StoryItem item) {
            return InterfaceHelpers.SwapItems(Items, item, false);
        }

        public override void Cleanup() {
            foreach (StoryItem item in Items) {
                item.Cleanup();
            }
            Items.Clear();

            Items = null;
            Parent = null;
        }
    }

    [JsonConverter(typeof(StoryItemConverter))]
    public class StoryItems : IMovableItems<StoryItemCategory> {
        public List<StoryItemCategory> Categories { get; private set; }
        public TreeStore Model { get; private set; }
        public RuleSet Rules { get; private set; }

        public StoryItems(RuleSet ruleSet) {
            Rules = ruleSet;
            Model = new TreeStore(typeof(StoryItemBase));
            Categories = new List<StoryItemCategory>();
        }

        public StoryItemCategory FindStoryItemCategory(string category) {
            foreach (StoryItemCategory cat in Categories) {
                if (cat.Id.Equals(category)) {
                    return cat;
                }
            }
            return null;
        }

        public StoryItem FindStoryItem(string category, string item) {
            return FindStoryItemCategory(category)?.FindStoryItem(item);
        }

        public bool CategoryNameAvailable(string name) {
            foreach (StoryItemCategory category in Categories) {
                if (category.Id.Equals(name))
                    return false;
            }
            return true;
        }

        public StoryItemCategory AddStoryItemCategory(string category) {
            if (CategoryNameAvailable(category)) {
                StoryItemCategory newCat = new StoryItemCategory(category, this);
                Categories.Add(newCat);
                Rules.ReportChange();
                return newCat;
            }
            else {
                return null;
            }
        }

        public void ChildWasRemoved(StoryItemCategory category) {
            Categories.Remove(category);
        }

        public void MergeItemToModel(StoryItemBase item) {
            if (item is StoryItemCategory) {
                item.Iter = Model.AppendValues(item);
            }
            else if (item is StoryItem storyItem) {
                storyItem.Iter = Model.AppendValues(storyItem.Category.Iter, storyItem);
            }
        }

        public void RemoveItemFromModel(TreeIter iter) {
            Model.Remove(ref iter);
        }

        public bool MoveUp(StoryItemCategory category) {
            return InterfaceHelpers.SwapItems(Categories, category, true);
        }

        public bool MoveDown(StoryItemCategory category) {
            return InterfaceHelpers.SwapItems(Categories, category, false);
        }

        public void Cleanup() {
            foreach (StoryItemCategory category in Categories) {
                category.Cleanup();
            }
            Categories.Clear();
            Categories = null;

            Model.Clear();
            Model.Dispose();
            Model = null;

            Rules = null;
        }
    }
}
