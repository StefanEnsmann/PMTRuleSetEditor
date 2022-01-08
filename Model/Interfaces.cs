using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

namespace PokemonTrackerEditor.Model {
    public interface IMovableItems<T> {
        bool MoveUp(T item);
        bool MoveDown(T item);
    }

    public interface IMovable { 
        TreeIter Iter { get; set; }
    }

    public class InterfaceHelpers {
        public static bool SwapItems<T>(List<T> list, T item, bool up) where T : IMovable {
            int idx = list.IndexOf(item);
            int newIdx = up ? idx - 1 : idx + 1;
            if (up && idx == 0 || !up && newIdx == list.Count) {
                return false;
            }
            else {
                list[idx] = list[newIdx];
                list[newIdx] = item;
                return true;
            }
        }
    }
}
