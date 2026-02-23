# Performance Tests

## Load test
```bash
k6 run -e BASE_URL=http://localhost:5000 tests/Performance/k6/load-test.js
```

## Soak test
```bash
k6 run -e BASE_URL=http://localhost:5000 tests/Performance/k6/soak-test.js
```

## 3x scale validation
```bash
k6 run \
  -e BASE_URL=http://localhost:5000 \
  -e BASELINE_VUS=30 \
  -e BASELINE_DURATION=5m \
  -e TARGET_DURATION=5m \
  tests/Performance/k6/scale-3x.js
```

Both scripts enforce latency and error-rate thresholds to validate performance budgets.
