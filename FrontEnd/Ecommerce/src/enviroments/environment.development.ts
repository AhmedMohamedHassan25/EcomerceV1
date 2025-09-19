
export const environment = {
  production: false,
  BaseUrl:'https://localhost:7169/api',
baseImageUrl: 'http://localhost:5089',
  tokenRefreshThreshold: 300000,
  imageUploadMaxSize: 5242880,
  paginationPageSize: 12
};
console.log('🔍 Current Environment:', environment);
console.log('🔍 Base URL:', environment.BaseUrl);
