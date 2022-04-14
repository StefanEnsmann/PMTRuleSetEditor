<template></template>

<script lang="ts">
import { defineComponent } from "vue";
import { useApplicationStore } from "../store/applicationStore";

export default defineComponent({
  name: "MainLayout",
  setup() {
    const applicationStore = useApplicationStore();

    return { applicationStore };
  },
  created() {
    this.axios
      .get("/game_data/pokedex.json")
      .then((data) => (this.applicationStore.pokedexData = data));
  },
});
</script>
