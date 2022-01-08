using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PokemonTrackerEditor.Model;

using Gtk;

namespace PokemonTrackerEditor.View.MainWindow {
    class ButtonCallbacks {
        private static string GetNewFile(MainWindow window) {
            string selectedFile = window.Main.CurrentFile;
            if (selectedFile == null) {
                selectedFile = CustomDialog.SelectRuleSetFile(window, "Select file path", true);
            }
            return selectedFile;
        }

        private static bool AskForFileSave(MainWindow window) {
            bool loadFile = true;
            if (window.Main.Rules != null && window.Main.Rules.HasChanged) {
                CustomDialog.ButtonType response = CustomDialog.YNCDialog(window, "There are changes to this file. Do you want to save?", "Save changes?");
                if (response == CustomDialog.ButtonType.YES) {
                    window.Main.Rules.SaveToFile(GetNewFile(window));
                }
                else if (response == CustomDialog.ButtonType.CANCEL) {
                    loadFile = false;
                }
            }
            return loadFile;
        }
        public static void OnNewFileClick(MainWindow window) {
            if (AskForFileSave(window)) {
                window.SetRuleSet(window.Main.NewRuleSet(), null);
            }
        }

        public static void OnSelectFileClick(MainWindow window) {
            if (AskForFileSave(window)) {
                string selectedFile = CustomDialog.SelectRuleSetFile(window, "Select rule set file", false);
                if (selectedFile != null) {
                    window.SetRuleSet(window.Main.LoadRuleSet(selectedFile), selectedFile);
                }
            }
        }

        public static void OnSaveFileClick(MainWindow window) {
            if (window.Main.Rules != null) {
                string saveFile = GetNewFile(window);
                if (saveFile != null) {
                    window.Main.Rules.SaveToFile(saveFile);
                    window.Main.CurrentFile = saveFile;
                }
                else {
                    CustomDialog.MessageDialog(window, "No file selected! Rule set was not saved.", "Warning");
                }
            }
        }

        public static void OnAddLocationClick(MainWindow window, bool root=true) {
            DependencyEntry currentSelection = window.CurrentLocationSelection;
            ILocationContainer locationContainer = root || currentSelection == null ? window.Main.Rules : currentSelection is Location location ? location : ((Check)currentSelection).Parent;
            string template = "new_location_";
            int value = 0;
            Location newLoc;
            while ((newLoc = locationContainer.AddLocation(template + value)) == null) {
                ++value;
            }
            ShowAndSelect(window.LocationTreeView, newLoc.Iter);
        }

        private static void ShowAndSelect(TreeView view, TreeIter iter) {
            TreePath path = view.Model.GetPath(iter);
            path.Up();
            view.ExpandToPath(path);
            view.Selection.SelectIter(iter);
        }

        public static void OnAddCheckClick(MainWindow window, Check.Type type) {
            DependencyEntry currentSelection = window.CurrentLocationSelection;
            if (window.CurrentLocationSelection != null) {
                Location loc = currentSelection is Location location ? location : ((Check)currentSelection).Parent as Location;
                string template = "new_" + type.ToString().ToLower() + "_";
                int value = 0;
                Check check;
                while ((check = loc.AddCheck(template + value, type)) == null) {
                    ++value;
                }
                ShowAndSelect(window.LocationTreeView, check.Iter);
            }
        }

        public static void OnRemoveLocationOrCheckClick(MainWindow window) {
            if (window.CurrentLocationSelection != null) {
                window.CurrentLocationSelection.InvokeRemove();
            }
        }

        public static void OnAddConditionClick(MainWindow window, Check.Type type) {
            if (window.CurrentLocationSelection != null) {
                Check selectedCheck = CustomDialog.SelectCheck(window, "Select check", window.Main.Rules.Model, type);
                if (selectedCheck != null) {
                    window.CurrentLocationSelection.AddCondition(selectedCheck);
                }
            }
        }

        public static void OnRemoveConditionClick(MainWindow window, TreeView treeView) {
            treeView.Selection.GetSelected(out TreeModel model, out TreeIter iter);
            if (window.CurrentLocationSelection != null && model.GetValue(iter, 0) is Check check) {
                window.CurrentLocationSelection.RemoveCondition(check);
            }
        }

        private static StoryItemConditionBase GetSelectedStoryItemCondition(MainWindow window, TreeView treeView) {
            treeView.Selection.GetSelected(out TreeModel model, out TreeIter iter);
            if (!iter.Equals(TreeIter.Zero) && model.GetValue(iter, 0) is StoryItemConditionBase selection) {
                return selection;
            }
            else {
                return null;
            }
        }

        private static StoryItemConditionCollection GetSelectedStoryItemConditionContainer(MainWindow window, TreeView treeView) {
            StoryItemConditionBase selection = GetSelectedStoryItemCondition(window, treeView);
            if (selection != null) {
                if (selection is StoryItemCondition selectedItem) {
                    return selectedItem.Container;
                }
                else if (selection is StoryItemConditionCollection collection) {
                    return collection;
                }
                else {
                    return null;
                }
            }
            else {
                return window.CurrentLocationSelection.StoryItemsConditions;
            }
        }

        public static void OnAddStoryItemConditionClick(MainWindow window, TreeView treeView) {
            if (window.CurrentLocationSelection != null) {
                StoryItem storyItem = CustomDialog.SelectStoryItem(window, "Select story item", window.Main.Rules.StoryItems.Model);
                if (storyItem != null) {
                    StoryItemConditionCollection collection = GetSelectedStoryItemConditionContainer(window, treeView);
                    collection.AddStoryItemCondition(storyItem);
                }
            }
        }

