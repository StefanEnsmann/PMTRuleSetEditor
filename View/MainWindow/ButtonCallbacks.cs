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
            string selectedFile = MainProg.CurrentFile;
            if (selectedFile == null) {
                selectedFile = CustomDialog.SelectRuleSetFile(window, "Select file path", true);
            }
            return selectedFile;
        }

        private static bool AskForFileSave(MainWindow window) {
            RuleSet ruleSet = MainProg.RuleSet;
            bool loadFile = true;
            if (ruleSet != null && ruleSet.HasChanged) {
                CustomDialog.ButtonType response = CustomDialog.YNCDialog(window, "There are changes to this file. Do you want to save?", "Save changes?");
                if (response == CustomDialog.ButtonType.YES) {
                    ruleSet.SaveToFile(GetNewFile(window));
                }
                else if (response == CustomDialog.ButtonType.CANCEL) {
                    loadFile = false;
                }
            }
            return loadFile;
        }
        public static void OnNewFileClick(MainWindow window) {
            if (AskForFileSave(window)) {
                window.SetRuleSet(MainProg.NewRuleSet(), null);
            }
        }

        public static void OnSelectFileClick(MainWindow window) {
            if (AskForFileSave(window)) {
                string selectedFile = CustomDialog.SelectRuleSetFile(window, "Select rule set file", false);
                if (selectedFile != null) {
                    window.SetRuleSet(MainProg.LoadRuleSet(selectedFile), selectedFile);
                }
            }
        }

        public static void OnSaveFileClick(MainWindow window) {
            RuleSet ruleSet = MainProg.RuleSet;
            if (ruleSet != null) {
                string saveFile = GetNewFile(window);
                if (saveFile != null) {
                    ruleSet.SaveToFile(saveFile);
                    MainProg.CurrentFile = saveFile;
                }
                else {
                    CustomDialog.MessageDialog(window, "No file selected! Rule set was not saved.", "Warning");
                }
            }
        }

        public static void OnAddLocationClick(MainWindow window, bool root=true) {
            DependencyEntry currentSelection = window.CurrentLocationSelection;
            ILocationContainer locationContainer = root || currentSelection == null ? MainProg.RuleSet : currentSelection is Location location ? location : ((Check)currentSelection).Parent;
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

        public static void OnAddCheckClick(MainWindow window, Check.CheckType type) {
            RuleSet ruleSet = MainProg.RuleSet;
            DependencyEntry currentSelection = window.CurrentLocationSelection;
            if (window.CurrentLocationSelection != null) {
                Location loc = currentSelection is Location location ? location : ((Check)currentSelection).Parent as Location;
                string template = "new_" + type.ToString().ToLower() + "_";
                int value = 0;
                Check check;
                while ((check = ruleSet.AddCheck(loc, template + value, type)) == null) {
                    ++value;
                }
                ShowAndSelect(window.LocationTreeView, check.Iter);
            }
        }

        public static void OnRemoveLocationOrCheckClick(MainWindow window) {
            RuleSet ruleSet = MainProg.RuleSet;
            if (window.CurrentLocationSelection != null) {
                if (window.CurrentLocationSelection is Location loc) {
                    ruleSet.RemoveLocation(loc);
                }
                else if (window.CurrentLocationSelection is Check check) {
                    ruleSet.RemoveCheck(check.Parent as Location, check);
                }
            }
        }

        public static void OnAddConditionClick(MainWindow window, Check.CheckType type) {
            if (window.CurrentLocationSelection != null) {
                Check selectedCheck = CustomDialog.SelectCheck(window, "Select check", MainProg.RuleSet.Model, type);
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
                StoryItem storyItem = CustomDialog.SelectStoryItem(window, "Select story item", MainProg.RuleSet.StoryItems.Model);
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
                if (selection is StoryItemsConditions conditions) {
                    conditions.RemoveAll();
                }
                else {
                    selection.Container.RemoveStoryItemCondition(selection);
                }
            }
        }

        public static void OnAddStoryItemCategoryClick(MainWindow window) {
            StoryItems storyItems = MainProg.RuleSet.StoryItems;
            string template = "new_story_item_category_";
            int value = 0;
            StoryItemCategory newCat;
            while ((newCat = storyItems.AddStoryItemCategory(template + value)) == null) {
                ++value;
            }
            ShowAndSelect(window.StoryItemsTreeView, newCat.Iter);
        }

        public static void OnAddStoryItemClick(MainWindow window) {
            StoryItemBase currentSelection = window.CurrentStoryItemSelection;
            if (window.CurrentStoryItemSelection != null) {
                StoryItemCategory cat = currentSelection is StoryItemCategory category ? category : ((StoryItem)currentSelection).Category;
                string template = "new_story_item_";
                int value = 0;
                StoryItem newItem;
                while ((newItem = cat.AddStoryItem(template + value)) == null) {
                    ++value;
                }
                ShowAndSelect(window.StoryItemsTreeView, newItem.Iter);
            }
        }

        public static void OnRemoveStoryItemClick(MainWindow window) {
            StoryItemBase currentSelection = window.CurrentStoryItemSelection;
            if (window.CurrentStoryItemSelection != null) {
                if (currentSelection is StoryItemCategory category) {
                    MainProg.RuleSet.StoryItems.RemoveStoryItemCategory(category);
                    window.CurrentStoryItemSelection = null;
                }
                else {
                    StoryItem storyItem = currentSelection as StoryItem;
                    storyItem.Category.RemoveStoryItem(storyItem);
                }
            }
        }

        public static void OnMoveUpStoryItemClick(MainWindow window) {
            if (window.CurrentStoryItemSelection is StoryItem storyItem) {
                MoveSelection(MainProg.RuleSet.StoryItems.Model, (StoryItem)window.CurrentStoryItemSelection, true, storyItem.Category);
            }
            else if (window.CurrentStoryItemSelection is StoryItemCategory) {
                MoveSelection(MainProg.RuleSet.StoryItems.Model, (StoryItemCategory)window.CurrentStoryItemSelection, true, MainProg.RuleSet.StoryItems);
            }
        }

        public static void OnMoveDownStoryItemClick(MainWindow window) {
            if (window.CurrentStoryItemSelection is StoryItem storyItem) {
                MoveSelection(MainProg.RuleSet.StoryItems.Model, (StoryItem)window.CurrentStoryItemSelection, false, storyItem.Category);
            }
            else if (window.CurrentStoryItemSelection is StoryItemCategory) {
                MoveSelection(MainProg.RuleSet.StoryItems.Model, (StoryItemCategory)window.CurrentStoryItemSelection, false, MainProg.RuleSet.StoryItems);
            }
        }

        public static void OnMoveUpLocationClick(MainWindow window) {
            IMovableItems<DependencyEntry> parent = window.CurrentLocationSelection.Parent ?? MainProg.RuleSet;
            MoveSelection(MainProg.RuleSet.Model, window.CurrentLocationSelection, true, parent);
        }

        public static void OnMoveDownLocationClick(MainWindow window) {
            IMovableItems<DependencyEntry> parent = window.CurrentLocationSelection.Parent ?? MainProg.RuleSet;
            MoveSelection(MainProg.RuleSet.Model, window.CurrentLocationSelection, false, parent);
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
                    MainProg.Pokedex.List[idx - 1].available = true;
                }
            }
            else {
                foreach (PokedexData.Entry entry in MainProg.Pokedex.List) {
                    entry.available = false;
                }
            }
            window.pokedexTreeView.QueueDraw();
        }
    }
}
