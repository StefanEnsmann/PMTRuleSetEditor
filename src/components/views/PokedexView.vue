<template>
  <n-collapse>
    <n-collapse-item v-for="region in regions" :title="region">
      <PokedexRegionListVue :region="region" />
    </n-collapse-item>
  </n-collapse>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { NCollapse, NCollapseItem } from "naive-ui";
import { usePokedexStore } from "../../store/pokedexStore";
import PokedexRegionListVue from "../pokedex/PokedexRegionList.vue";

export default defineComponent({
  name: "PokedexView",
  components: {
    PokedexRegionListVue,
    NCollapse,
    NCollapseItem,
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
