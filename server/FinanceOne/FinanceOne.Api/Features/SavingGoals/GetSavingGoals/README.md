# List Saving Goals

**Endpoint:** `GET /api/saving-goals`

Returns all saving goals with their progress: amount saved so far, amount remaining, and time remaining until the target date.

**Behavior**
- Returns an empty list if none exist.

**Open question**
- Where does "amount saved so far" come from — manual contributions logged against the goal, or a computed leftover (income minus expenses minus budgets) allocated to it? This decides whether Saving Goals needs its own "contribution" command or is purely derived from other slices.
