<template>
  <div class="pokedex-view">
    <n-grid cols="3 s:4 m:5 l:6 xl:7 2xl:8" responsive="screen">
      <n-grid-item>
        <PokemonCard
          id=001
          name="Bulbasaur"
          imgSrc="https://pkmntracker.ensmann.de/img/pkmn/0001_bulbasaur.png"
          typeA="grass"
          typeB="poison"
        />
      </n-grid-item>
      <n-grid-item>
        <PokemonCard
          id=002
          name="Ivysaur"
          imgSrc="https://pkmntracker.ensmann.de/img/pkmn/0002_ivysaur.png"
          typeA="grass"
          typeB="poison"
        />
      </n-grid-item>
      <n-grid-item>
        <PokemonCard
          id=003
          name="Venusaur"
          imgSrc="https://pkmntracker.ensmann.de/img/pkmn/0003_venusaur.png"
          typeA="grass"
          typeB="poison"
        />
      </n-grid-item>
    </n-grid>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { NGrid, NGridItem } from 'naive-ui'
import PokemonCard from '../pokedex/PokemonCard.vue'

export default defineComponent({
  name: 'PokedexView',
  components: {
    PokemonCard,
    NGrid,
    NGridItem
  },
  props: {
    msg: String
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
