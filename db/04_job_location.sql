-- ============================================================================
--  Table-driven Job Location master. LOC_CODE stores the value written to
--  MTJ_JOB_LOCATION / TS_LOCATION (kept identical to the existing string
--  values 'Field'/'Workshop'/'Service' so current data stays compatible).
--  Idempotent.
-- ============================================================================
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_JOB_LOCATION';
  IF n = 0 THEN
    EXECUTE IMMEDIATE q'[
      CREATE TABLE TM_JOB_LOCATION
      (
        LOC_CODE          VARCHAR2(20)  NOT NULL,
        LOC_NAME          VARCHAR2(50)  NOT NULL,
        SORT_ORDER        NUMBER(5),
        ACTIVE_YN         VARCHAR2(1)   DEFAULT 'Y',
        CREATION_USER_ID  NUMBER(10),
        CREATION_DATE     DATE          DEFAULT SYSDATE,
        UPDATE_USER_ID    NUMBER(10),
        UPDATE_DATE       DATE,
        CONSTRAINT TM_JOB_LOCATION_PK PRIMARY KEY (LOC_CODE)
      )]';
  END IF;
END;
/

INSERT INTO TM_JOB_LOCATION (LOC_CODE, LOC_NAME, SORT_ORDER)
  SELECT 'Field', 'Field', 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_JOB_LOCATION WHERE LOC_CODE='Field');
INSERT INTO TM_JOB_LOCATION (LOC_CODE, LOC_NAME, SORT_ORDER)
  SELECT 'Workshop', 'Workshop', 2 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_JOB_LOCATION WHERE LOC_CODE='Workshop');
INSERT INTO TM_JOB_LOCATION (LOC_CODE, LOC_NAME, SORT_ORDER)
  SELECT 'Service', 'Service', 3 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_JOB_LOCATION WHERE LOC_CODE='Service');

COMMIT;
