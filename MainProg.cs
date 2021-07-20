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

        private MainWindow window;

        public RuleSet NewRuleSet() {
            RuleSet.Cleanup();
            RuleSet = new RuleSet();
            return RuleSet;
        }

        public RuleSet LoadRuleSet(string file) {
            RuleSet.Cleanup();
            RuleSet = RuleSet.FromFile(file);
            CurrentFile = file;
            return RuleSet;
        }

        private void Run() {
            RuleSet = new RuleSet();

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
