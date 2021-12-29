using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace PokemonTrackerEditor.Model {
    class RuleSetConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(RuleSet);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            RuleSet ruleSet = (RuleSet)value;
            Formatting serializerBackup = serializer.Formatting;
            Formatting writerBackup = writer.Formatting;
            writer.WriteStartObject();

            writer.WritePropertyName("name");
            writer.WriteValue(ruleSet.Name ?? "");

            writer.WritePropertyName("game");
            writer.WriteValue(ruleSet.Game ?? "");

            writer.WritePropertyName("pokedex");
            serializer.Formatting = Formatting.None;
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (PokedexData.Entry entry in ruleSet.Main.Pokedex.List) {
                if (entry.available) {
                    writer.WriteValue(entry.nr);
                }
            }
            writer.WriteEndArray();
            writer.Formatting = writerBackup;
            serializer.Formatting = serializerBackup;

            writer.WritePropertyName("languages");
            serializer.Formatting = Formatting.None;
            writer.Formatting = Formatting.None;
            serializer.Serialize(writer, ruleSet.ActiveLanguages);
            writer.Formatting = writerBackup;
            serializer.Formatting = serializerBackup;

            // maps

            writer.WritePropertyName("story_items");
            serializer.Serialize(writer, ruleSet.StoryItems);

            writer.WritePropertyName("locations");
            writer.WriteStartObject();
            foreach (Location loc in ruleSet.Locations) {
                writer.WritePropertyName(loc.Id);
                serializer.Serialize(writer, loc);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }

    class StoryItemConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(StoryItem);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            StoryItems storyItems = (StoryItems)value;
            writer.WriteStartObject();
            foreach (StoryItemCategory category in storyItems.Categories) {
                writer.WritePropertyName(category.Id);
                writer.WriteStartArray();
                foreach (StoryItem item in category.Items) {
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");
                    writer.WriteValue(item.Id);
                    writer.WritePropertyName("url");
                    writer.WriteValue(item.ImageURL);
                    writer.WritePropertyName("localization");
                    serializer.Serialize(writer, item.Localization);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }

    abstract class DependencyEntryConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            DependencyEntry entry = (DependencyEntry)value;
            writer.WritePropertyName("localization");
            serializer.Serialize(writer, entry.Localization);
            writer.WritePropertyName("conditions");
            Dictionary<string, List<Check>> pairs = new Dictionary<string, List<Check>> { { "items", entry.ItemsConditions }, { "pokemon", entry.PokemonConditions }, { "trainers", entry.TrainersConditions }, { "trades", entry.TradesConditions} };
            writer.WriteStartObject();
            foreach (KeyValuePair<string, List<Check>> pair in pairs) {
                writer.WritePropertyName(pair.Key);
                writer.WriteStartArray();
                foreach (Check check in pair.Value) {
                    writer.WriteStartObject();
                    writer.WritePropertyName("location");
                    writer.WriteValue(check.location.LocationPath);
                    writer.WritePropertyName("id");
                    writer.WriteValue(check.Id);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
            writer.WritePropertyName("story_items");
            serializer.Serialize(writer, entry.StoryItemsConditions);
            writer.WriteEndObject();
        }
    }

    class LocationConverter : DependencyEntryConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Location);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            Location loc = (Location)value;
            writer.WriteStartObject();
            base.WriteJson(writer, value, serializer);

            Dictionary<string, List<Check>> pairs = new Dictionary<string, List<Check>> { { "items", loc.Items }, { "pokemon", loc.Pokemon }, { "trainers", loc.Trainers }, { "trades", loc.Trades } };
            foreach (KeyValuePair<string, List<Check>> pair in pairs) {
                writer.WritePropertyName(pair.Key);
                writer.WriteStartObject();
                foreach (Check check in pair.Value) {
                    writer.WritePropertyName(check.Id);
                    serializer.Serialize(writer, check);
                }
                writer.WriteEndObject();
            }

            writer.WritePropertyName("locations");
            writer.WriteStartObject();
            foreach (Location nestedLoc in loc.Locations) {
                writer.WritePropertyName(nestedLoc.Id);
                serializer.Serialize(writer, nestedLoc);
            }
            writer.WriteEndObject();

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
            return (new List<Type> { typeof(StoryItemCondition), typeof(StoryItemANDCondition), typeof(StoryItemORCondition), typeof(StoryItemsConditions) }.Contains(objectType));
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
                writer.WritePropertyName("type");
                writer.WriteValue(condition is StoryItemANDCondition ? "AND" : "OR");
                writer.WritePropertyName("conditions");
                writer.WriteStartArray();
                foreach (StoryItemConditionBase nestedCondition in conditionalCollection.Conditions) {
                    serializer.Serialize(writer, nestedCondition);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else if (condition is StoryItemCondition nestedCondition) {
                writer.WriteStartObject();
                writer.WritePropertyName("category");
                writer.WriteValue(nestedCondition.StoryItem.Category.Id);
                writer.WritePropertyName("id");
                writer.WriteValue(nestedCondition.StoryItem.Id);
                writer.WriteEndObject();
            }
        }
    }

    class LocalizationConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Localization);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            Localization loc = (Localization)value;
            writer.WriteStartObject();
            foreach (KeyValuePair<string, string> pair in loc.Translations) {
                writer.WritePropertyName(pair.Key);
                writer.WriteValue(pair.Value);
            }
            writer.WriteEndObject();
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
            throw new NotImplementedException();
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
