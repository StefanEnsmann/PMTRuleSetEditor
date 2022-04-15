<template>
  <MainLayout />
</template>

<script lang="ts">
import { defineComponent } from "vue";
import MainLayout from "./components/layouts/MainLayout.vue";
import { usePokedexStore } from "./store/pokedexStore";

export default defineComponent({
  name: "App",
  components: {
    MainLayout,
  },
  setup() {
    const pokedexStore = usePokedexStore();

    return {
      pokedexStore,
    };
  },
  created() {
    this.axios.get("/pkmn_data/pokedex.json").then(({ data }) => {
      this.pokedexStore.overrides = data.overrides;
      this.pokedexStore.regions = data.regions;
      this.pokedexStore.templates = data.templates;
      this.pokedexStore.list = data.list;
    });
  },
});
</script>
