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
                if (ruleSet != null) {
                    ruleSet.Cleanup();
                }
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
            RuleSet ruleSet = window.Main.RuleSet;
            string template = "new_location_";
            int value = 0;
            while (!ruleSet.AddLocation(template + value)) {
                ++value;
            }
        }

        public static void OnAddCheckClick(MainWindow window, Check.CheckType type) {
            RuleSet ruleSet = window.Main.RuleSet;
            DependencyEntry currentSelection = window.CurrentSelection;
            if (window.CurrentSelection != null) {
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
            if (window.CurrentSelection != null) {
                if (window.CurrentSelection is Location loc) {
                    ruleSet.RemoveLocation(loc);
                }
                else if (window.CurrentSelection is Check check) {
                    ruleSet.RemoveCheck(check.location, check);
                }
            }
        }

        public static void OnAddConditionClick(MainWindow window, Check.CheckType type) {
            if (window.CurrentSelection != null) {
                Check selectedCheck = CustomDialog.SelectCheck(window, "Select check", window.Main.RuleSet.Model, type);
                if (selectedCheck != null) {
                    window.CurrentSelection.AddCondition(selectedCheck);
                }
            }
        }

        public static void OnRemoveConditionClick(MainWindow window, TreeView treeView) {
            treeView.Selection.GetSelected(out TreeModel model, out TreeIter iter);
            if (window.CurrentSelection != null && model.GetValue(iter, 0) is Check check) {
                window.CurrentSelection.RemoveCondition(check);
            }
        }
    }
}
