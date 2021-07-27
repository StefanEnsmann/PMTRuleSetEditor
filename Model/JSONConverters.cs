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
            writer.WriteStartObject();
            writer.WritePropertyName("pokedex");
            // write value
            writer.WriteValue("pokedex/gen2.json");
            writer.WritePropertyName("languages");
            serializer.Serialize(writer, ruleSet.ActiveLanguages);
            // maps
            writer.WritePropertyName("story_items");
            serializer.Serialize(writer, ruleSet.StoryItems);
            //writer.WritePropertyName("locations");
            //foreach (Location loc in ruleSet.Locations) {
            //    writer.WritePropertyName(loc.Id);
            //    serializer.Serialize(writer, loc);
            //}
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
                writer.WriteStartObject();
                foreach (StoryItem item in category.Items) {
                    writer.WritePropertyName(item.Id);
                    writer.WriteStartObject();
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
    }

    class DependencyEntryConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }

    class LocationConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }

    class CheckConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }

    class StoryItemConditionConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
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
            foreach (KeyValuePair<string, LocalizationEntry> pair in loc.Translation) {
                writer.WritePropertyName(pair.Key);
                writer.WriteValue(pair.Value.Value);
            }
            writer.WriteEndObject();
        }
    }
}
