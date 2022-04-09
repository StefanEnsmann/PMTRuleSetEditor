import { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
      {
        path: '',
        name: 'home',
        component: () => import('pages/HomePage.vue'),
      },
      {
        path: 'checks',
        name: 'checks',
        component: () => import('pages/ChecksPage.vue'),
      },
      {
        path: 'items',
        name: 'items',
        component: () => import('pages/ItemsPage.vue'),
      },
      {
        path: 'pokedex',
        name: 'pokedex',
        component: () => import('pages/PokedexPage.vue'),
      },
      {
        path: 'maps',
        name: 'maps',
        component: () => import('pages/MapsPage.vue'),
      },
    ],
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
