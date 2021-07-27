using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PokemonTrackerEditor.Model;

using Gtk;

namespace PokemonTrackerEditor.View.MainWindow {
    class EventCallbacks {
        public static void OnLocationTreeSelectionChanged(MainWindow window, TreeSelection selection) {
            DependencyEntry currentSelection = null;
            if (selection.GetSelected(out TreeIter iter)) {
                object obj = window.Main.RuleSet.Model.GetValue(iter, 0);
                DependencyEntryBase entry = (DependencyEntryBase)obj;
                if (entry != null) {
                    if (entry is LocationCategory locCat) {
                        currentSelection = locCat.Parent;
                    }
                    else if (entry is DependencyEntry depEntry) {
                        currentSelection = depEntry;
                    }
                }
            }
            window.UpdateEditorSelection(currentSelection);
        }
        public static void OnStoryItemTreeSelectionChanged(MainWindow window, TreeSelection selection) {
            if (selection.GetSelected(out TreeIter iter)) {
                object obj = window.Main.RuleSet.StoryItems.Model.GetValue(iter, 0);
                StoryItemBase entry = (StoryItemBase)obj;
                if (entry != null) {
                    window.CurrentStoryItemSelection = entry;
                }
            }
        }

        public static void OnLocationNameEdited(MainWindow window, TreePath path, string newText) {
            window.Main.RuleSet.Model.GetIter(out TreeIter iter, path);
            DependencyEntryBase data = (DependencyEntryBase)window.Main.RuleSet.Model.GetValue(iter, 0);
            if ((data is Location && window.Main.RuleSet.LocationNameAvailable(newText)) || (data is Check check && check.location.CheckNameAvailable(newText, check.Type))) {
                data.Id = newText;
                window.Main.RuleSet.Model.Model.EmitRowChanged(path, iter);
            }
        }

        public static void OnStoryItemNameEdited(MainWindow window, TreePath path, string newText) {
            window.Main.RuleSet.StoryItems.Model.GetIter(out TreeIter iter, path);
            StoryItemBase data = (StoryItemBase)window.Main.RuleSet.StoryItems.Model.GetValue(iter, 0);
            if ((data is StoryItemCategory && window.Main.RuleSet.StoryItems.CategoryNameAvailable(newText)) || (data is StoryItem storyItem && storyItem.Category.StoryItemNameAvailable(newText))) {
                data.Id = newText;
                window.Main.RuleSet.StoryItems.Model.EmitRowChanged(path, iter);
            }
        }

        public static void OnLocalizationValueEdited(MainWindow window, TreePath path, string newText) {
            window.CurrentLocationSelection.Localization.Model.GetIter(out TreeIter iter, path);
            LocalizationEntry entry = (LocalizationEntry)window.CurrentLocationSelection.Localization.Model.GetValue(iter, 0);
            entry.Value = newText;
        }
    }
}
