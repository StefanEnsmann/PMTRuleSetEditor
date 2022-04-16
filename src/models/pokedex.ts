import { LocalizationData } from "./application";

export interface PokedexOverrideData {
  typeA: string | null;
  typeB?: string;
}

export interface PokedexGenerationOverrides {
  [index: string]: PokedexOverrideData;
}

export interface PokedexOverrides {
  [index: string]: PokedexGenerationOverrides;
}

export interface PokedexRegion {
  0: number;
  1: number;
}

export interface PokedexRegions {
  [index: string]: PokedexRegion;
}

export interface PokedexTemplates {
  [index: string]: Array<number>;
}

export interface PokemonData {
  localization: LocalizationData;
  nr: number;
  typeA: string;
  typeB: string;
}

export type PokedexList = Array<PokemonData>;

export interface PokedexData {
  overrides: PokedexOverrides;
  regions: PokedexRegions;
  templates: PokedexTemplates;
  list: PokedexList;
}
