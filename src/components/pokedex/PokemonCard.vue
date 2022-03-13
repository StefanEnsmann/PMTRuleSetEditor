<template>
  <n-card :title="name" :segmented="{ footer: 'soft' }">
    <template #header-extra> #{{ id }} </template>
    <img :src="imgSrc" height="64" />
    <template #footer>
      <n-tag round :color="typeColor(typeA)">{{ typeA }}</n-tag>
      <n-tag round :color="typeColor(typeB)">{{ typeB }}</n-tag>
    </template>
  </n-card>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { NCard, NTag } from "naive-ui";
import { TagColor } from "naive-ui/lib/tag/src/common-props";

export default defineComponent({
  name: "PokemonCard",
  components: {
    NCard,
    NTag,
  },
  props: {
    id: Number,
    name: String,
    imgSrc: String,
    typeA: String,
    typeB: String,
    active: Boolean,
  },
  methods: {
    typeColor(type: string | undefined): TagColor {
      switch (type) {
        case "normal":
          return {
            color: "#9099A1",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "fighting":
          return {
            color: "#CE4069",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "flying":
          return {
            color: "#8FA8DD",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "poison":
          return {
            color: "#AB6AC8",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "ground":
          return {
            color: "#D97746",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "rock":
          return {
            color: "#c7b78b",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "bug":
          return {
            color: "#90C12C",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "ghost":
          return {
            color: "#5269AC",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "steel":
          return {
            color: "#5A8EA1",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "fire":
          return {
            color: "#FF9C54",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "water":
          return {
            color: "#4D90D5",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "grass":
          return {
            color: "#63BB5B",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "electric":
          return {
            color: "#F3D23B",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "psychic":
          return {
            color: "#F97176",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "ice":
          return {
            color: "#74CEC0",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "dragon":
          return {
            color: "#0A6DC4",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "dark":
          return {
            color: "#5A5366",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        case "fairy":
          return {
            color: "#EC8FE6",
            textColor: "#FFF",
            borderColor: "#6D6D4E",
          };
        default:
          return { color: "#000", textColor: "#F00", borderColor: "#F00" };
      }
    },
  },
});
</script>

<style scoped></style>
