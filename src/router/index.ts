import { createRouter, createWebHashHistory } from 'vue-router'

import HomeView from '../components/views/HomeView.vue'
import InformationView from '../components/views/InformationView.vue'
import LocationsView from '../components/views/LocationsView.vue'
import StoryItemsView from '../components/views/StoryItemsView.vue'
import PokedexView from '../components/views/PokedexView.vue'
import MapsView from '../components/views/MapsView.vue'
import AboutView from '../components/views/AboutView.vue'

export default createRouter({
    history: createWebHashHistory(),
    routes: [
        { path: '/', component: HomeView },
        { path: '/information', component: InformationView },
        { path: '/locations', component: LocationsView },
        { path: '/story-items', component: StoryItemsView },
        { path: '/pokedex', component: PokedexView },
        { path: '/maps', component: MapsView },
        { path: '/about', component: AboutView }
    ]
})