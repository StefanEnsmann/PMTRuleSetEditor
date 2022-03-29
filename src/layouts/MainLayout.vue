<template>
  <q-layout view="hHh LpR lFf">
    <q-header elevated class="bg-primary text-white" height-hint="98">
      <q-toolbar>
        <q-toolbar-title>
          <q-avatar>
            <img src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg" />
          </q-avatar>
          Pokémon Ruleset Editor
        </q-toolbar-title>

        <q-tabs inline-label>
          <q-route-tab to="/home" icon="home" label="Home" />
          <q-route-tab to="/checks" icon="check" label="Checks">
            <q-badge color="grey" floating>0</q-badge>
          </q-route-tab>
          <q-route-tab to="/items" icon="backpack" label="Items">
            <q-badge color="grey" floating>0</q-badge>
          </q-route-tab>
          <q-route-tab to="/pokedex" icon="catching_pokemon" label="Pokédex">
            <q-badge color="grey" floating>0</q-badge>
          </q-route-tab>
          <q-route-tab to="/maps" icon="map" label="Maps">
            <q-badge color="grey" floating>0</q-badge>
          </q-route-tab>
        </q-tabs>
      </q-toolbar>
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'MainLayout',
});
</script>
