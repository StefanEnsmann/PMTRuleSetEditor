<template>
  <div class="flex-grow-0 p-2 bg-white">
    <n-space :size="[100, 0]">
      <n-statistic label="Checks" value="123"> </n-statistic>
      <n-statistic label="Story Items" value="123"> </n-statistic>
      <n-statistic label="Pokedex" value="0">
        <template #suffix> / {{ pokedexStore.list.length }} </template>
      </n-statistic>
      <n-statistic label="Maps" value="123"> </n-statistic>
    </n-space>
    <n-divider class="flex-grow-0 my-2" />
    <n-space>
      <router-link v-for="route in routes" :to="route">{{ route }}</router-link>
    </n-space>
    <n-divider class="my-2" />
  </div>
  <RouterView class="flex-grow px-2"></RouterView>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { NStatistic, NSpace, NDivider, NBadge } from "naive-ui";
import { RouterView, RouterLink } from "vue-router";
import { routes } from "../../router";
import { useApplicationStore } from "../../store/applicationStore";
import { usePokedexStore } from "../../store/pokedexStore";

export default defineComponent({
  name: "MainLayout",
  components: {
    NStatistic,
    RouterView,
    RouterLink,
    NSpace,
    NDivider,
    NBadge,
  },
  setup() {
    const applicationStore = useApplicationStore();
    const pokedexStore = usePokedexStore();

    return {
      routes,
      applicationStore,
      pokedexStore,
    };
  },
});
</script>
