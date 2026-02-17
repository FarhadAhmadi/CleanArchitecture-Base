import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  vus: 30,
  duration: "5m",
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<500", "p(99)<900"],
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
