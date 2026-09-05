-- ============================================================================
--  Phase 2 change: capture the Job Location per task line so each technician's
--  booked line records WHERE the work was done (TM_JOB_LOCATION value), and that
--  location can drive the line's labour rate / cost. Mirrors the header
--  TS_LOCATION but at TM_TIME_LINE grain.
--  Idempotent: adds the column only if it does not already exist.
-- ============================================================================
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_tab_columns
   WHERE table_name = 'TM_TIME_LINE' AND column_name = 'TL_LOCATION';
  IF n = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE TM_TIME_LINE ADD (TL_LOCATION VARCHAR2(20))';
  END IF;
END;
/
COMMIT;
