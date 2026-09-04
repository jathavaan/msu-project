# List Monthly Savings

**Endpoint:** `GET /api/monthly-savings`

Returns all recurring monthly savings, each with the name of the saving goal it contributes to.

**Behavior**
- Returns an empty list if none exist.
- Feeds the Dashboard's "available after savings" figure and the Monthly Savings snapshot widget.
