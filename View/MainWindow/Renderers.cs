using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PokemonTrackerEditor.Model;

using Gtk;

namespace PokemonTrackerEditor.View.MainWindow {
    class Renderers {
        public static void DependencyEntryName(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep.Id;
        }

        public static void DependencyEntryCheckCount(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is Location loc ? "" + loc.CheckCount : (dep is LocationCategory locCat ? "" + locCat.CheckCount : "");
        }

        public static void DependencyEntryDepCount(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is Check check ? "" + check.DependingEntryCount : "";
        }

        public static void DependencyEntryCondCount(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is DependencyEntry entry ? "" + entry.Conditions.Count : "";
        }

        public static void StoryItemName(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            StoryItemBase storyItem = (StoryItemBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = storyItem.Id;
        }

        public static void StoryItemDependencyCount(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            StoryItemBase storyItem = (StoryItemBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = "" + storyItem.DependencyCount;
        }

        public static void ConditionName(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            ConditionBase conditionBase = (ConditionBase)model.GetValue(iter, 0);
            if (conditionBase is Condition condition) {
                (cell as CellRendererText).Text = condition.Id;
            }
            else if (conditionBase is ConditionCollection collection) {
                (cell as CellRendererText).Text = collection.Type.ToString();
            }
        }

        public static void CheckComboBoxCell(ICellLayout cellLayout, CellRenderer cell, ITreeModel model, TreeIter iter) {
            DependencyEntryBase dep = (DependencyEntryBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep.Id;
        }

        public static void StoryItemComboBoxCell(ICellLayout cellLayout, CellRenderer cell, ITreeModel model, TreeIter iter) {
            StoryItemBase dep = (StoryItemBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep.Id;
        }

        public static void LocalizationCodeCell(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            LocalizationEntry entry = (LocalizationEntry)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = entry.Code;
        }

        public static void LocalizationValueCell(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            LocalizationEntry entry = (LocalizationEntry)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = entry.Value;
        }

        public static void PokedexIndexCell(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            PokedexRules.Entry entry = (PokedexRules.Entry)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = entry.Nr.ToString("D4");
        }

        public static void PokedexNameCell(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            PokedexRules.Entry entry = (PokedexRules.Entry)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = entry.Data.Localization.TryGetValue("en", out string name) ? name : "MISSING";
        }

        public static void PokedexAvailableCell(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) {
            PokedexRules.Entry entry = (PokedexRules.Entry)model.GetValue(iter, 0);
            (cell as CellRendererToggle).Active = entry.Available;
        }
    }
}
