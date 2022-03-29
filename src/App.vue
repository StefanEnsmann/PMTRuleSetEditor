<template>
  <div class="flex-grow-0 p-2 bg-white">
    <n-menu
      v-model:value="activeKey"
      mode="horizontal"
      :options="menuOptions" 
    />
    <n-page-header>
      <n-grid :cols="4">
        <n-gi><n-statistic label="Checks" value="123">
          <template #prefix>
            <n-icon>
              <PlaylistAddCheckRound />
            </n-icon>
          </template>
          </n-statistic>
        </n-gi>
        <n-gi><n-statistic label="Story Items" value="123" /></n-gi>
        <n-gi><n-statistic label="Pokedex" value="123" /></n-gi>
        <n-gi><n-statistic label="Maps" value="123" /></n-gi>
      </n-grid>
    </n-page-header>
  </div>
  <RouterView class="flex-grow"></RouterView>
</template>

<script lang="ts">
import { h, ref, Component, defineComponent } from "vue"
import { NIcon, NLayout, NMenu, NLayoutSider, NPageHeader, NGrid, NStatistic, NGi } from "naive-ui"
import { RouterLink, RouterView } from "vue-router"
import { useStore } from "./store"
import {
  HomeRound,
  InfoRound,
  PlaylistAddCheckRound,
  BackpackRound,
  CatchingPokemonRound,
  MapRound,
  HelpOutlineRound
} from "@vicons/material"

function renderIcon(icon: Component) {
  return() => h(NIcon, null, { default: () => h(icon) })
}

const menuOptions = [
  {
    label: () => h(RouterLink, {
      to: {
        path: "/"
      }
    }, { default: () => "Home" }),
    key: "go-home",
    icon: renderIcon(HomeRound)
  },
  {
    key: "home-divider",
    type: "divider"
  },
  {
    label: () => h(RouterLink, {
      to: {
        path: "/information"
      }
    }, { default: () => "Information" }),
    key: "go-to-information",
    icon: renderIcon(InfoRound)
  },
  {
    label: () => h(RouterLink, {
      to: {
        path: "/locations"
      }
    }, { default: () => "Locations" }),
    key: "go-to-locations",
    icon: renderIcon(PlaylistAddCheckRound)
  },
  {
    label: () => h(RouterLink, {
      to: {
        path: "/story-items"
      }
    }, { default: () => "Story Items" }),
    key: "go-to-story-items",
    icon: renderIcon(BackpackRound)
  },
  {
    label: () => h(RouterLink, {
      to: {
        path: "/pokedex"
      }
    }, { default: () => "Pokédex" }),
    key: "go-to-pokedex",
    icon: renderIcon(CatchingPokemonRound)
  },
  {
    label: () => h(RouterLink, {
      to: {
        path: "/maps"
      }
    }, { default: () => "Maps" }),
    key: "go-to-maps",
    icon: renderIcon(MapRound)
  },
  {
    key: "about-divider",
    type: "divider"
  },
  {
    label: () => h(RouterLink, {
      to: {
        path: "/about"
      }
    }, { default: () => "About" }),
    key: "go-to-about",
    icon: renderIcon(HelpOutlineRound)
  }
]

export default defineComponent({
  name: 'App',
  components: {
    PlaylistAddCheckRound,
    NIcon,
    NLayout,
    NMenu,
    NLayoutSider,
    NPageHeader,
    NGrid,
    NGi,
    NStatistic,
    RouterView
  },
  setup() {
    const store = useStore()

    return {
      activeKey: ref("go-home"),
      collapsed: ref(true),
      menuOptions
    }
  },
  created() {
    useStore().dispatch("externals/loadPokedexData")
  }
})
</script>
