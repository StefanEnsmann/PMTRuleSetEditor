<template>
  <div class="row q-col-gutter-x-lg q-col-gutter-y-lg q-px-lg">
    <pokedex-card
      v-for="pokemonId in pokemonIds"
      :key="pokemonId"
      :pokemon-id="pokemonId"
    />
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import PokedexCard from './PokedexCard.vue';
import { useAppStore } from '../../stores/app';

export default defineComponent({
  name: 'PokedexCardGrid',
  components: { PokedexCard },
  props: {
    region: String,
  },
  setup() {
    const appStore = useAppStore();

    return { appStore };
  },
  computed: {
    pokemonIds() {
      return this.appStore.pokedexData.regions[this.region];
    },
  },
});
</script>
