<template>
    <n-layout has-sider>
      <n-layout-sider
        bordered
        collapse-mode="width"
        :collapsed-width="64"
        :width="240"
        :collapsed="collapsed"
        show-trigger
        @collapse="collapsed = true"
        @expand="collapsed = false"
      >
        <n-menu 
          v-model:value="activeKey"
          :collapsed="collapsed"
          :collapsed-width="64"
          :collapsed-icon-size="22"
          :options="menuOptions" 
        />
      </n-layout-sider>
      <n-layout>
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
        <RouterView></RouterView>
      </n-layout>
    </n-layout>
</template>

<script>
import { h, ref } from "vue"
import { NIcon, NLayout, NMenu, NLayoutSider, NPageHeader, NGrid, NStatistic, NGi } from "naive-ui"
import { RouterLink } from "vue-router"
import {
  HomeRound,
  InfoRound,
  PlaylistAddCheckRound,
  BackpackRound,
  CatchingPokemonRound,
  MapRound,
  HelpOutlineRound
} from "@vicons/material"

function renderIcon(icon) {
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

export default {
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
    NStatistic
  },
  setup() {
    return {
      activeKey: ref("go-home"),
      collapsed: ref(true),
      menuOptions
    }
  }
}
</script>

<style>
html, body, #app {
  width: 100%;
  height: 100%;
  margin: 0;
}

.n-layout {
  height: 100%;
}
</style>
