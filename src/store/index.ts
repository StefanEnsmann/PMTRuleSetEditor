import { createStore, createLogger, Store, useStore as baseUseStore } from 'vuex'

import { IExternalsState, IRootState } from './interfaces'

import externals from './modules/external'
import metadata from './modules/metadata'
import locations from './modules/locations'
import story_items from './modules/story_items'
import pokemon from './modules/pokemon'
import maps from './modules/maps'
import { InjectionKey } from 'vue'

const debug = process.env.NODE_ENV !== 'production'

export const injectionKey: InjectionKey<Store<IRootState>> = Symbol()

export const store = createStore<IRootState>({
    modules: {
        externals,
    },
    strict: debug,
    plugins: debug ? [createLogger()] : []
})

export function useStore() {
    return baseUseStore(injectionKey)
}