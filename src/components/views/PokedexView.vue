<template>
  <n-list class="p-1 overflow-y-scroll">
    <PokemonCard
      v-for="pokemon in pokedex"
      :id="pokemon.nr"
      :name="pokemon.localization.en"
      :typeA="pokemon.typeA"
      :typeB="pokemon.typeB"
    />
  </n-list>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { NList } from "naive-ui";
import PokemonCard from "../pokedex/PokemonCard.vue";
import { IPokemonData } from "../../models/pokedex";

export default defineComponent({
  name: "PokedexView",
  components: {
    PokemonCard,
    NList,
  },
  props: {
    msg: String,
  },
});
</script>
