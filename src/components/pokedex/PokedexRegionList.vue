<template>
  <q-expansion-item>
    <template v-slot:header>
      <q-item-section avatar>
        <q-avatar icon="bluetooth" color="primary" text-color="white" />
      </q-item-section>

      <q-item-section>
        {{ $t(`regions.${region}`) }}
      </q-item-section>

      <q-item-section side>
        <!--<q-checkbox label="Select all?" />-->
        Test
      </q-item-section>
    </template>
    <pokedex-card-grid :region="region" />
  </q-expansion-item>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import PokedexCardGrid from './PokedexCardGrid.vue';

export default defineComponent({
  name: 'PokedexRegionList',
  components: { PokedexCardGrid },
  props: {
    region: String,
  },
});
</script>
