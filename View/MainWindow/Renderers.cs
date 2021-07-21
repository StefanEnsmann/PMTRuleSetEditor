using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PokemonTrackerEditor.Model;

using Gtk;

namespace PokemonTrackerEditor.View.MainWindow {
    class Renderers {
        public static void DependencyEntryName(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep.Id;
        }

        public static void DependencyEntryCheckCount(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is Location loc ? "" + loc.CheckCount : (dep is LocationCategory locCat ? "" + locCat.CheckCount : "");
        }

        public static void DependencyEntryDepCount(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is Check check ? "" + check.DependingEntryCount : "";
        }

        public static void DependencyEntryCondCount(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is DependencyEntry entry ? "" + entry.ConditionCount : "";
        }

        public static void ConditionName(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            DependencyEntryBase cond = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = cond.Id;
        }

        public static void ConditionLocation(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            Check cond = (Check)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = cond.location.Id;
        }

        public static void StoryItemName(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            StoryItemBase storyItem = (StoryItemBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = storyItem.Id;
        }

        public static void StoryItemDependencyCount(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            StoryItemBase storyItem = (StoryItemBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = "" + storyItem.DependencyCount;
        }

        public static void StoryItemConditionName(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            StoryItemConditionBase storyItemCondition = (StoryItemConditionBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = storyItemCondition.Id;
        }

        public static void StoryItemConditionCollectionCount(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            StoryItemConditionBase storyItemCondition = (StoryItemConditionBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = storyItemCondition is StoryItemConditionCollection coll ? "" + coll.Count : "";
        }

        public static void CheckComboBoxCell(CellLayout cellLayout, CellRenderer cell, TreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep.Id;
        }

        public static void StoryItemComboBoxCell(CellLayout cellLayout, CellRenderer cell, TreeModel model, TreeIter iter) {
            StoryItemBase dep = (StoryItemBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep.Id;
        }
    }
}
