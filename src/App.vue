<template>
  <router-view />
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { api } from 'boot/axios';
import { useAppStore } from 'stores/app';

export default defineComponent({
  name: 'App',
  setup: () => {
    const store = useAppStore();

    api.get('/pkmn_data/pokedex.json').then(({ data }) => {
      store.pokedexData = data;
    });
  },
});
</script>
