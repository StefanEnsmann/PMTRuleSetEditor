export interface ILocalizationData {
    "de": string,
    "en": string,
    "fr": string,
    "jp": string,
    "ko": string,
    "zh-Hans": string,
    "zh-Hant": string
}

export interface IPokemonData {
    localization: ILocalizationData,
    nr: number,
    typeA: string,
    typeB: string
}

export interface IPokedexData {
    overrides: Object,
    templates: Object,
    list: Array<IPokemonData>
}