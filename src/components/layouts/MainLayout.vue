<template>
  <div class="flex-grow-0 p-2 bg-white">
    <n-space :size="[100, 0]">
      <n-statistic label="Checks" value="123"> </n-statistic>
      <n-statistic label="Story Items" value="123"> </n-statistic>
      <n-statistic label="Pokedex" value="0">
        <template #suffix> / 906 </template>
      </n-statistic>
      <n-statistic label="Maps" value="123"> </n-statistic>
    </n-space>
    <n-divider class="flex-grow-0 my-2" />
    <n-space>
      <n-badge :value="20"><a href="#/pokedex">Pokedex</a></n-badge>
    </n-space>
    <n-divider class="my-2" />
  </div>
  <RouterView class="flex-grow px-2"></RouterView>
</template>

<script lang="ts">
import { h, defineComponent } from "vue";
import { NStatistic, NSpace, NDivider, NBadge } from "naive-ui";
import { RouterView } from "vue-router";

export default defineComponent({
  name: "MainLayout",
  components: {
    NStatistic,
    RouterView,
    NSpace,
    NDivider,
    NBadge,
  },
});
</script>
