import { createApp } from 'vue'
import { store, injectionKey } from './store'
import router from './router'

import App from './App.vue'

createApp(App)
    .use(store, injectionKey)
    .use(router)
    .mount('#app')
