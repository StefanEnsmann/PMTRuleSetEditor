using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace PokemonTrackerEditor.Model {
    class DependencyCache {
        public enum NextType { ITEM, POKEMON, TRAINER, TRADE, LOCATION }
        public static NextType nextType;

        public static Dictionary<string, Path> Locations;
        public static RuleSet ruleSet;
        public static ILocationContainer currentLocationContainer;

        public static void Init() {
            ruleSet = null;
            currentLocationContainer = null;
            if (Locations != null) {
                foreach (Path p in Locations.Values) {
                    p.Clear();
                }
                Locations.Clear();
            }
            else {
                Locations = new Dictionary<string, Path>();
            }
        }

        public class Path {
            public int Index { get; private set; }
            public Dictionary<string, Path> Items;
            public Dictionary<string, Path> Pokemon;
            public Dictionary<string, Path> Trainers;
            public Dictionary<string, Path> Trades;
            public Dictionary<string, Path> Locations;

            public Path(int index) {
                Index = index;
                Items = new Dictionary<string, Path>();
                Pokemon = new Dictionary<string, Path>();
                Trainers = new Dictionary<string, Path>();
                Trades = new Dictionary<string, Path>();
                Locations = new Dictionary<string, Path>();
            }

            public void Clear() {
                foreach (Dictionary<string, Path> d in new Dictionary<string, Path>[]{ Items, Pokemon, Trainers, Trades, Locations }) {
                    foreach (Path p in d.Values) {
                        p.Clear();
                    }
                    d.Clear();
                }
            }
        }
    }

    class RuleSetConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(RuleSet);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            DependencyCache.Init();
            RuleSet ruleSet = new RuleSet();
            DependencyCache.ruleSet = ruleSet;
            reader.Read(); // "name"
            ruleSet.Name = reader.ReadAsString();
            Console.WriteLine("name: " + ruleSet.Name);
            reader.Read(); // "game"
            ruleSet.Game = reader.ReadAsString();
            Console.WriteLine("game: " + ruleSet.Game);

            foreach (PokedexData.Entry entry in MainProg.Pokedex.List) {
                entry.available = false;
            }
            Console.WriteLine("Pokedex cleared");

            reader.Read(); // "pokedex"
            reader.Read(); // StartArray
            Console.Write("Setting pokedex...");
            int? idx = reader.ReadAsInt32();
            while (idx.HasValue) {
                MainProg.Pokedex.List[idx.Value].available = true;
                idx = reader.ReadAsInt32();
                Console.Write(" " + idx);
            }
            Console.WriteLine("Pokedex set");

            reader.Read(); // "languages"
            reader.Read(); // StartArray
            Console.Write("Setting Languages...");
            string str = reader.ReadAsString();
            while (str != null) {
                ruleSet.SetLanguageActive(str);
                Console.Write(" " + str);
                str = reader.ReadAsString();
            }
            Console.WriteLine("Languages set");

            serializer.Deserialize(reader, typeof(StoryItems));
            Console.WriteLine("Story items deserialized");

            // maps

            reader.Read(); // "locations"
            reader.Read(); // StartArray
            DependencyCache.currentLocationContainer = ruleSet;
            while (serializer.Deserialize(reader, typeof(Location)) != null) ;

            reader.Read(); // EndObject

            return ruleSet;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            RuleSet ruleSet = (RuleSet)value;
            writer.WriteStartObject();

            writer.WritePropertyName("name"); writer.WriteValue(ruleSet.Name ?? "");

            writer.WritePropertyName("game"); writer.WriteValue(ruleSet.Game ?? "");

            serializer.Serialize(writer, MainProg.Pokedex);

            writer.WritePropertyName("languages");
            Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
            serializer.Serialize(writer, ruleSet.ActiveLanguages);
            writer.Formatting = writerBackup;

            serializer.Serialize(writer, ruleSet.StoryItems);

            // maps

            writer.WritePropertyName("locations");
            writer.WriteStartArray();
            foreach (Location loc in ruleSet.Locations) {
                serializer.Serialize(writer, loc);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }

    class StoryItemConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(StoryItem);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            RuleSet ruleSet = DependencyCache.ruleSet;
            StoryItems storyItems = ruleSet.StoryItems;
            reader.Read(); // "story_items"
            reader.Read(); // StartAray
            reader.Read(); // StartObject or EndArray
            while (reader.TokenType != JsonToken.EndArray) {
                reader.Read(); // "id"
                StoryItemCategory category = storyItems.AddStoryItemCategory(reader.ReadAsString());
                reader.Read(); // "items"
                reader.Read(); // StartArray
                reader.Read(); // StartObject or EndArray
                while (reader.TokenType != JsonToken.EndArray) {
                    reader.Read(); // "id"
                    StoryItem item = category.AddStoryItem(reader.ReadAsString());
                    reader.Read(); // "url"
                    item.ImageURL = reader.ReadAsString();
                    reader.Read(); // "localization"
                    category.Localization = (Localization)serializer.Deserialize(reader, typeof(Localization));
                    reader.Read(); // EndObject
                    reader.Read(); // StartObject or EndArray
                }
                reader.Read(); // EndObject
                reader.Read(); // StartObject or EndArray
            }
            return storyItems;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            StoryItems storyItems = (StoryItems)value;
            writer.WritePropertyName("story_items");
            writer.WriteStartArray();
            foreach (StoryItemCategory category in storyItems.Categories) {
                writer.WriteStartObject();
                writer.WritePropertyName("id"); writer.WriteValue(category.Id);
                writer.WritePropertyName("items");
                writer.WriteStartArray();
                foreach (StoryItem item in category.Items) {
                    writer.WriteStartObject();
                    Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
                    writer.WritePropertyName("id"); writer.WriteValue(item.Id);
                    writer.WritePropertyName("url"); writer.WriteValue(item.ImageURL);
                    writer.WritePropertyName("localization"); serializer.Serialize(writer, item.Localization);
                    writer.WriteEndObject();
                    writer.Formatting = writerBackup;
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }

    abstract class DependencyEntryConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            reader.Read(); // StartObject or something else
            if (reader.TokenType != JsonToken.StartObject) {
                reader.Read(); // "id"
                DependencyEntry entry = null;
                string id = reader.ReadAsString();
                switch (DependencyCache.nextType) {
                    case DependencyCache.NextType.ITEM:
                        entry = new Check(id, DependencyCache.ruleSet, Check.CheckType.ITEM, DependencyCache.currentLocationContainer as Location);
                        break;
                    case DependencyCache.NextType.POKEMON:
                        entry = new Check(id, DependencyCache.ruleSet, Check.CheckType.POKEMON, DependencyCache.currentLocationContainer as Location);
                        break;
                    case DependencyCache.NextType.TRAINER:
                        entry = new Check(id, DependencyCache.ruleSet, Check.CheckType.TRAINER, DependencyCache.currentLocationContainer as Location);
                        break;
                    case DependencyCache.NextType.TRADE:
                        entry = new Check(id, DependencyCache.ruleSet, Check.CheckType.TRADE, DependencyCache.currentLocationContainer as Location);
                        break;
                    case DependencyCache.NextType.LOCATION:
                        entry = DependencyCache.currentLocationContainer.AddLocation(id);
                        break;
                    default:
                        return null;
                }
                if (DependencyCache.nextType != DependencyCache.NextType.LOCATION) {
                    (DependencyCache.currentLocationContainer as Location).AddCheck(entry as Check);
                }
                reader.Read(); // "localization"
                entry.Localization = (Localization)serializer.Deserialize(reader, typeof(Localization));
                reader.Read(); // "conditions" or EndObject
                if (reader.TokenType != JsonToken.EndObject) {
                    reader.Read(); // StartObject
                    reader.Read(); // id or EndObject
                    while (reader.TokenType != JsonToken.EndObject) {
                        string type = (string)reader.Value;
                        switch (type) {
                            case "items":
                                break;
                            case "pokemon":
                                break;
                            case "trainers":
                                break;
                            case "trades":
                                break;
                            case "story_items":
                                break;
                            default:
                                Console.WriteLine("ERROR!");
                                break;
                        }
                        reader.Read(); // id or EndObject
                    }
                }
                return entry;
            }
            else {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            DependencyEntry entry = (DependencyEntry)value;
            writer.WritePropertyName("id"); writer.WriteValue(entry.Id);
            writer.WritePropertyName("localization"); serializer.Serialize(writer, entry.Localization);
            Dictionary<string, List<Check>> pairs = new Dictionary<string, List<Check>> { { "items", entry.ItemsConditions }, { "pokemon", entry.PokemonConditions }, { "trainers", entry.TrainersConditions }, { "trades", entry.TradesConditions} };
            bool hasOpened = false;
            foreach (KeyValuePair<string, List<Check>> pair in pairs) {
                if (pair.Value.Count > 0) {
                    if (!hasOpened) {
                        writer.WritePropertyName("conditions"); writer.WriteStartObject();
                        hasOpened = true;
                    }
                    writer.WritePropertyName(pair.Key); writer.WriteStartArray();
                    foreach (Check check in pair.Value) {
                        writer.WriteStartObject();
                        Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
                        writer.WritePropertyName("location"); writer.WriteValue((check.Parent as Location).LocationPath);
                        writer.WritePropertyName("id"); writer.WriteValue(check.Id);
                        writer.WriteEndObject();
                        writer.Formatting = writerBackup;
                    }
                    writer.WriteEndArray();
                }
            }
            if (entry.StoryItemsCondCount > 0) {
                if (!hasOpened) {
                    writer.WritePropertyName("conditions"); writer.WriteStartObject();
                    hasOpened = true;
                }
                writer.WritePropertyName("story_items"); serializer.Serialize(writer, entry.StoryItemsConditions);
            }
            if (hasOpened) {
                writer.WriteEndObject();
            }
        }
    }

    class LocationConverter : DependencyEntryConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Location);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            DependencyCache.nextType = DependencyCache.NextType.LOCATION;
            if (base.ReadJson(reader, objectType, existingValue, serializer) is Location loc) {
                return loc;
            }
            else {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            Location loc = (Location)value;
            writer.WriteStartObject();
            base.WriteJson(writer, value, serializer);

            Dictionary<string, List<Check>> pairs = new Dictionary<string, List<Check>> { { "items", loc.Items }, { "pokemon", loc.Pokemon }, { "trainers", loc.Trainers }, { "trades", loc.Trades } };
            foreach (KeyValuePair<string, List<Check>> pair in pairs) {
                if (pair.Value.Count > 0) {
                    writer.WritePropertyName(pair.Key); writer.WriteStartArray();
                    foreach (Check check in pair.Value) {
                        serializer.Serialize(writer, check);
                    }
                    writer.WriteEndArray();
                }
            }

            if (loc.LocationCount > 0) {
                writer.WritePropertyName("locations"); writer.WriteStartArray();
                foreach (Location nestedLoc in loc.Locations) {
                    serializer.Serialize(writer, nestedLoc);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }

    class CheckConverter : DependencyEntryConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Check);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteStartObject();
            base.WriteJson(writer, value, serializer);
            writer.WriteEndObject();
        }
    }

    class StoryItemConditionConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return (new List<Type> { typeof(StoryItemCondition), typeof(StoryItemANDCondition), typeof(StoryItemORCondition), typeof(StoryItemNOTCondition), typeof(StoryItemsConditions) }.Contains(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            StoryItemConditionBase condition = (StoryItemConditionBase)value;
            if (condition is StoryItemsConditions collection) {
                writer.WriteStartArray();
                foreach (StoryItemConditionBase nestedCondition in collection.Conditions) {
                    serializer.Serialize(writer, nestedCondition);
                }
                writer.WriteEndArray();
            }
            else if (condition is StoryItemConditionCollection conditionalCollection) {
                writer.WriteStartObject();
                writer.WritePropertyName("type"); writer.WriteValue(conditionalCollection.Type);
                writer.WritePropertyName("conditions"); writer.WriteStartArray();
                foreach (StoryItemConditionBase nestedCondition in conditionalCollection.Conditions) {
                    serializer.Serialize(writer, nestedCondition);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else if (condition is StoryItemCondition nestedCondition) {
                writer.WriteStartObject();
                Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
                writer.WritePropertyName("category"); writer.WriteValue(nestedCondition.StoryItem.Category.Id);
                writer.WritePropertyName("id"); writer.WriteValue(nestedCondition.StoryItem.Id);
                writer.WriteEndObject();
                writer.Formatting = writerBackup;
            }
        }
    }

    class LocalizationConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Localization);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            Localization loc = new Localization(DependencyCache.ruleSet, MainProg.SupportedLanguages.Keys.ToList(), DependencyCache.ruleSet.ActiveLanguages);
            reader.Read(); // StartObject
            reader.Read(); // id or EndObject
            while (reader.TokenType != JsonToken.EndObject) {
                string id = (string)reader.Value;
                loc[id] = reader.ReadAsString();
                reader.Read(); // id or EndObject
            }
            return loc;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            Localization loc = (Localization)value;
            writer.WriteStartObject();
            Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
            foreach (KeyValuePair<string, string> pair in loc.Translations) {
                writer.WritePropertyName(pair.Key); writer.WriteValue(pair.Value);
            }
            writer.WriteEndObject();
            writer.Formatting = writerBackup;
        }
    }

    class PokedexDataConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(PokedexData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            PokedexData pokedex = new PokedexData();
            reader.Read(); // object key
            while (reader.TokenType != JsonToken.EndObject) {
                string currentPosition = reader.Value.ToString();
                switch (currentPosition) {
                    case "overrides":
                        ReadOverrides(reader, pokedex);
                        break;
                    case "templates":
                        ReadTemplates(reader, pokedex);
                        break;
                    case "list":
                        ReadList(reader, pokedex);
                        break;
                }
                reader.Read(); // object key or EndObject
            }
            return pokedex;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WritePropertyName("pokedex");
            Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (PokedexData.Entry entry in MainProg.Pokedex.List) {
                if (entry.available) {
                    writer.WriteValue(entry.nr);
                }
            }
            writer.WriteEndArray();
            writer.Formatting = writerBackup;
        }

        private void ReadList(JsonReader reader, PokedexData pokedex) {
            reader.Read(); // StartArray
            reader.Read(); // StartObject or EndArray
            while (reader.TokenType != JsonToken.EndArray) {
                pokedex.AddPokemon(ReadListEntry(reader));
                reader.Read(); // StartObject or EndArray
            }
        }

        private PokedexData.Entry ReadListEntry(JsonReader reader) {
            PokedexData.Entry entry = new PokedexData.Entry();
            reader.Read();
            while (reader.TokenType != JsonToken.EndObject) {
                string entryKey = reader.Value.ToString();
                switch (entryKey) {
                    case "localization":
                        ReadLocalization(reader, entry);
                        break;
                    case "nr":
                        entry.nr = reader.ReadAsInt32() ?? -1;
                        break;
                    case "typeA":
                        entry.typeA = reader.ReadAsString();
                        break;
                    case "typeB":
                        entry.typeB = reader.ReadAsString();
                        break;
                }
                reader.Read();
            }
            return entry;
        }

        private void ReadLocalization(JsonReader reader, PokedexData.Entry entry) {
            reader.Read(); // StartObject
            reader.Read(); // localization key
            while (reader.TokenType != JsonToken.EndObject) {
                entry.Localization.Add(reader.Value.ToString(), reader.ReadAsString());
                reader.Read(); // localization key or EndObject
            }
        }

        private void ReadTemplates(JsonReader reader, PokedexData pokedex) {
            reader.Read(); // StartObject
            reader.Read(); // template key
            while (reader.TokenType != JsonToken.EndObject) {
                string templateKey = reader.Value.ToString();
                List<int> templateList = new List<int>();
                reader.Read(); // StartArray
                int? index = reader.ReadAsInt32();
                while (index.HasValue) {
                    templateList.Add(index.Value);
                    index = reader.ReadAsInt32();
                }
                pokedex.Templates.Add(templateKey, templateList);
                reader.Read(); // template key or EndObject
            }
        }

        private void ReadOverrides(JsonReader reader, PokedexData pokedex) {
            reader.Read(); // StartObject
            reader.Read(); // override key
            while (reader.TokenType != JsonToken.EndObject) {
                string overrideName = reader.Value.ToString();
                if (overrideName != null) {
                    pokedex.Overrides.Add(overrideName, ReadOverride(reader));
                }
                reader.Read(); // override key or EndObject
            }
        }

        private Dictionary<string, Dictionary<string, string>> ReadOverride(JsonReader reader) {
            reader.Read(); // StartObject
            reader.Read(); // override index
            Dictionary<string, Dictionary<string, string>> overrideData = new Dictionary<string, Dictionary<string, string>>();
            while (reader.TokenType != JsonToken.EndObject) {
                string overrideIndex = reader.Value.ToString();
                Dictionary<string, string> overrideIndexData = new Dictionary<string, string>();
                if (overrideIndex != null) {
                    reader.Read(); // StartObject
                    reader.Read(); // override index data
                    while (reader.TokenType != JsonToken.EndObject) {
                        string overrideIndexDataKey = reader.Value.ToString();
                        overrideIndexData.Add(overrideIndexDataKey, reader.ReadAsString());
                        reader.Read(); // override index data or EndObject
                    }
                }
                overrideData.Add(overrideIndex, overrideIndexData);
                reader.Read(); // override index or EndObject
            }
            return null;
        }
    }
}
