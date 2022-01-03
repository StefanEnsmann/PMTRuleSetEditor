using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {
    interface ILocalizable {
        Localization Localization { get; set; }
    }

    class LocalizationEntry {
        public string Code { get; private set; }
        public string Value { get; set; }

        public TreeIter Iter { get; set; }

        public LocalizationEntry(string code, string value) {
            Code = code;
            Value = value;
        }
    }

    [JsonConverter(typeof(LocalizationConverter))]
    class Localization {
        private Dictionary<string, LocalizationEntry> translations;
        public Dictionary<string, string> Translations {
            get {
                Dictionary<string, string> trans = new Dictionary<string, string>();
                foreach (string language in RuleSet.ActiveLanguages) {
                    trans.Add(language, translations[language].Value);
                }
                return trans;
            }
        }
        private TreeStore treeStore;
        public TreeModelSort Model { get; private set; }
        public RuleSet RuleSet { get; private set; }

        public Localization(RuleSet ruleSet, List<string> languages, List<string> activeLanguages, Dictionary<string, string> translations = null) {
            this.translations = new Dictionary<string, LocalizationEntry>();
            RuleSet = ruleSet;
            treeStore = new TreeStore(typeof(LocalizationEntry));
            Model = new TreeModelSort(treeStore);
            foreach (string language in languages) {
                string v = translations == null ? "" : translations[language];
                LocalizationEntry entry = new LocalizationEntry(language, v);
                this.translations.Add(language, entry);
                entry.Iter = activeLanguages.Contains(language) ? treeStore.AppendValues(entry) : TreeIter.Zero;
            }
        }

        public string this[string key] {
            get => translations[key].Value;
            set {
                translations[key].Value = value;
            }
        }

        public void SetLanguageActive(string languageCode, bool active = true) {
            LocalizationEntry entry = translations[languageCode];
            if (entry.Iter.Equals(TreeIter.Zero) && active) {
                entry.Iter = treeStore.AppendValues(entry);
            }
            else if (!entry.Iter.Equals(TreeIter.Zero) && !active) {
                TreeIter iter = entry.Iter;
                treeStore.Remove(ref iter);
                entry.Iter = TreeIter.Zero;
            }
        }
    }
}
