import { Action, Mutation } from "vuex"
import { IPokedexData } from "../models/pokedex"

export interface IRootState {
    externals: IExternalsState
}

export interface IExternalsState {
    baseUrl: string,
    pokedexData?: IPokedexData
}
export interface IExternalsGetters {

}
export interface IExternalsMutations {
    setPokedexData: Mutation<IExternalsState>
}
export interface IExternalsActions {
    loadPokedexData: Action<IExternalsState, IRootState>
}