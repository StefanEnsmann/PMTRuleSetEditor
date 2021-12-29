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
            RuleSet ruleSet = window.Main.RuleSet;
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
            RuleSet ruleSet = window.Main.RuleSet;
            if (AskForFileSave(window)) {
                window.SetRuleSet(window.Main.NewRuleSet(), null);
            }
        }

        public static void OnSelectFileClick(MainWindow window) {
            RuleSet ruleSet = window.Main.RuleSet;
            if (AskForFileSave(window)) {
                if (ruleSet != null) {
                    ruleSet.Cleanup();
                }
                string selectedFile = CustomDialog.SelectRuleSetFile(window, "Select rule set file", false);
                if (selectedFile != null) {
                    window.SetRuleSet(window.Main.LoadRuleSet(selectedFile), selectedFile);
                }
            }
        }

        public static void OnSaveFileClick(MainWindow window) {
            RuleSet ruleSet = window.Main.RuleSet;
            if (ruleSet != null) {
                string saveFile = GetNewFile(window);
                if (saveFile != null) {
                    ruleSet.SaveToFile(saveFile);
                    window.Main.CurrentFile = saveFile;
                }
                else {
                    CustomDialog.MessageDialog(window, "No file selected! Rule set was not saved.", "Warning");
                }
            }
        }

        public static void OnAddLocationClick(MainWindow window) {
            DependencyEntry currentSelection = window.CurrentLocationSelection;
            Location loc = currentSelection == null ? null : currentSelection is Location location ? location : ((Check)currentSelection).location;
            RuleSet ruleSet = window.Main.RuleSet;
            string template = "new_location_";
            int value = 0;
            while (!ruleSet.AddLocation(template + value, loc)) {
                ++value;
            }
        }

        public static void OnAddCheckClick(MainWindow window, Check.CheckType type) {
            RuleSet ruleSet = window.Main.RuleSet;
            DependencyEntry currentSelection = window.CurrentLocationSelection;
            if (window.CurrentLocationSelection != null) {
                Location loc = currentSelection is Location location ? location : ((Check)currentSelection).location;
                string template = "new_" + type.ToString().ToLower() + "_";
                int value = 0;
                while (!ruleSet.AddCheck(loc, template + value, type)) {
                    ++value;
                }
            }
        }

        public static void OnRemoveLocationOrCheckClick(MainWindow window) {
            RuleSet ruleSet = window.Main.RuleSet;
            if (window.CurrentLocationSelection != null) {
                if (window.CurrentLocationSelection is Location loc) {
                    ruleSet.RemoveLocation(loc);
                }
                else if (window.CurrentLocationSelection is Check check) {
                    ruleSet.RemoveCheck(check.location, check);
                }
            }
        }

        public static void OnAddConditionClick(MainWindow window, Check.CheckType type) {
            if (window.CurrentLocationSelection != null) {
                Check selectedCheck = CustomDialog.SelectCheck(window, "Select check", window.Main.RuleSet.Model, type);
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
                StoryItem storyItem = CustomDialog.SelectStoryItem(window, "Select story item", window.Main.RuleSet.StoryItems.Model);
                if (storyItem != null) {
                    StoryItemConditionCollection collection = GetSelectedStoryItemConditionContainer(window, treeView);
                    Console.WriteLine(collection.Id);
                    collection.AddStoryItemCondition(storyItem);
                }
            }
        }

        public static void OnAddStoryItemConditionANDCollectionClick(MainWindow window, TreeView treeView) {
            if (window.CurrentLocationSelection != null) {
                StoryItemConditionCollection collection = GetSelectedStoryItemConditionContainer(window, treeView);
                Console.WriteLine(collection.Id);
                collection.AddANDCollection();
            }
        }

        public static void OnAddStoryItemConditionORCollectionClick(MainWindow window, TreeView treeView) {
            if (window.CurrentLocationSelection != null) {
                StoryItemConditionCollection collection = GetSelectedStoryItemConditionContainer(window, treeView);
                Console.WriteLine(collection.Id);
                collection.AddORCollection();
            }
        }

        public static void OnRemoveStoryItemConditionClick(MainWindow window, TreeView treeView) {
            if (window.CurrentLocationSelection != null) {
                StoryItemConditionBase selection = GetSelectedStoryItemCondition(window, treeView);
                if (selection is StoryItemsConditions conditions) {
                    Console.WriteLine("Removing all conditions");
                    conditions.RemoveAll();
                }
                else {
                    Console.WriteLine(selection.Id);
                    selection.Container.RemoveStoryItemCondition(selection);
                }
            }
        }

        public static void OnAddStoryItemCategoryClick(MainWindow window) {
            StoryItems storyItems = window.Main.RuleSet.StoryItems;
            string template = "new_story_item_category_";
            int value = 0;
            while (!storyItems.AddStoryItemCategory(template + value)) {
                ++value;
            }
        }

        public static void OnAddStoryItemClick(MainWindow window) {
            StoryItemBase currentSelection = window.CurrentStoryItemSelection;
            if (window.CurrentStoryItemSelection != null) {
                StoryItemCategory cat = currentSelection is StoryItemCategory category ? category : ((StoryItem)currentSelection).Category;
                string template = "new_story_item_";
                int value = 0;
                while (!cat.AddStoryItem(template + value)) {
                    ++value;
                }
            }
        }

        public static void OnRemoveStoryItemClick(MainWindow window) {
            StoryItemBase currentSelection = window.CurrentStoryItemSelection;
            if (window.CurrentStoryItemSelection != null) {
                if (currentSelection is StoryItemCategory category) {
                    window.Main.RuleSet.StoryItems.RemoveStoryItemCategory(category);
                    window.CurrentStoryItemSelection = null;
                }
                else {
                    StoryItem storyItem = currentSelection as StoryItem;
                    storyItem.Category.RemoveStoryItem(storyItem);
                }
            }
        }

        public static void OnMoveUpStoryItemClick(MainWindow window) {

        }

        public static void OnMoveDownStoryItemClick(MainWindow window) {

        }

        public static void OnApplyPokedexTemplateClick(MainWindow window, string template) {
            if (template != null) {
                foreach (int idx in window.Main.Pokedex.Templates[template]) {
                    window.Main.Pokedex.List[idx - 1].available = true;
                }
            }
            else {
                foreach (PokedexData.Entry entry in window.Main.Pokedex.List) {
                    entry.available = false;
                }
            }
            window.pokedexTreeView.QueueDraw();
        }
    }
}
