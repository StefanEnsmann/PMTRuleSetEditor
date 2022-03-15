<template>
  <div class="pokedex-view">
    <n-grid cols="3 s:4 m:5 l:6 xl:7 2xl:8" responsive="screen">
      <n-grid-item v-for="pokemon in pokedex">
        <PokemonCard
          :id="pokemon.nr"
          :name="pokemon.localization.en"
          :typeA="pokemon.typeA"
          :typeB="pokemon.typeB"
        />
      </n-grid-item>
      <n-grid-item>
      </n-grid-item>
    </n-grid>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { NGrid, NGridItem } from 'naive-ui'
import PokemonCard from '../pokedex/PokemonCard.vue'
import { useStore } from "../../store"
import { IPokemonData } from '../../models/pokedex'

export default defineComponent({
  name: 'PokedexView',
  components: {
    PokemonCard,
    NGrid,
    NGridItem
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

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h3 {
  margin: 40px 0 0;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
</style>
