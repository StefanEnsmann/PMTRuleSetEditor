<template>
  <n-card
    :title="pkmn.localization.en"
    hoverable
    class="select-none w-52"
    :class="{ 'bg-green-100': active }"
    @click="toggleActive"
  >
    <template #cover>
      <n-space justify="space-between" align="baseline" class="pt-1 px-3">
        <img :src="imgSrc" class="h-14 w-14 inline" />
        <n-space :size="[4, 0]">
          <n-tag size="small" :color="typeColor(pkmn.typeA)">
            {{ pkmn.typeA }}
          </n-tag>
          <n-tag
            v-if="pkmn.typeB !== undefined"
            size="small"
            :color="typeColor(pkmn.typeB)"
          >
            {{ pkmn.typeB }}
          </n-tag>
        </n-space>
      </n-space>
    </template>
    <template #header-extra> #{{ pkmn.nr }} </template>
  </n-card>
</template>

<script lang="ts">
import { defineComponent, PropType } from "vue";
import { NAvatar, NCard, NTag, NSwitch, NSpace } from "naive-ui";
import { TagColor } from "naive-ui/lib/tag/src/common-props";
import { PokemonData } from "../../models/pokedex";

export default defineComponent({
  name: "PokemonCard",
  components: {
    NAvatar,
    NCard,
    NTag,
    NSwitch,
    NSpace,
  },
  props: {
    pkmn: {
      type: Object as PropType<PokemonData>,
      default: {},
    },
  },
  data() {
    return {
      active: false,
    };
  },
  computed: {
    imgSrc(): string {
      return (
        "https://pkmntracker.ensmann.de/img/pkmn/" +
        `${String(this.pkmn.nr).padStart(4, "0")}_${this.pokemonSlug(
          this.pkmn.localization.en
        )}.png`
      );
    },
  },
  methods: {
    toggleActive(): void {
      this.active = !this.active;
    },
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
            textColor: "#000",
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
            textColor: "#000",
            borderColor: "#6D6D4E",
          };
        case "psychic":
          return {
            color: "#F97176",
            textColor: "#000",
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
            textColor: "#000",
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
