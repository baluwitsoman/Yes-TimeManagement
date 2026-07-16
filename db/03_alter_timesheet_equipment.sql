-- ============================================================================
--  Phase 1 change: capture the job's equipment details on the time sheet header
--  so a posted booking records Brand / Equipment Type / Service Type / Serial /
--  Job Opening Date (sourced from MTL_TRANSACTION_JOB_OTHER_DTLS on the job).
--  Idempotent: adds each column only if it does not already exist.
-- ============================================================================
DECLARE
  PROCEDURE add_col(p_col VARCHAR2, p_ddl VARCHAR2) IS
    n NUMBER;
  BEGIN
    SELECT COUNT(*) INTO n FROM user_tab_columns
     WHERE table_name = 'TM_TIME_SHEET' AND column_name = p_col;
    IF n = 0 THEN
      EXECUTE IMMEDIATE 'ALTER TABLE TM_TIME_SHEET ADD (' || p_ddl || ')';
    END IF;
  END;
BEGIN
  add_col('TS_BRAND',            'TS_BRAND VARCHAR2(200)');
  add_col('TS_EQUIPMENT_TYPE',   'TS_EQUIPMENT_TYPE VARCHAR2(200)');
  add_col('TS_SERVICE_TYPE',     'TS_SERVICE_TYPE VARCHAR2(200)');
  add_col('TS_SERIAL_NO',        'TS_SERIAL_NO VARCHAR2(50)');
  add_col('TS_JOB_OPENING_DATE', 'TS_JOB_OPENING_DATE DATE');
END;
/
COMMIT;
