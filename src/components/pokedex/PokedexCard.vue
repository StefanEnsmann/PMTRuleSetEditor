<template>
  <div class="col-xs-6 col-sm-4 col-md-3 col-lg-2 col-xl-1">
    <q-card>
      <q-card-section> #{{ pokemonId }} - {{ pokemonName }} </q-card-section>
      <q-separator inset />
      <q-card-section>
        <img :src="imageURL" class="full-width" />
      </q-card-section>
    </q-card>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { useAppStore } from '../../stores/app';

export default defineComponent({
  name: 'PokedexCard',
  props: {
    pokemonId: Number,
  },
  setup() {
    const appStore = useAppStore();

    return { appStore };
  },
  computed: {
    isActive() {
      return true;
    },
    pokemonName() {
      return this.appStore.pokedexData.list[this.pokemonId - 1].localization.en;
    },
    imageURL() {
      let slug =
        String(this.pokemonId).padStart(4, '0') +
        '_' +
        this.$pokemonNameEncoding(this.pokemonName);
      return `https://pkmntracker.ensmann.de/img/pkmn/${slug}`;
    },
  },
});
</script>
