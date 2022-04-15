<template>
  <n-list class="p-1 overflow-y-scroll">
    <PokedexRegionListVue v-for="region in regions" :region="region" />
  </n-list>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { NList } from "naive-ui";
import { usePokedexStore } from "../../store/pokedexStore";
import PokedexRegionListVue from "../pokedex/PokedexRegionList.vue";

export default defineComponent({
  name: "PokedexView",
  components: {
    PokedexRegionListVue,
    NList,
  },
  setup() {
    const pokedexStore = usePokedexStore();

    return { pokedexStore };
  },
  computed: {
    regions(): Array<string> {
      return Object.keys(this.pokedexStore.regions);
    },
  },
});
</script>
