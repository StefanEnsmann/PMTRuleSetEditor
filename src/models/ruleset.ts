import { LocalizationData } from "./application";

export type Pokedex = Array<number>;

export interface StoryItem {
  id: string;
  url: string | null;
  localization: LocalizationData;
}

export interface StoryItemCategory {
  id: string;
  localization: LocalizationData;
  items: Array<StoryItem>;
}

export type StoryItems = Array<StoryItemCategory>;

export interface Condition {
  logic: "AND" | "OR" | "NOT";
  list: Array<Condition | string>;
}

export interface Check {
  id: string;
  localization: LocalizationData;
  conditions?: Condition;
}

export interface Item extends Check {}

export interface Trainer extends Check {}

export interface Pokemon extends Check {}

export interface Location extends Check {
  items?: Array<Item>;
  trainers?: Array<Trainer>;
  pokemon?: Array<Pokemon>;
  locations?: Array<Location>;
}

export interface RulesetData {
  name: string;
  game: string;
  languages: Array<"de" | "en" | "fr" | "ja" | "ko" | "zh-CN" | "zh-TW">;
  pokedex: Pokedex;
  story_items: StoryItems;
  locations: Location;
}
