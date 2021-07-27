using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

using PokemonTrackerEditor.Model;
using PokemonTrackerEditor.View.MainWindow;

namespace PokemonTrackerEditor {
    partial class MainProg {
        public RuleSet RuleSet { get; private set; }
        public string CurrentFile { get; set; }

        public string BaseURL => "https://pkmntracker.ensmann.de/";
        public Dictionary<string, string> SupportedLanguages { get; private set; }


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
