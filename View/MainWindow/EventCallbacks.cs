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
                TreeModelBaseClass entry = (TreeModelBaseClass)obj;
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

        public static void OnLocationNameEdited(MainWindow window, TreePath path, string newText) {
            window.Main.RuleSet.Model.GetIter(out TreeIter iter, path);
            TreeModelBaseClass data = (TreeModelBaseClass)window.Main.RuleSet.Model.GetValue(iter, 0);
            if ((data is Location && window.Main.RuleSet.LocationNameAvailable(newText)) || (data is Check check && check.location.CheckNameAvailable(newText, check.Type))) {
                data.Id = newText;
                window.Main.RuleSet.Model.Model.EmitRowChanged(path, iter);
            }
        }
    }
}
