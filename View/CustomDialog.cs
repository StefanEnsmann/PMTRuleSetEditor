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
            dlg.ContentArea.PackStart(lbl, true, true, 5);
            dlg.AddButton(Stock.Yes, (int)ButtonType.YES);
            dlg.AddButton(Stock.No, (int)ButtonType.NO);
            dlg.AddButton(Stock.Cancel, (int)ButtonType.CANCEL);
            dlg.ContentArea.ShowAll();
            int response = dlg.Run();
            dlg.Destroy();
            return (ButtonType)response;
        }

        public static void MessageDialog(Window parent, string message, string title) {
            Dialog dlg = new Dialog(title, parent, DialogFlags.DestroyWithParent);
            dlg.ContentArea.PackStart(new Label(message), true, true, 5);
            dlg.AddButton(Stock.Ok, 0);
            dlg.ContentArea.ShowAll();
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
            string ret = dlg.Filename;
            dlg.Destroy();
            return response == (int)ResponseType.Accept ? ret : null;
        }

        private static bool FilterCheckList(ITreeModel model, TreeIter iter, Check.Type type) {
            DependencyEntryBase current = (DependencyEntryBase)model.GetValue(iter, 0);
            if (current != null) {
                if (current is LocationCategory locCat) {
                    if (current is LocationCategoryForLocations locCatLoc) {
                        foreach (Location loc in locCatLoc.Parent.Locations) {
                            if (loc.HasChecksOfType(type)) {
                                return true;
                            }
                        }
                        return false;
                    }
                    else {
                        return locCat.Type == type && locCat.Parent.GetListForCheckType(type).Count > 0;
                    }
                }
                else if (current is Location loc) {
                    return loc.HasChecksOfType(type);
                }
                else if (current is Check check) {
                    return check.CheckType == type;
                }
                else {
                    return false;
                }
            }
            else {
                return false;
            }
        }

        public static Check SelectCheck(Window parent, string title, ITreeModel model, Check.Type type) {
            Dialog dlg = new Dialog(title, parent, DialogFlags.DestroyWithParent);
            dlg.ContentArea.PackStart(new Label("Select check:"), true, true, 5);
            TreeModelFilter filter = new TreeModelFilter(model, null) {
                VisibleFunc = new TreeModelFilterVisibleFunc((ITreeModel m, TreeIter i) => FilterCheckList(m, i, type))
            };
            ComboBox cb = new ComboBox(filter);
            CellRendererText cbCellRenderer = new CellRendererText();
            cb.PackStart(cbCellRenderer, true);
            cb.SetCellDataFunc(cbCellRenderer, new CellLayoutDataFunc(MainWindow.Renderers.CheckComboBoxCell));
            dlg.ContentArea.PackStart(cb, true, true, 5);
            dlg.AddButton(Stock.Ok, 0);
            dlg.AddButton(Stock.Cancel, 1);
            dlg.ContentArea.ShowAll();
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

        public static StoryItem SelectStoryItem(Window parent, string title, ITreeModel model) {
            Dialog dlg = new Dialog(title, parent, DialogFlags.DestroyWithParent);
            dlg.ContentArea.PackStart(new Label("Select story item:"), true, true, 5);
            ComboBox cb = new ComboBox(model);
            CellRendererText cbCellRenderer = new CellRendererText();
            cb.PackStart(cbCellRenderer, true);
            cb.SetCellDataFunc(cbCellRenderer, new CellLayoutDataFunc(MainWindow.Renderers.StoryItemComboBoxCell));
            dlg.ContentArea.PackStart(cb, true, true, 5);
            dlg.AddButton(Stock.Ok, 0);
            dlg.AddButton(Stock.Cancel, 1);
            dlg.ContentArea.ShowAll();
            int response = dlg.Run();
            cb.GetActiveIter(out TreeIter iter);
            dlg.Destroy();
            if (response == 0) {
                return iter.Equals(TreeIter.Zero) ? null : model.GetValue(iter, 0) is StoryItem item ? item : null;
            }
            else {
                return null;
            }
        }
    }
}
