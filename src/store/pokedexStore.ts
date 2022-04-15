import { PokedexData } from "../models/pokedex";
import { defineStore } from "pinia";

export const usePokedexStore = defineStore("pokedex", {
  state: () => {
    return {
      overrides: {},
      regions: {},
      templates: {},
      list: [],
    } as PokedexData;
  },
  getters: {
    getRegionByName: (state) => {
      return (region: string) => state.regions[region];
    },
    getTemplateByName: (state) => {
      return (template: string) => state.templates[template];
    },
    getPokemonById: (state) => {
      return (pokemonId: number) => state.list[pokemonId];
    },
  },
});
