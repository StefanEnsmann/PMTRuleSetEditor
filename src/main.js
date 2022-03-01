import { createApp } from 'vue'
import App from './App.vue'
import { createRouter, createWebHashHistory } from 'vue-router'

import HomeView from './components/views/HomeView'
import InformationView from './components/views/InformationView'
import LocationsView from './components/views/LocationsView'
import StoryItemsView from './components/views/StoryItemsView'
import PokedexView from './components/views/PokedexView'
import MapsView from './components/views/MapsView'
import AboutView from './components/views/AboutView'

import 'bootstrap/dist/css/bootstrap.min.css'

const routes = [
    { path: '/', component: HomeView },
    { path: '/information', component: InformationView },
    { path: '/locations', component: LocationsView },
    { path: '/story-items', component: StoryItemsView },
    { path: '/pokedex', component: PokedexView },
    { path: '/maps', component: MapsView },
    { path: '/about', component: AboutView }
]

const router = createRouter({
    history: createWebHashHistory(),
    routes
})

const app = createApp(App)
app.use(router)
app.mount('#app')
