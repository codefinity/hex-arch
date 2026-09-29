-- Ops script: repairs any missing, stale or orphaned viewmodels.userviewmodel rows.
-- Run from psql or a schedule; safe to run at any time.
SELECT viewmodels.reconcile_user_viewmodels();
