import { boot } from 'quasar/wrappers';

export default boot(({ app }) => {
  app.config.globalProperties.$pokemonNameEncoding = (name: string) =>
    name
      .toLowerCase()
      .replace(/ /g, '-')
      .replace(/:/g, '')
      .replace(/\./g, '')
      .replace(/’/g, '')
      .replace(/♀/g, '-f')
      .replace(/♂/g, '-m')
      .replace(/é/g, 'e');
});
