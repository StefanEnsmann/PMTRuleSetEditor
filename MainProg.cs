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
    public partial class MainProg {
        public RuleSet Rules { get; private set; }
        public string CurrentFile { get; set; }

        private static MainProg Instance { get; set; }

        public static readonly string BaseURL = "https://pkmntracker.ensmann.de/";
        public static Dictionary<string, string> SupportedLanguages { get; private set; }
        public static List<string> DefaultGames { get; private set; }
        public static Pokedex Pokedex { get; private set; }

        public MainProg() {
            if (Instance == null) {
                Instance = this;
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

                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 }) {
                    string pokedexData = wc.DownloadString(BaseURL + "pkmn_data/pokedex.json");
                    Pokedex = JsonConvert.DeserializeObject<Pokedex>(pokedexData);
                }

                Rules = new RuleSet();
            }
        }

        public RuleSet NewRuleSet() {
            if (Rules != null) {
                Rules.Cleanup();
            }
            Rules = new RuleSet();
            return Rules;
        }

        public RuleSet LoadRuleSet(string file) {
            Rules.Cleanup();
            Rules = RuleSet.FromFile(file);
            CurrentFile = file;
            return Rules;
        }

        private void Run() {
            Application.Init();
            MainWindow window = new MainWindow(this);
            Application.Run();
        }

        static void Main(string[] args) {
            MainProg prog = new MainProg();
            prog.Run();
        }
    }
}
