const api = process.env['services__api__http__0'];
if (!api) throw new Error('services__api__http__0 is not set. Start the app with Aspire.');

export default {
  '/api': { target: api },
};
