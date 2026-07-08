// RNF-03 — escenario "asistencia": GET /api/obtener-resultados-programados-ficha
// con 50 VUs sostenidos. Requiere una ficha real (TEST_NUMERO_FICHA) con
// resultados programados en el ambiente que se está probando.
import http from 'k6/http';
import { check, sleep } from 'k6';
import {
  API_BASE_URL,
  INTERNAL_AUTH_KEY,
  INTERNAL_AUTH_ISSUER,
  INTERNAL_AUTH_AUDIENCE,
  TEST_ID_USUARIO,
  TEST_ROL,
  TEST_ID_INSTRUCTOR,
  TEST_NUMERO_FICHA,
  requireInternalAuthKey,
} from './helpers/config.js';
import { mintInternalToken } from './helpers/jwt.js';

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '2m', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  requireInternalAuthKey();

  if (!TEST_NUMERO_FICHA) {
    throw new Error('Falta TEST_NUMERO_FICHA. Ver README.md de tests/load.');
  }

  const token = mintInternalToken({
    key: INTERNAL_AUTH_KEY,
    issuer: INTERNAL_AUTH_ISSUER,
    audience: INTERNAL_AUTH_AUDIENCE,
    idUsuario: TEST_ID_USUARIO,
    rol: TEST_ROL,
    idInstructor: TEST_ID_INSTRUCTOR,
  });

  const url = `${API_BASE_URL}/api/obtener-resultados-programados-ficha?numeroFicha=${encodeURIComponent(TEST_NUMERO_FICHA)}`;
  const res = http.get(url, { headers: { Authorization: `Bearer ${token}` } });

  check(res, { 'status es 200': (r) => r.status === 200 });

  sleep(1);
}
