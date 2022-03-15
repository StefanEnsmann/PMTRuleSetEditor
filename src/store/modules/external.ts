import { Module } from "vuex"
import { ActionTree, ActionContext, GetterTree, MutationTree } from "vuex"
import { IPokedexData } from "../../models/pokedex"
import { IExternalsState, IRootState } from "../interfaces"

const externalsState: IExternalsState = {
    baseUrl: "",
    pokedexData: undefined
}

const externalsGetters: GetterTree<IExternalsState, IRootState> = {

}

const externalsMutations: MutationTree<IExternalsState> = {
    setPokedexData(state: IExternalsState, pokedexData: IPokedexData): void {
        state.pokedexData = pokedexData
    }
}

const externalsActions: ActionTree<IExternalsState, IRootState> = {
    async loadPokedexData({ commit, state }: ActionContext<IExternalsState, IRootState>): Promise<any> {
        return fetch(`${state.baseUrl}/pkmn_data/pokedex.json`)
            .then(response => response.json())
            .then(data => commit("setPokedexData", data))
    }
}

const externalsStore: Module<IExternalsState, IRootState> = {
    namespaced: true,
    state: externalsState,
    getters: externalsGetters,
    mutations: externalsMutations,
    actions: externalsActions
}

export default externalsStore