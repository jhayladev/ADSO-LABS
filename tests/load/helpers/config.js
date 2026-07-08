// Configuración compartida por los 4 escenarios, vía variables de entorno k6
// (-e NOMBRE=valor). Ver README.md de esta carpeta para el detalle de cada una.
export const API_BASE_URL = __ENV.API_BASE_URL || 'https://localhost:7221';

export const INTERNAL_AUTH_KEY = __ENV.INTERNAL_AUTH_KEY || '';
export const INTERNAL_AUTH_ISSUER = __ENV.INTERNAL_AUTH_ISSUER || 'AdsoLabs.Web';
export const INTERNAL_AUTH_AUDIENCE = __ENV.INTERNAL_AUTH_AUDIENCE || 'AdsoLabs.API';

export const TEST_ID_USUARIO = __ENV.TEST_ID_USUARIO || '1';
export const TEST_ROL = __ENV.TEST_ROL || 'Administrador';
export const TEST_ID_INSTRUCTOR = __ENV.TEST_ID_INSTRUCTOR || '0';
export const TEST_NUMERO_FICHA = __ENV.TEST_NUMERO_FICHA || '';

export const TEST_CORREO = __ENV.TEST_CORREO || '';
export const TEST_PASSWORD = __ENV.TEST_PASSWORD || '';

export function requireInternalAuthKey() {
  if (!INTERNAL_AUTH_KEY) {
    throw new Error(
      'Falta INTERNAL_AUTH_KEY. Pasala con -e INTERNAL_AUTH_KEY=<mismo valor que InternalAuth:Key ' +
      'en appsettings.Development.json del ambiente que estás probando>.'
    );
  }
}
