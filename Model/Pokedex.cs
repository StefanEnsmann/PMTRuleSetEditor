using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;
using Newtonsoft.Json;

namespace PokemonTrackerEditor.Model {
    [JsonConverter(typeof(PokedexRulesConverter))]
    public class PokedexRules {
        public Pokedex Pokedex { get; private set; }
        public RuleSet Rules { get; private set; }
        public List<Entry> List { get; private set; }

        public TreeStore ListStore { get; private set; }

        public PokedexRules(Pokedex pokedex, RuleSet ruleSet) {
            Pokedex = pokedex;
            Rules = ruleSet;
            List = new List<Entry>();
            ListStore = new TreeStore(typeof(Entry));
            foreach (Pokedex.Entry entry in pokedex.List) {
                Entry e = new Entry(entry.nr, this);
                List.Add(e);
                ListStore.AppendValues(e);
            }
        }

        public class Entry {
            public int Nr { get; set; }
            public bool Available { get; set; }
            public PokedexRules Rules { get; private set; }
            public Pokedex.Entry Data => Rules.Pokedex.List[Nr-1];

            public Entry(int nr, PokedexRules rules) {
                Rules = rules;
                Nr = nr;
                Available = false;
            }
        }
    }

    [JsonConverter(typeof(PokedexDataConverter))]
    public class Pokedex {
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Overrides { get; private set; }

        public Dictionary<string, List<int>> Templates { get; private set; }

        public List<Entry> List { get; private set; }

        public Pokedex() {
            Overrides = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            Templates = new Dictionary<string, List<int>>();
            List = new List<Entry>();
        }

        public void AddPokemon(Entry entry) {
            List.Add(entry);
        }

        public class Entry {
            public int nr;
            public string typeA;
            public string typeB;
            public Dictionary<string, string> Localization { get; private set; }

            public Entry() {
                nr = -1;
                typeA = null;
                typeB = null;
                Localization = new Dictionary<string, string>();
            }
        }
    }
}
