<template>
  <q-page class="width-full">
    <q-list>
      <pokedex-region-list
        v-for="region in regions"
        :region="region"
        :key="region"
      />
    </q-list>
  </q-page>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import PokedexRegionList from 'components/pokedex/PokedexRegionList.vue';
import { useAppStore } from '../stores/app';

export default defineComponent({
  name: 'PokedexPage',
  components: { PokedexRegionList },
  setup() {
    const appStore = useAppStore();

    return {
      appStore,
    };
  },
  computed: {
    regions() {
      return Object.keys(this.appStore.pokedexData.regions);
    },
  },
});
</script>
