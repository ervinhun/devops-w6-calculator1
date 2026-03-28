import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '5s', target: 5 },
    { duration: '5s', target: 60 },
    { duration: '10s', target: 60 },
    { duration: '5s', target: 5 },
    { duration: '5s', target: 0 }
  ],
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<800'],
    checks: ['rate>0.98']
  }
};

const baseUrl = __ENV.BASE_URL || 'http://localhost:8080';

export default function () {
  const payload = JSON.stringify({
    operation: 'add',
    a: 12,
    b: 30
  });

  const params = {
    headers: {
      'Content-Type': 'application/json'
    }
  };

  const response = http.post(`${baseUrl}/api/calculations`, payload, params);

  check(response, {
    'returns 200': (r) => r.status === 200,
    'contains result field': (r) => Boolean(r.json('result')),
    'result is correct': (r) => r.json('result') === '42'
  });

  sleep(0.1);
}

