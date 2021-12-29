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
            window.UpdateLocationEditorSelection(currentSelection);
        }
        public static void OnStoryItemTreeSelectionChanged(MainWindow window, TreeSelection selection) {
            StoryItemBase currentSelection = null;
            if (selection.GetSelected(out TreeIter iter)) {
                object obj = window.Main.RuleSet.StoryItems.Model.GetValue(iter, 0);
                StoryItemBase entry = (StoryItemBase)obj;
                if (entry != null) {
                    currentSelection = entry;
                }
            }
            window.UpdateStoryItemEditorSelection(currentSelection);
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

        public static void OnStoryItemURLEdited(MainWindow window, string newText) {
            if (window.CurrentStoryItemSelection is StoryItem) {
                (window.CurrentStoryItemSelection as StoryItem).ImageURL = newText;
            }
        }

        public static void OnLocalizationValueEdited(MainWindow window, TreePath path, string newText, bool location=true) {
            TreeModel model = location ? window.CurrentLocationSelection.Localization.Model : window.CurrentStoryItemSelection.Localization.Model;
            model.GetIter(out TreeIter iter, path);
            LocalizationEntry entry = (LocalizationEntry)model.GetValue(iter, 0);
            entry.Value = newText;
        }

        public static void OnPokedexEntryToggled(MainWindow window, TreePath path) {
            window.Main.Pokedex.ListStore.GetIter(out TreeIter iter, path);
            PokedexData.Entry entry = (PokedexData.Entry)window.Main.Pokedex.ListStore.GetValue(iter, 0);
            entry.available = !entry.available;
        }
    }
}
