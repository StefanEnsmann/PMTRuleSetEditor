<template>
  <PokemonCardVue v-for="pkmn in pokemonForRegion" :pkmn="pkmn" />
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { PokedexList } from "../../models/pokedex";
import { usePokedexStore } from "../../store/pokedexStore";
import PokemonCardVue from "./PokemonCard.vue";

export default defineComponent({
  name: "PokedexRegionList",
  components: {
    PokemonCardVue,
  },
  props: {
    region: String,
  },
  setup() {
    const pokedexStore = usePokedexStore();

    return { pokedexStore };
  },
  computed: {
    pokemonForRegion(): PokedexList {
      const regionLimits = this.pokedexStore.getRegionByName(this.region ?? "");
      return this.pokedexStore.list.filter(function (pkmn) {
        return regionLimits[0] <= pkmn.nr && pkmn.nr <= regionLimits[1];
      });
    },
  },
});
</script>
