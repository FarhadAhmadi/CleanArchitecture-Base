# Performance Tests

## Load test
```bash
k6 run -e BASE_URL=http://localhost:5000 tests/Performance/k6/load-test.js
```

## Soak test
```bash
k6 run -e BASE_URL=http://localhost:5000 tests/Performance/k6/soak-test.js
```

Both scripts enforce latency and error-rate thresholds to validate performance budgets.
