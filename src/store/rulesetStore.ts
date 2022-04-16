import { RulesetData } from "../models/ruleset";
import { defineStore } from "pinia";

export const usePokedexStore = defineStore("ruleset", {
  state: () => {
    return {} as RulesetData;
  },
});
