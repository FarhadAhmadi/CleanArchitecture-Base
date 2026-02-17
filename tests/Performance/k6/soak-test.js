import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  stages: [
    { duration: "10m", target: 10 },
    { duration: "2h", target: 10 },
    { duration: "10m", target: 0 },
  ],
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<700", "p(99)<1200"],
  },
};

const baseUrl = __ENV.BASE_URL || "http://localhost:5000";

export default function () {
  const response = http.get(`${baseUrl}/health`);
  check(response, {
    "status is 200": (r) => r.status === 200,
  });
  sleep(1);
}
