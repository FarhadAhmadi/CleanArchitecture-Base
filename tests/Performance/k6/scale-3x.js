import http from "k6/http";
import { check, sleep } from "k6";

const baselineVus = Number(__ENV.BASELINE_VUS || 30);
const baselineDuration = __ENV.BASELINE_DURATION || "5m";
const targetVus = Math.max(1, baselineVus * 3);
const targetDuration = __ENV.TARGET_DURATION || baselineDuration;
const baseUrl = __ENV.BASE_URL || "http://localhost:5000";

export const options = {
  vus: targetVus,
  duration: targetDuration,
  thresholds: {
    http_req_failed: ["rate<0.001"],
    http_req_duration: ["p(95)<300", "p(99)<700"],
  },
};

export default function () {
  const response = http.get(`${baseUrl}/health`);
  check(response, {
    "status is 200": (r) => r.status === 200,
  });
  sleep(1);
}
