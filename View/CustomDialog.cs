using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PokemonTrackerEditor.Model;

using Gtk;

namespace PokemonTrackerEditor.View {
    class CustomDialog {
        public enum ButtonType {
            YES = 0,
            NO = 1,
            CANCEL = 2
        }
        public static ButtonType YNCDialog(Window parent, string message, string title) {
            Dialog dlg = new Dialog(title, parent, DialogFlags.DestroyWithParent);
            Label lbl = new Label(message);
            dlg.VBox.PackStart(lbl);
            dlg.AddButton(Stock.Yes, (int)ButtonType.YES);
            dlg.AddButton(Stock.No, (int)ButtonType.NO);
            dlg.AddButton(Stock.Cancel, (int)ButtonType.CANCEL);
            dlg.VBox.ShowAll();
            int response = dlg.Run();
            dlg.Destroy();
            return (ButtonType)response;
        }

        public static void MessageDialog(Window parent, string message, string title) {
            Dialog dlg = new Dialog(title, parent, DialogFlags.DestroyWithParent);
            dlg.VBox.PackStart(new Label(message));
            dlg.AddButton(Stock.Ok, 0);
            dlg.VBox.ShowAll();
            dlg.Run();
            dlg.Destroy();
        }

        public static string SelectRuleSetFile(Window parent, string title, bool allowNewFile = false) {
            FileChooserDialog dlg = new FileChooserDialog(title, parent, allowNewFile ? FileChooserAction.Save : FileChooserAction.Open, "Cancel", ResponseType.Cancel, allowNewFile ? "Save" : "Open", ResponseType.Accept) {
                LocalOnly = true,
                SelectMultiple = false
            };
            FileFilter jsonFileFilter = new FileFilter { Name = "JSON files" };
            jsonFileFilter.AddPattern("*.json");
            dlg.AddFilter(jsonFileFilter);
            int response = dlg.Run();
            dlg.Destroy();
            return response == (int)ResponseType.Accept ? dlg.Filename : null;
        }

        private static bool FilterCheckList(TreeModel model, TreeIter iter, Check.CheckType type) {
            DependencyEntryBase current = (DependencyEntryBase)model.GetValue(iter, 0);
            if (current != null) {
                if (current is LocationCategory locCat) {
                    return locCat.Type == type;
                }
                else if (current is Location loc) {
                    return true;
                }
                else if (current is Check check) {
                    return check.Type == type;
                }
                else {
                    return false;
                }
            }
            else {
                return false;
            }
        }

        public static Check SelectCheck(Window parent, string title, TreeModel model, Check.CheckType type) {
            Dialog dlg = new Dialog(title, parent, DialogFlags.DestroyWithParent);
            dlg.VBox.PackStart(new Label("Select check:"));
            TreeModelFilter filter = new TreeModelFilter(model, null) {
                VisibleFunc = new TreeModelFilterVisibleFunc((TreeModel m, TreeIter i) => FilterCheckList(m, i, type))
            };
            ComboBox cb = new ComboBox(filter);
            CellRendererText cbCellRenderer = new CellRendererText();
            cb.PackStart(cbCellRenderer, true);
            cb.SetCellDataFunc(cbCellRenderer, new CellLayoutDataFunc(MainWindow.Renderers.CheckComboBoxCell));
            dlg.VBox.PackStart(cb);
            dlg.AddButton(Stock.Ok, 0);
            dlg.AddButton(Stock.Cancel, 1);
            dlg.VBox.ShowAll();
            int response = dlg.Run();
            cb.GetActiveIter(out TreeIter iter);
            dlg.Destroy();
            if (response == 0) {
                return filter.GetValue(iter, 0) is Check check ? check : null;
            }
            else {
                return null;
            }
        }

        public static StoryItem SelectStoryItem(Window parent, string title, TreeModel model) {
            Dialog dlg = new Dialog(title, parent, DialogFlags.DestroyWithParent);
            dlg.VBox.PackStart(new Label("Select story item:"));
            ComboBox cb = new ComboBox(model);
            CellRendererText cbCellRenderer = new CellRendererText();
            cb.PackStart(cbCellRenderer, true);
            cb.SetCellDataFunc(cbCellRenderer, new CellLayoutDataFunc(MainWindow.Renderers.StoryItemComboBoxCell));
            dlg.VBox.PackStart(cb);
            dlg.AddButton(Stock.Ok, 0);
            dlg.AddButton(Stock.Cancel, 1);
            dlg.VBox.ShowAll();
            int response = dlg.Run();
            cb.GetActiveIter(out TreeIter iter);
            dlg.Destroy();
            if (response == 0) {
                return model.GetValue(iter, 0) is StoryItem item ? item : null;
            }
            else {
                return null;
            }
        }
    }
}
