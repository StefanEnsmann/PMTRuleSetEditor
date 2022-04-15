export function pokemonSlug(name: string) {
  return name
    .toLowerCase()
    .replace(/ /g, "-")
    .replace(/:/g, "")
    .replace(/\./g, "")
    .replace(/’/g, "")
    .replace(/♀/g, "-f")
    .replace(/♂/g, "-m")
    .replace(/é/g, "e");
}
