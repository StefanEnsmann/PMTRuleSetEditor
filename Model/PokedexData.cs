using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;
using Newtonsoft.Json;

namespace PokemonTrackerEditor.Model {
    [JsonConverter(typeof(PokedexDataConverter))]
    class PokedexData {
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Overrides { get; private set; }

        public Dictionary<string, List<int>> Templates { get; private set; }

        public List<PokedexEntry> List { get; private set; }

        public TreeStore ListStore { get; private set; }

        public PokedexData() {
            Overrides = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            Templates = new Dictionary<string, List<int>>();
            List = new List<PokedexEntry>();
            ListStore = new TreeStore(typeof(PokedexEntry));
        }

        public void AddPokemon(PokedexEntry entry) {
            List.Add(entry);
            ListStore.AppendValues(entry);
        }

        public class PokedexEntry {
            public bool available;
            public int nr;
            public string typeA;
            public string typeB;
            public Dictionary<string, string> Localization { get; private set; }

            public PokedexEntry() {
                available = false;
                nr = -1;
                typeA = null;
                typeB = null;
                Localization = new Dictionary<string, string>();
            }
        }
    }
}
