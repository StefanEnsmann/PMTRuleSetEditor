<template>
  <q-layout view="hHh LpR lFf">
    <q-header elevated class="bg-primary text-white" height-hint="98">
      <q-toolbar>
        <q-toolbar-title>
          <q-avatar>
            <img src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg" />
          </q-avatar>
          {{ $t('title') }}
        </q-toolbar-title>

        <q-tabs inline-label>
          <q-route-tab to="/" icon="home" :label="$t('navigation.home')" />
          <q-route-tab
            to="/checks"
            icon="check"
            :label="$t('navigation.checks')"
          >
            <q-badge color="grey" floating>0</q-badge>
          </q-route-tab>
          <q-route-tab
            to="/items"
            icon="backpack"
            :label="$t('navigation.items')"
          >
            <q-badge color="grey" floating>0</q-badge>
          </q-route-tab>
          <q-route-tab
            to="/pokedex"
            icon="catching_pokemon"
            :label="$t('navigation.pokedex')"
          >
            <q-badge color="grey" floating>0</q-badge>
          </q-route-tab>
          <q-route-tab to="/maps" icon="map" :label="$t('navigation.maps')">
            <q-badge color="grey" floating>0</q-badge>
          </q-route-tab>
        </q-tabs>

        <q-select
          v-model="$root.$i18n.locale"
          :options="supportedLanguages"
          label="Language"
          dense
          borderless
          emit-value
          map-options
          options-dense
          style="min-width: 150px"
        />
      </q-toolbar>
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { useQuasar } from 'quasar';
import { supportedLanguages } from '../i18n';

export default defineComponent({
  name: 'MainLayout',
  setup() {
    const $q = useQuasar();
    console.log($q.lang.getLocale());

    return {
      supportedLanguages,
    };
  },
});
</script>
