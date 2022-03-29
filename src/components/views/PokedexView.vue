<template>
  <n-list class="p-1 overflow-y-scroll">
      <PokemonCard v-for="pokemon in pokedex"
        :id="pokemon.nr"
        :name="pokemon.localization.en"
        :typeA="pokemon.typeA"
        :typeB="pokemon.typeB"
      />
  </n-list>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { NList } from 'naive-ui'
import PokemonCard from '../pokedex/PokemonCard.vue'
import { useStore } from "../../store"
import { IPokemonData } from '../../models/pokedex'

export default defineComponent({
  name: 'PokedexView',
  components: {
    PokemonCard,
    NList
  },
  props: {
    msg: String
  },
  computed: {
    pokedex(): IPokemonData[] {
      let store = useStore()
      if (store === undefined || store.state.externals.pokedexData === undefined) {
        return [];
      } else {
        return store.state.externals.pokedexData.list
      }
    }
  }
})
</script>
