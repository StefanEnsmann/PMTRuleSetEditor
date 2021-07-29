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
            writer.WriteValue("pokedex/gen2.json"); // TODO: Write actual value

            writer.WritePropertyName("languages");
            serializer.Serialize(writer, ruleSet.ActiveLanguages);

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
                    writer.WriteValue(check.location.Id);
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
}
