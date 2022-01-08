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

        public static RuleSet ruleSet;
        public static DependencyEntry currentDependencyEntry;
        public static ILocationContainer currentLocationContainer;
        public static StoryItemBase currentStoryItemBase;
        public static StoryItemConditionCollection currentStoryItemConditionCollection;
        public static Dictionary<string, List<string>> dependencyCollections;
        public static bool localizationIsStoryItem;

        public static void Init() {
            ruleSet = null;
            currentDependencyEntry = null;
            currentLocationContainer = null;
            currentStoryItemConditionCollection = null;
            currentStoryItemBase = null;
            localizationIsStoryItem = true;
            if (dependencyCollections == null) {
                dependencyCollections = new Dictionary<string, List<string>>();
            }
            else {
                dependencyCollections.Clear();
            }
        }

        public static void SolveDependencies() {
            foreach (KeyValuePair<string, List<string>> dependencies in dependencyCollections) {
                DependencyEntry entry = ruleSet.ResolvePath(dependencies.Key);
                foreach (string path in dependencies.Value) {
                    Check check = ruleSet.ResolvePath(path) as Check;
                    entry.AddCondition(check);
                }
            }
        }
    }

    abstract class CustomConverter : JsonConverter {
        private static readonly StartObjectClass soInstance = new StartObjectClass();
        private static readonly EndObjectClass eoInstance = new EndObjectClass();
        private static readonly StartArrayClass saInstance = new StartArrayClass();
        private static readonly EndArrayClass eaInstance = new EndArrayClass();
        public static StartObjectClass StartObject => soInstance;
        public static EndObjectClass EndObject => eoInstance;
        public static StartArrayClass StartArray => saInstance;
        public static EndArrayClass EndArray => eaInstance;

        public abstract class ReadParam {

            public class TokenException : System.Exception {
                public TokenException() : base() { }
                public TokenException(string message) : base(message) { }
            }

            private readonly string v;
            protected abstract JsonToken Type { get; }

            public ReadParam(string value = null) {
                v = value;
            }

            public bool Match(JsonReader reader) {
                return reader.TokenType == Type && (v == null || v.Equals(reader.Value));
            }

            public override string ToString() {
                return string.Format("({0}: {1})", Type.ToString(), v);
            }
        }

        public class Property : ReadParam {
            protected override JsonToken Type => JsonToken.PropertyName;
            private static readonly Dictionary<string, Property> instances = new Dictionary<string, Property>();
            private static readonly Property nullValue = new Property();

            private Property(string value=null) : base(value) { }

            public static Property Name(string value = null) {
                Property ret;
                if (value == null) {
                    ret = nullValue;
                }
                else if (!instances.TryGetValue(value, out ret)) {
                    ret = new Property(value);
                    instances.Add(value, ret);
                }
                return ret;
            }
        }
        public class StartObjectClass : ReadParam { protected override JsonToken Type => JsonToken.StartObject; public StartObjectClass() : base() { } }
        public class EndObjectClass : ReadParam { protected override JsonToken Type => JsonToken.EndObject; public EndObjectClass() : base() { } }
        public class StartArrayClass : ReadParam { protected override JsonToken Type => JsonToken.StartArray; public StartArrayClass() : base() { } }
        public class EndArrayClass : ReadParam { protected override JsonToken Type => JsonToken.EndArray; public EndArrayClass() : base() { } }


        private const bool DEBUG = false;
        private static int indentation = 0;

        public void Log(string message) {
            for (int i = 0; i < indentation; ++i) {
                Console.Write(" ");
            }
            Console.WriteLine(message);
        }

        public void Read(JsonReader reader, params ReadParam[] validTokens) {
            reader.Read();
            if (DEBUG)
                Log("Read: " + reader.TokenType + " " + reader.Value?.ToString());
            bool valid = validTokens.Length == 0;
            foreach (ReadParam param in validTokens) {
                if (param.Match(reader)) {
                    valid = true;
                    break;
                }
            }
            if (!valid) {
                throw new ReadParam.TokenException(
                    string.Format(
                        "Token ({0}: {1}) did not match the expected list:\n{2}",
                        reader.TokenType.ToString(),
                        reader.Value,
                        string.Join<ReadParam>(", ", validTokens)
                    )
                );
            }
        }

        public string ReadPropertyString(JsonReader reader, string property) {
            Read(reader, Property.Name(property));
            return ReadAsString(reader);
        }

        public int? ReadPropertyInt(JsonReader reader, string property) {
            Read(reader, Property.Name(property));
            return ReadAsInt32(reader);
        }

        public string ReadAsString(JsonReader reader) {
            string s = reader.ReadAsString();
            if (DEBUG)
                Log("ReadAsString: " + s);
            return s;
        }

        public int? ReadAsInt32(JsonReader reader) {
            int? i = reader.ReadAsInt32();
            if (DEBUG)
                Log("ReadAsInt32: " + i);
            return i;
        }

        public string StringValue(JsonReader reader) {
            try {
                string s = reader.Value.ToString();
                if (DEBUG)
                    Log("StringValue: " + s);
                return s;
            }
            catch (Exception e) {
                Log("ERROR in StringValue: " + reader.TokenType + " " + reader.Value);
                throw e;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (DEBUG) {
                Log("\n\n");
                Log("-----------------------------------");
                Log("Starting deserialization of type " + objectType.ToString());
            }
            return null;
        }

        public void Indent() {
            indentation += 4;
        }

        public void Unindent() {
            indentation -= 4;
        }
    }

    class RuleSetConverter : CustomConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(RuleSet);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            base.ReadJson(reader, objectType, existingValue, serializer);
            DependencyCache.Init();
            RuleSet ruleSet = new RuleSet();
            DependencyCache.ruleSet = ruleSet;
            ruleSet.Name = ReadPropertyString(reader, "name");
            ruleSet.Game = ReadPropertyString(reader, "game");

            serializer.Deserialize(reader, typeof(PokedexRules));

            Read(reader, Property.Name("languages"));
            Indent();
            Read(reader, StartArray);
            string str = ReadAsString(reader);
            while (str != null) {
                ruleSet.SetLanguageActive(str);
                str = ReadAsString(reader);
            }
            Unindent();

            serializer.Deserialize(reader, typeof(StoryItems));

            // maps
            Read(reader, Property.Name("locations"));
            Indent();
            Read(reader, StartArray);
            DependencyCache.currentLocationContainer = ruleSet;
            while (serializer.Deserialize(reader, typeof(Location)) != null) ;
            Unindent();

            Read(reader, EndObject);
            DependencyCache.SolveDependencies();

            return ruleSet;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            RuleSet ruleSet = (RuleSet)value;
            DependencyCache.Init();
            DependencyCache.ruleSet = ruleSet;

            writer.WriteStartObject();

            writer.WritePropertyName("name"); writer.WriteValue(ruleSet.Name ?? "");

            writer.WritePropertyName("game"); writer.WriteValue(ruleSet.Game ?? "");

            serializer.Serialize(writer, ruleSet.Pokedex);

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

    class StoryItemConverter : CustomConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(StoryItem);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            base.ReadJson(reader, objectType, existingValue, serializer);
            DependencyCache.localizationIsStoryItem = true;
            RuleSet ruleSet = DependencyCache.ruleSet;
            StoryItems storyItems = ruleSet.StoryItems;
            Read(reader, Property.Name("story_items"));
            Indent();
            Read(reader, StartArray);
            Read(reader, StartObject, EndArray);
            while (reader.TokenType != JsonToken.EndArray) {
                Indent();
                StoryItemCategory category = storyItems.AddStoryItemCategory(ReadPropertyString(reader, "id"));
                DependencyCache.currentStoryItemBase = category;
                Read(reader, Property.Name("localization"));
                category.Localization.Cleanup();
                category.Localization = (Localization)serializer.Deserialize(reader, typeof(Localization));
                Read(reader, Property.Name("items"));
                Read(reader, StartArray);
                Read(reader, StartObject, EndArray);
                while (reader.TokenType != JsonToken.EndArray) {
                    Indent();
                    StoryItem item = category.AddStoryItem(ReadPropertyString(reader, "id"));
                    DependencyCache.currentStoryItemBase = item;
                    item.ImageURL = ReadPropertyString(reader, "url");
                    Read(reader, Property.Name("localization"));
                    item.Localization.Cleanup();
                    item.Localization = (Localization)serializer.Deserialize(reader, typeof(Localization));
                    Read(reader, EndObject);
                    Unindent();
                    Read(reader, StartObject, EndArray);
                }
                Read(reader, EndObject);
                Unindent();
                Read(reader, StartObject, EndArray);
            }
            Unindent();
            DependencyCache.localizationIsStoryItem = false;
            return storyItems;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            StoryItems storyItems = (StoryItems)value;
            writer.WritePropertyName("story_items");
            writer.WriteStartArray();
            foreach (StoryItemCategory category in storyItems.Categories) {
                writer.WriteStartObject();
                writer.WritePropertyName("id"); writer.WriteValue(category.Id);
                writer.WritePropertyName("localization"); serializer.Serialize(writer, category.Localization);
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

    abstract class DependencyEntryConverter : CustomConverter {
        public override bool CanConvert(Type objectType) {
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            base.ReadJson(reader, objectType, existingValue, serializer);
            Read(reader); // StartObject or something else
            if (reader.TokenType == JsonToken.StartObject) {
                Indent();
                DependencyEntry entry;
                string id = ReadPropertyString(reader, "id");
                switch (DependencyCache.nextType) {
                    case DependencyCache.NextType.ITEM:
                        entry = (DependencyCache.currentLocationContainer as Location).AddCheck(id, Check.Type.ITEM);
                        break;
                    case DependencyCache.NextType.POKEMON:
                        entry = (DependencyCache.currentLocationContainer as Location).AddCheck(id, Check.Type.POKEMON);
                        break;
                    case DependencyCache.NextType.TRAINER:
                        entry = (DependencyCache.currentLocationContainer as Location).AddCheck(id, Check.Type.TRAINER);
                        break;
                    case DependencyCache.NextType.TRADE:
                        entry = (DependencyCache.currentLocationContainer as Location).AddCheck(id, Check.Type.TRADE);
                        break;
                    case DependencyCache.NextType.LOCATION:
                        entry = DependencyCache.currentLocationContainer.AddLocation(id);
                        break;
                    default:
                        Unindent();
                        return null;
                }
                DependencyCache.currentDependencyEntry = entry;
                Read(reader, Property.Name("localization"));
                entry.Localization.Cleanup();
                entry.Localization = (Localization)serializer.Deserialize(reader, typeof(Localization));
                Read(reader, Property.Name(), EndObject); // "conditions" or EndObject or property from subclass
                if (reader.TokenType == JsonToken.PropertyName && StringValue(reader) == "conditions") {
                    Indent();
                    Read(reader, StartObject);
                    Read(reader, Property.Name(), EndObject);
                    List<string> conditionsCache = new List<string>();
                    while (reader.TokenType != JsonToken.EndObject) {
                        string type = StringValue(reader);
                        switch (type) {
                            case "items":
                            case "pokemon":
                            case "trainers":
                            case "trades":
                                ReadConditionsList(reader, type, conditionsCache);
                                break;
                            case "story_items":
                                DependencyCache.currentStoryItemConditionCollection = entry.StoryItemsConditions;
                                serializer.Deserialize(reader, typeof(StoryItemConditionCollection));
                                break;
                            default:
                                Console.WriteLine("ERROR! Unknown dependency condition: " + type);
                                break;
                        }
                        Read(reader, Property.Name(), EndObject);
                    }
                    if (conditionsCache.Count > 0) {
                        DependencyCache.dependencyCollections.Add(entry.FullPath, conditionsCache);
                    }
                    Unindent();
                    Read(reader, Property.Name(), EndObject); // EndObject or property from subclass
                }
                Unindent();
                return entry;
            }
            else {
                return null;
            }
        }

        private void ReadConditionsList(JsonReader reader, string type, List<string> conditionsCache) {
            Indent();
            Read(reader, StartArray);
            Read(reader, StartObject, EndArray);
            while (reader.TokenType != JsonToken.EndArray) {
                string cond = string.Join(".", new string[] { ReadPropertyString(reader, "location"), type, ReadPropertyString(reader, "id") });
                conditionsCache.Add(cond);
                Read(reader, EndObject);
                Read(reader, StartObject, EndArray);
            }
            Unindent();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            DependencyEntry entry = (DependencyEntry)value;
            writer.WritePropertyName("id"); writer.WriteValue(entry.Id);
            writer.WritePropertyName("localization"); serializer.Serialize(writer, entry.Localization);
            Dictionary<string, Dictionary<Check, Gtk.TreeIter>> pairs = new Dictionary<string, Dictionary<Check, Gtk.TreeIter>> { { "items", entry.ItemsConditions }, { "pokemon", entry.PokemonConditions }, { "trainers", entry.TrainersConditions }, { "trades", entry.TradesConditions } };
            bool hasOpened = false;
            foreach (KeyValuePair<string, Dictionary<Check, Gtk.TreeIter>> pair in pairs) {
                if (pair.Value.Count > 0) {
                    if (!hasOpened) {
                        writer.WritePropertyName("conditions"); writer.WriteStartObject();
                        hasOpened = true;
                    }
                    writer.WritePropertyName(pair.Key); writer.WriteStartArray();
                    foreach (Check check in pair.Value.Keys) {
                        writer.WriteStartObject();
                        Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
                        writer.WritePropertyName("location"); writer.WriteValue((check.Parent as Location).LocationPath());
                        writer.WritePropertyName("id"); writer.WriteValue(check.Id);
                        writer.WriteEndObject();
                        writer.Formatting = writerBackup;
                    }
                    writer.WriteEndArray();
                }
            }
            if (entry.StoryItemsConditions.Count > 0) {
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
        private static readonly Dictionary<string, DependencyCache.NextType> typeMap = new Dictionary<string, DependencyCache.NextType>{
            { "items", DependencyCache.NextType.ITEM },
            { "pokemon", DependencyCache.NextType.POKEMON },
            { "trainers", DependencyCache.NextType.TRAINER },
            { "trades", DependencyCache.NextType.TRADE },
            { "locations", DependencyCache.NextType.LOCATION }
        };

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Location);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            DependencyCache.nextType = DependencyCache.NextType.LOCATION;
            if (base.ReadJson(reader, objectType, existingValue, serializer) is Location loc) {
                Indent();
                ILocationContainer parentContainer = DependencyCache.currentLocationContainer;
                DependencyCache.currentLocationContainer = loc;
                while (reader.TokenType != JsonToken.EndObject) {
                    DependencyCache.nextType = typeMap[StringValue(reader)];
                    Read(reader, StartArray);
                    Type t = DependencyCache.nextType == DependencyCache.NextType.LOCATION ? typeof(Location) : typeof(Check);
                    while (serializer.Deserialize(reader, t) != null) ; // EndArray afterwards
                    Read(reader, Property.Name(), EndObject); // property name or EndObject
                }
                DependencyCache.currentLocationContainer = parentContainer;
                Unindent();
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

            if (loc.Locations.Count > 0) {
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
            object o = base.ReadJson(reader, objectType, existingValue, serializer);
            return o;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteStartObject();
            base.WriteJson(writer, value, serializer);
            writer.WriteEndObject();
        }
    }

    class StoryItemConditionConverter : CustomConverter {
        public override bool CanConvert(Type objectType) {
            return (new List<Type> { typeof(StoryItemCondition), typeof(StoryItemANDCondition), typeof(StoryItemORCondition), typeof(StoryItemNOTCondition), typeof(StoryItemsConditions) }.Contains(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            base.ReadJson(reader, objectType, existingValue, serializer);
            if (objectType == typeof(StoryItemConditionCollection)) {
                Indent();
                Read(reader, StartArray);
                StoryItemConditionBase conditionBase = serializer.Deserialize(reader, typeof(StoryItemConditionBase)) as StoryItemConditionBase;
                while (conditionBase != null) {
                    conditionBase = serializer.Deserialize(reader, typeof(StoryItemConditionBase)) as StoryItemConditionBase;
                }
                Unindent();
            }
            else if (objectType == typeof(StoryItemConditionBase)) {
                Read(reader, StartObject, EndArray);
                Indent();
                if (reader.TokenType == JsonToken.StartObject) {
                    Read(reader, Property.Name("type"), Property.Name("category"));
                    string type = StringValue(reader);
                    if (type == "type") {
                        StoryItemConditionCollection collection;
                        string collectionType = ReadAsString(reader);
                        switch (collectionType) {
                            case "AND":
                                collection = DependencyCache.currentStoryItemConditionCollection.AddANDCollection();
                                break;
                            case "OR":
                                collection = DependencyCache.currentStoryItemConditionCollection.AddORCollection();
                                break;
                            case "NOT":
                                collection = DependencyCache.currentStoryItemConditionCollection.AddNOTCollection();
                                break;
                            default:
                                Unindent();
                                return null;
                        }
                        Read(reader, Property.Name("conditions"));
                        StoryItemConditionCollection backup = DependencyCache.currentStoryItemConditionCollection;
                        DependencyCache.currentStoryItemConditionCollection = collection;
                        serializer.Deserialize(reader, typeof(StoryItemConditionCollection));
                        DependencyCache.currentStoryItemConditionCollection = backup;
                        Read(reader, EndObject);
                        Unindent();
                        return collection;
                    }
                    else if (type == "category") {
                        string category = ReadAsString(reader);
                        string item = ReadPropertyString(reader, "id");
                        StoryItemCondition condition = DependencyCache.currentStoryItemConditionCollection.AddStoryItemCondition(DependencyCache.ruleSet.StoryItems.FindStoryItem(category, item));
                        Read(reader, EndObject);
                        Unindent();
                        return condition;
                    }
                }
                Unindent();
            }
            return null;
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

    class LocalizationConverter : CustomConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Localization);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            base.ReadJson(reader, objectType, existingValue, serializer);
            Localization loc;
            if (DependencyCache.localizationIsStoryItem) {
                loc = new Localization(DependencyCache.currentStoryItemBase);
            }
            else {
                loc = new Localization(DependencyCache.currentDependencyEntry);
            }
            Indent();
            Read(reader, StartObject);
            Read(reader, Property.Name(), EndObject);
            while (reader.TokenType != JsonToken.EndObject) {
                string id = StringValue(reader);
                loc[id] = ReadAsString(reader);
                Read(reader, Property.Name(), EndObject);
            }
            Unindent();
            return loc;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            Localization loc = (Localization)value;
            writer.WriteStartObject();
            Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
            foreach (string language in DependencyCache.ruleSet.ActiveLanguages) {
                writer.WritePropertyName(language); writer.WriteValue(loc[language]);
            }
            writer.WriteEndObject();
            writer.Formatting = writerBackup;
        }
    }

    class PokedexRulesConverter : CustomConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(PokedexRules);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            Read(reader, Property.Name("pokedex"));
            Indent();
            Read(reader, StartArray);
            int? idx = ReadAsInt32(reader);
            while (idx.HasValue) {
                DependencyCache.ruleSet.Pokedex.List[idx.Value - 1].Available = true;
                idx = ReadAsInt32(reader);
            }
            Unindent();
            return DependencyCache.ruleSet.Pokedex;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            PokedexRules rules = (PokedexRules)value;
            writer.WritePropertyName("pokedex");
            Formatting writerBackup = writer.Formatting; writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            foreach (PokedexRules.Entry entry in rules.List) {
                if (entry.Available) {
                    writer.WriteValue(entry.Nr);
                }
            }
            writer.WriteEndArray();
            writer.Formatting = writerBackup;
        }
    }

    class PokedexDataConverter : CustomConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Pokedex);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            base.ReadJson(reader, objectType, existingValue, serializer);
            Pokedex pokedex = new Pokedex();
            Read(reader, Property.Name(), EndObject);
            while (reader.TokenType != JsonToken.EndObject) {
                string currentPosition = StringValue(reader);
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
                Read(reader, Property.Name(), EndObject);
            }
            return pokedex;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        private void ReadList(JsonReader reader, Pokedex pokedex) {
            Read(reader, StartArray);
            Read(reader, StartObject, EndArray);
            while (reader.TokenType != JsonToken.EndArray) {
                pokedex.AddPokemon(ReadListEntry(reader));
                Read(reader, StartObject, EndArray);
            }
        }

        private Pokedex.Entry ReadListEntry(JsonReader reader) {
            Pokedex.Entry entry = new Pokedex.Entry();
            Read(reader, Property.Name(), EndObject);
            while (reader.TokenType != JsonToken.EndObject) {
                string entryKey = StringValue(reader);
                switch (entryKey) {
                    case "localization":
                        ReadLocalization(reader, entry);
                        break;
                    case "nr":
                        entry.nr = ReadAsInt32(reader) ?? -1;
                        break;
                    case "typeA":
                        entry.typeA = ReadAsString(reader);
                        break;
                    case "typeB":
                        entry.typeB = ReadAsString(reader);
                        break;
                }
                Read(reader, Property.Name(), EndObject);
            }
            return entry;
        }

        private void ReadLocalization(JsonReader reader, Pokedex.Entry entry) {
            Read(reader, StartObject);
            Read(reader, Property.Name(), EndObject);
            while (reader.TokenType != JsonToken.EndObject) {
                entry.Localization.Add(StringValue(reader), ReadAsString(reader));
                Read(reader, Property.Name(), EndObject);
            }
        }

        private void ReadTemplates(JsonReader reader, Pokedex pokedex) {
            Read(reader, StartObject);
            Read(reader, Property.Name(), EndObject);
            while (reader.TokenType != JsonToken.EndObject) {
                string templateKey = StringValue(reader);
                List<int> templateList = new List<int>();
                Read(reader, StartArray);
                int? index = ReadAsInt32(reader);
                while (index.HasValue) {
                    templateList.Add(index.Value);
                    index = ReadAsInt32(reader);
                }
                pokedex.Templates.Add(templateKey, templateList);
                Read(reader, Property.Name(), EndObject);
            }
        }

        private void ReadOverrides(JsonReader reader, Pokedex pokedex) {
            Read(reader, StartObject);
            Read(reader, Property.Name());
            while (reader.TokenType != JsonToken.EndObject) {
                string overrideName = StringValue(reader);
                if (overrideName != null) {
                    pokedex.Overrides.Add(overrideName, ReadOverride(reader));
                }
                Read(reader, Property.Name(), EndObject);
            }
        }

        private Dictionary<string, Dictionary<string, string>> ReadOverride(JsonReader reader) {
            Read(reader, StartObject);
            Read(reader, Property.Name());
            Dictionary<string, Dictionary<string, string>> overrideData = new Dictionary<string, Dictionary<string, string>>();
            while (reader.TokenType != JsonToken.EndObject) {
                string overrideIndex = StringValue(reader);
                Dictionary<string, string> overrideIndexData = new Dictionary<string, string>();
                if (overrideIndex != null) {
                    Read(reader, StartObject);
                    Read(reader, Property.Name(), EndObject);
                    while (reader.TokenType != JsonToken.EndObject) {
                        string overrideIndexDataKey = StringValue(reader);
                        overrideIndexData.Add(overrideIndexDataKey, ReadAsString(reader));
                        Read(reader, Property.Name(), EndObject);
                    }
                }
                overrideData.Add(overrideIndex, overrideIndexData);
                Read(reader, Property.Name(), EndObject);
            }
            return null;
        }
    }
}
