<template>
  <q-page class="width-full">
    <q-list>
      <pokedex-region-list region="kanto"></pokedex-region-list>
      <pokedex-region-list region="johto"></pokedex-region-list>
      <pokedex-region-list region="hoenn"></pokedex-region-list>
    </q-list>
  </q-page>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import PokedexRegionList from 'components/pokedex/PokedexRegionList.vue';

export default defineComponent({
  name: 'PokedexPage',
  components: { PokedexRegionList },
  setup() {
    return {};
  },
});
</script>
