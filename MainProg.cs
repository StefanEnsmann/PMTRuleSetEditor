using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

using Gtk;

using PokemonTrackerEditor.Model;
using PokemonTrackerEditor.View.MainWindow;

namespace PokemonTrackerEditor {
    partial class MainProg {
        public readonly string BaseURL = "https://pkmntracker.ensmann.de/";
        public RuleSet RuleSet { get; private set; }
        public string CurrentFile { get; set; }

        public Dictionary<string, string> SupportedLanguages { get; private set; }
        public List<string> DefaultGames { get; private set; }

        public PokedexData Pokedex { get; private set; }

        private MainWindow window;

        public MainProg() {
            SupportedLanguages = new Dictionary<string, string> {
                {"en", "English"},
                {"jp", "Japanese"},
                {"de", "German"},
                {"fr", "French"},
                {"es", "Spanish"},
                {"it", "Italian"},
                {"ko", "Korean"},
                {"zh-Hans", "Chinese (simplified)"},
                {"zh-Hant", "Chinese (traditional)"}
            };

            DefaultGames = new List<string> {
                "Pokémon Red",
                "Pokémon Green",
                "Pokémon Blue",
                "Pokémon Yellow",

                "Pokémon Gold",
                "Pokémon Silver",
                "Pokémon Crystal",

                "Pokémon Ruby",
                "Pokémon Sapphire",
                "Pokémon FireRed",
                "Pokémon LeafGreen",
                "Pokémon Emerald",

                "Pokémon Diamond",
                "Pokémon Perl",
                "Pokémon Platinum",
                "Pokémon HeartGold",
                "Pokémon SoulSilver",

                "Pokémon Black",
                "Pokémon White",
                "Pokémon Black 2",
                "Pokémon White 2",

                "Pokémon X",
                "Pokémon Y",
                "Pokémon Omega Ruby",
                "Pokémon Alpha Sapphire",

                "Pokémon Sun",
                "Pokémon Moon",
                "Pokémon Ultra Sun",
                "Pokémon Ultra Moon",
                "Pokémon Let's Go Pikachu!",
                "Pokémon Let's Go Eevee!",

                "Pokémon Sword",
                "Pokémon Shield",
                "Pokémon Brilliant Diamond",
                "Pokémon Shining Pearl",
            };
        }

        public RuleSet NewRuleSet() {
            if (RuleSet != null) {
                RuleSet.Cleanup();
            }
            RuleSet = new RuleSet(this);
            return RuleSet;
        }

        public RuleSet LoadRuleSet(string file) {
            RuleSet.Cleanup();
            RuleSet = RuleSet.FromFile(file, this);
            CurrentFile = file;
            return RuleSet;
        }

        private void Run() {
            using (WebClient wc = new WebClient()) {
                string pokedexData = wc.DownloadString(BaseURL + "pkmn_data/pokedex.json");
                Pokedex = JsonConvert.DeserializeObject<PokedexData>(pokedexData);
            }

            RuleSet = new RuleSet(this);

            Application.Init();
            window = new MainWindow(this);
            Application.Run();
        }

        static void Main(string[] args) {
            MainProg prog = new MainProg();
            prog.Run();
        }
    }
}
