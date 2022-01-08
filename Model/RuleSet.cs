using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Gtk;

namespace PokemonTrackerEditor.Model {
    [JsonConverter(typeof(RuleSetConverter))]
    public class RuleSet : ILocationContainer {
        public string Name { get; set; }
        public string Game { get; set; }
        public TreeStore Model { get; private set; }
        public List<Location> Locations { get; private set; }
        public StoryItems StoryItems { get; private set; }
        public PokedexRules Pokedex { get; private set; }

        public List<string> ActiveLanguages { get; private set; }
        public bool HasChanged { get; private set; }

        public RuleSet Rules => this;

        public RuleSet() {
            Pokedex = new PokedexRules(MainProg.Pokedex, this);
            Model = new TreeStore(typeof(DependencyEntryBase));
            Locations = new List<Location>();
            StoryItems = new StoryItems(this);
            ActiveLanguages = new List<string>();
            HasChanged = false;
        }

        public void Cleanup() {
            foreach (Location location in Locations) {
                location.Cleanup();
            }
            Locations.Clear();
            StoryItems.Cleanup();
            ActiveLanguages.Clear();
            Model.Clear();
            Model.Dispose();
        }

        public void MergeLocationToModel(Location location) {
            location.Iter = location.Parent is Location
                ? Model.AppendValues((location.Parent as Location).LocationIter, location)
                : Model.AppendValues(location);

            location.ItemIter = Model.AppendValues(location.Iter, new LocationCategory("Items", location, Check.Type.ITEM));
            location.PokemonIter = Model.AppendValues(location.Iter, new LocationCategory("Pokémon", location, Check.Type.POKEMON));
            location.TradeIter = Model.AppendValues(location.Iter, new LocationCategory("Trades", location, Check.Type.TRADE));
            location.TrainerIter = Model.AppendValues(location.Iter, new LocationCategory("Trainers", location, Check.Type.TRAINER));
            location.LocationIter = Model.AppendValues(location.Iter, new LocationCategoryForLocations("Locations", location));
        }

        public void MergeCheckToModel(Check check) {
            TreeIter parentIter = TreeIter.Zero;
            Location location = check.Parent as Location;
            switch (check.CheckType) {
                case Check.Type.ITEM:
                    parentIter = location.ItemIter; break;
                case Check.Type.POKEMON:
                    parentIter = location.PokemonIter; break;
                case Check.Type.TRADE:
                    parentIter = location.TradeIter; break;
                case Check.Type.TRAINER:
                    parentIter = location.TrainerIter; break;
            }
            check.Iter = Model.AppendValues(parentIter, check);
        }

        public void RemoveDependencyIter(TreeIter iter) {
            Model.Remove(ref iter);
        }

        public void ChildWasRemoved(DependencyEntry entry) {
            Locations.Remove(entry as Location);
        }

        public bool MoveUp(DependencyEntry location) {
            return InterfaceHelpers.SwapItems(Locations, (Location)location, true);
        }

        public bool MoveDown(DependencyEntry location) {
            return InterfaceHelpers.SwapItems(Locations, (Location)location, false);
        }

        public void SetLanguageActive(string language, bool active = true) {
            if (active && !ActiveLanguages.Contains(language)) {
                ActiveLanguages.Add(language);
            }
            else {
                ActiveLanguages.Remove(language);
            }
        }

        public void ReportChange() {
            HasChanged = true;
        }

        public void SaveToFile(string filepath) {
            ActiveLanguages.Sort();
            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
            using (StreamWriter file = File.CreateText(filepath)) {
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, this);
            }
            HasChanged = false;
        }

        public static RuleSet FromFile(string filepath) {
            JsonSerializer serializer = new JsonSerializer();
            RuleSet ret;
            using (StreamReader reader = new StreamReader(filepath)) {
                ret = (RuleSet)serializer.Deserialize(reader, typeof(RuleSet));
            }
            return ret;
        }
    }
}
