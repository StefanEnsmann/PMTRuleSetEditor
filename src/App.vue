<template>
  <MainLayout />
</template>

<script lang="ts">
import { defineComponent } from "vue";
import MainLayout from "./components/layouts/MainLayout.vue";
import { useApplicationStore } from "./store/applicationStore";

export default defineComponent({
  name: "App",
  components: {
    MainLayout,
  },
  setup() {
    const applicationStore = useApplicationStore();

    return {
      applicationStore,
    };
  },
  created() {
    this.axios
      .get("/pkmn_data/pokedex.json")
      .then(({ data }) => (this.applicationStore.pokedexData = data));
  },
});
</script>
