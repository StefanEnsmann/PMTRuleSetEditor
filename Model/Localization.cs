using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {
    public interface ILocalizable {
        Localization Localization { get; set; }
        RuleSet Rules { get; }
    }

    public class LocalizationEntry {
        public string Code { get; private set; }
        public string Value { get; set; }

        public TreeIter Iter { get; set; }

        public LocalizationEntry(string code, string value="") {
            Code = code;
            Value = value;
        }
    }

    [JsonConverter(typeof(LocalizationConverter))]
    public class Localization {
        public Dictionary<string, LocalizationEntry> Translations { get; private set; }
        private readonly TreeStore treeStore;
        private readonly TreeModelSort treeSort;
        private ILocalizable localizable;
        public TreeModelFilter Model { get; private set; }

        public Localization(ILocalizable localizable) {
            this.localizable = localizable;
            Translations = new Dictionary<string, LocalizationEntry>();
            treeStore = new TreeStore(typeof(LocalizationEntry));
            foreach (string language in MainProg.SupportedLanguages.Keys) {
                LocalizationEntry entry = new LocalizationEntry(language);
                Translations.Add(language, entry);
                entry.Iter = treeStore.AppendValues(entry);
            }
            treeSort = new TreeModelSort(treeStore);
            Model = new TreeModelFilter(treeSort, null) {
                VisibleFunc = new TreeModelFilterVisibleFunc((ITreeModel m, TreeIter i) => FilterLanguageList(m, i, localizable.Rules))
            };
        }

        private static bool FilterLanguageList(ITreeModel model, TreeIter iter, RuleSet rules) {
            LocalizationEntry current = (LocalizationEntry)model.GetValue(iter, 0);
            return rules.ActiveLanguages.Contains(current.Code);
        }

        public string this[string key] {
            get => Translations[key].Value;
            set {
                Translations[key].Value = value;
            }
        }

        public void Cleanup() {
            Translations.Clear();

            Model.Dispose();

            treeStore.Clear();
            treeStore.Dispose();
        }
    }
}
