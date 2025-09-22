#!/usr/bin/env bash
set -euo pipefail

BASE_URL=${BASE_URL:-http://localhost:5173}

echo "Testing Excel export..."
HTTP_STATUS=$(curl -s -w "%{http_code}" -D - "$BASE_URL/api/reports/payments/excel" -o /tmp/test-payments.xlsx)
if [ "$HTTP_STATUS" -ne 200 ]; then
  echo "Excel export failed with HTTP $HTTP_STATUS"
  exit 2
fi
FILE_SIZE=$(stat -f%z /tmp/test-payments.xlsx 2>/dev/null || stat -c%s /tmp/test-payments.xlsx)
if [ "$FILE_SIZE" -lt 100 ]; then
  echo "Excel file size too small: $FILE_SIZE bytes"
  exit 3
fi

echo "Excel OK: $FILE_SIZE bytes"

echo "Testing PDF export..."
HTTP_STATUS=$(curl -s -w "%{http_code}" -D - "$BASE_URL/api/reports/payments/pdf" -o /tmp/test-payments.pdf)
if [ "$HTTP_STATUS" -ne 200 ]; then
  echo "PDF export failed with HTTP $HTTP_STATUS"
  exit 4
fi
FILE_SIZE=$(stat -f%z /tmp/test-payments.pdf 2>/dev/null || stat -c%s /tmp/test-payments.pdf)
if [ "$FILE_SIZE" -lt 500 ]; then
  echo "PDF file size too small: $FILE_SIZE bytes"
  exit 5
fi

echo "PDF OK: $FILE_SIZE bytes"

echo "All export tests passed."
exit 0
