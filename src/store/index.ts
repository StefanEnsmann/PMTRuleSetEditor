import { createStore, createLogger } from 'vuex'

import metadata from './modules/metadata'
import locations from './modules/locations'
import story_items from './modules/story_items'
import pokemon from './modules/pokemon'
import maps from './modules/maps'

const debug = process.env.NODE_ENV !== 'production'

export default createStore({
    modules: {
        metadata,
        locations,
        story_items,
        pokemon,
        maps
    },
    strict: debug,
    plugins: debug ? [createLogger()] : []
})