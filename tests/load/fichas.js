// RNF-03 — escenario "fichas": GET /api/obtener-fichas con 50 VUs sostenidos.
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

  const token = mintInternalToken({
    key: INTERNAL_AUTH_KEY,
    issuer: INTERNAL_AUTH_ISSUER,
    audience: INTERNAL_AUTH_AUDIENCE,
    idUsuario: TEST_ID_USUARIO,
    rol: TEST_ROL,
    idInstructor: TEST_ID_INSTRUCTOR,
  });

  const res = http.get(`${API_BASE_URL}/api/obtener-fichas`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  check(res, { 'status es 200': (r) => r.status === 200 });

  sleep(1);
}
