import encoding from 'k6/encoding';
import crypto from 'k6/crypto';

// Reproduce exactamente el token que AdsoLabs.Web/Services/InternalTokenHandler.cs
// mintea por cada llamada saliente a AdsoLabs.API. Solo sirve para pruebas de
// carga contra la API — nunca usar la clave real de InternalAuth:Key de un
// ambiente productivo aqui.
export function mintInternalToken({ key, issuer, audience, idUsuario, rol, idInstructor = 0, ttlSeconds = 120 }) {
  const header = { alg: 'HS256', typ: 'JWT' };
  const now = Math.floor(Date.now() / 1000);
  const payload = {
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': String(idUsuario),
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': rol,
    IdInstructor: String(idInstructor),
    iss: issuer,
    aud: audience,
    nbf: now,
    iat: now,
    exp: now + ttlSeconds,
  };

  const encodedHeader = encoding.b64encode(JSON.stringify(header), 'rawurl');
  const encodedPayload = encoding.b64encode(JSON.stringify(payload), 'rawurl');
  const signingInput = `${encodedHeader}.${encodedPayload}`;
  const signature = crypto.hmac('sha256', key, signingInput, 'base64rawurl');

  return `${signingInput}.${signature}`;
}
