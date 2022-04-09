import { boot } from 'quasar/wrappers';

export default boot(({ app }) => {
  app.config.globalProperties.$pokemonNameEncoding = (name: string) =>
    name
      .toLowerCase()
      .replace(' ', '-')
      .replace(':', '')
      .replace('.', '')
      .replace('’', '')
      .replace('♀', '-f')
      .replace('♂', '-m')
      .replace('é', 'e');
});