        public static void OnAddStoryItemConditionANDCollectionClick(MainWindow window, TreeView treeView) {
            if (window.CurrentLocationSelection != null) {
                StoryItemConditionCollection collection = GetSelectedStoryItemConditionContainer(window, treeView);
                collection.AddANDCollection();
            }
        }

        public static void OnAddStoryItemConditionORCollectionClick(MainWindow window, TreeView treeView) {
            if (window.CurrentLocationSelection != null) {
                StoryItemConditionCollection collection = GetSelectedStoryItemConditionContainer(window, treeView);
                collection.AddORCollection();
            }
        }

        public static void OnAddStoryItemConditionNOTCollectionClick(MainWindow window, TreeView treeView) {
            if (window.CurrentLocationSelection != null) {
                StoryItemConditionCollection collection = GetSelectedStoryItemConditionContainer(window, treeView);
                collection.AddNOTCollection();
            }
        }

        public static void OnRemoveStoryItemConditionClick(MainWindow window, TreeView treeView) {
            if (window.CurrentLocationSelection != null) {
                StoryItemConditionBase selection = GetSelectedStoryItemCondition(window, treeView);
                if (!(selection is StoryItemsConditions)) {
                    selection.InvokeRemove();
                }
            }
        }

        public static void OnAddStoryItemCategoryClick(MainWindow window) {
            StoryItems storyItems = window.Main.Rules.StoryItems;
            string template = "new_story_item_category_";
            int value = 0;
            while (!storyItems.CategoryNameAvailable(template + value)) {
                ++value;
            }
            StoryItemCategory newCat = storyItems.AddStoryItemCategory(template + value);
            ShowAndSelect(window.StoryItemsTreeView, newCat.Iter);
        }

        public static void OnAddStoryItemClick(MainWindow window) {
            StoryItemBase currentSelection = window.CurrentStoryItemSelection;
            if (window.CurrentStoryItemSelection != null) {
                StoryItemCategory cat = currentSelection is StoryItemCategory category ? category : ((StoryItem)currentSelection).Category;
                string template = "new_story_item_";
                int value = 0;
                while (!cat.StoryItemNameAvailable(template + value)) {
                    ++value;
                }
                StoryItem newItem = cat.AddStoryItem(template + value);
                ShowAndSelect(window.StoryItemsTreeView, newItem.Iter);
            }
        }

        public static void OnRemoveStoryItemClick(MainWindow window) {
            StoryItemBase currentSelection = window.CurrentStoryItemSelection;
            if (window.CurrentStoryItemSelection != null) {
                currentSelection.InvokeRemove();
            }
        }

        public static void OnMoveUpStoryItemClick(MainWindow window) {
            if (window.CurrentStoryItemSelection is StoryItem storyItem) {
                MoveSelection(window.Main.Rules.StoryItems.Model, (StoryItem)window.CurrentStoryItemSelection, true, storyItem.Category);
            }
            else if (window.CurrentStoryItemSelection is StoryItemCategory) {
                MoveSelection(window.Main.Rules.StoryItems.Model, (StoryItemCategory)window.CurrentStoryItemSelection, true, window.Main.Rules.StoryItems);
            }
        }

        public static void OnMoveDownStoryItemClick(MainWindow window) {
            if (window.CurrentStoryItemSelection is StoryItem storyItem) {
                MoveSelection(window.Main.Rules.StoryItems.Model, (StoryItem)window.CurrentStoryItemSelection, false, storyItem.Category);
            }
            else if (window.CurrentStoryItemSelection is StoryItemCategory) {
                MoveSelection(window.Main.Rules.StoryItems.Model, (StoryItemCategory)window.CurrentStoryItemSelection, false, window.Main.Rules.StoryItems);
            }
        }

        public static void OnMoveUpLocationClick(MainWindow window) {
            IMovableItems<DependencyEntry> parent = window.CurrentLocationSelection.Parent ?? window.Main.Rules;
            MoveSelection(window.Main.Rules.Model, window.CurrentLocationSelection, true, parent);
        }

        public static void OnMoveDownLocationClick(MainWindow window) {
            IMovableItems<DependencyEntry> parent = window.CurrentLocationSelection.Parent ?? window.Main.Rules;
            MoveSelection(window.Main.Rules.Model, window.CurrentLocationSelection, false, parent);
        }

        private static void MoveSelection<T>(TreeStore model, T movable, bool up, IMovableItems<T> parent) where T : IMovable {
            TreePath path = model.GetPath(movable.Iter);
            if (up) {
                path.Prev();
            }
            else {
                path.Next();
            }
            model.GetIter(out TreeIter replace, path);
            if (model.IterIsValid(replace)) {
                if (up) {
                    model.MoveBefore(movable.Iter, replace);
                    parent.MoveUp(movable);
                }
                else {
                    model.MoveAfter(movable.Iter, replace);
                    parent.MoveDown(movable);
                }
            }
        }

        public static void OnApplyPokedexTemplateClick(MainWindow window, string template) {
            if (template != null) {
                foreach (int idx in MainProg.Pokedex.Templates[template]) {
                    window.Main.Rules.Pokedex.List[idx - 1].Available = true;
                }
            }
            else {
                foreach (PokedexRules.Entry entry in window.Main.Rules.Pokedex.List) {
                    entry.Available = false;
                }
            }
            window.pokedexTreeView.QueueDraw();
        }
    }
}
