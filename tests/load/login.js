// RNF-03 — escenario "login": POST /api/login con 50 VUs sostenidos.
// Es el único de los 4 escenarios que no necesita token interno (AllowAnonymous).
import http from 'k6/http';
import { check, sleep } from 'k6';
import { API_BASE_URL, TEST_CORREO, TEST_PASSWORD } from './helpers/config.js';

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
  if (!TEST_CORREO || !TEST_PASSWORD) {
    throw new Error('Faltan TEST_CORREO / TEST_PASSWORD. Ver README.md de tests/load.');
  }

  const res = http.post(
    `${API_BASE_URL}/api/login`,
    JSON.stringify({ correo: TEST_CORREO, password: TEST_PASSWORD }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  check(res, {
    'status es 200': (r) => r.status === 200,
    'responde esExitoso=true': (r) => {
      try {
        return JSON.parse(r.body).esExitoso === true;
      } catch {
        return false;
      }
    },
  });

  sleep(1);
}
