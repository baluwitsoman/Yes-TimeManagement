-- ============================================================================
--  Phase 5: Non-job time booked on the Time Booking screen.
--    Holidays, leave, week-off, idle, training … are "non-job activities" that
--    appear in the Job dropdown and are booked like a job (same screen, same
--    lines). They are TM-only codes (prefix NJ-), not ERP job cards.
--
--    * TM_NONJOB_ACTIVITY  – the activities, with a CATEGORY that tells the
--                            reports what the hours mean:
--          HOLIDAY | LEAVE | OFF   → absence: reduce available hours
--          IDLE                    → present, waiting / no work
--          NONPROD                 → present, non-productive (training, housekeeping…)
--    * TM_TIME_SHEET_UK_JOB_ACTIVE is rebuilt so "one active sheet per job"
--      applies to real jobs only (TS_SHEET_TYPE = 'NONJOB' sheets are exempt —
--      leave for different people / periods are separate sheets).
--
--  Additive except for the index rebuild (same rule, narrower). Idempotent.
-- ============================================================================
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_NONJOB_ACTIVITY';
  IF n = 0 THEN
    EXECUTE IMMEDIATE q'[
      CREATE TABLE TM_NONJOB_ACTIVITY
      (
        NJ_CODE            VARCHAR2(15)   NOT NULL,   -- stored in TS_JOB_CODE / TL_JOB_CODE (e.g. NJ-AL)
        NJ_NAME            VARCHAR2(100)  NOT NULL,
        NJ_CATEGORY        VARCHAR2(10)   NOT NULL,   -- HOLIDAY | LEAVE | OFF | IDLE | NONPROD
        NJ_PAID_YN         VARCHAR2(1)    DEFAULT 'Y',-- N = no labour cost (e.g. unpaid leave)
        NJ_SORT            NUMBER(5),
        ACTIVE_YN          VARCHAR2(1)    DEFAULT 'Y',
        CREATION_USER_ID   NUMBER(10),
        CREATION_DATE      DATE           DEFAULT SYSDATE,
        UPDATE_USER_ID     NUMBER(10),
        UPDATE_DATE        DATE,
        CONSTRAINT TM_NONJOB_ACTIVITY_PK PRIMARY KEY (NJ_CODE),
        CONSTRAINT TM_NONJOB_ACTIVITY_CK CHECK (NJ_CATEGORY IN ('HOLIDAY','LEAVE','OFF','IDLE','NONPROD'))
      )]';
  END IF;

  -- One active sheet per REAL job; non-job sheets are exempt.
  SELECT COUNT(*) INTO n FROM user_indexes WHERE index_name = 'TM_TIME_SHEET_UK_JOB_ACTIVE';
  IF n > 0 THEN
    EXECUTE IMMEDIATE 'DROP INDEX TM_TIME_SHEET_UK_JOB_ACTIVE';
  END IF;
  EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX TM_TIME_SHEET_UK_JOB_ACTIVE ON TM_TIME_SHEET '
                 || '(CASE WHEN NVL(TS_ACTIVE_YN,''Y'') = ''Y'' AND NVL(TS_SHEET_TYPE,''STANDARD'') <> ''NONJOB'' '
                 || 'THEN TS_JOB_CODE END)';
END;
/

INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-HOL', 'Public Holiday', 'HOLIDAY', 'Y', 10 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-HOL');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-AL', 'Annual Leave', 'LEAVE', 'Y', 20 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-AL');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-SL', 'Sick Leave', 'LEAVE', 'Y', 30 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-SL');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-EL', 'Emergency Leave', 'LEAVE', 'Y', 40 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-EL');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-UL', 'Unpaid Leave', 'LEAVE', 'N', 50 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-UL');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-OFF', 'Week Off / Rest Day', 'OFF', 'Y', 60 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-OFF');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-CO', 'Comp Off', 'OFF', 'Y', 70 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-CO');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-IDLE', 'Idle / No Work', 'IDLE', 'Y', 80 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-IDLE');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-TRN', 'Training', 'NONPROD', 'Y', 90 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-TRN');
INSERT INTO TM_NONJOB_ACTIVITY (NJ_CODE, NJ_NAME, NJ_CATEGORY, NJ_PAID_YN, NJ_SORT)
  SELECT 'NJ-WSH', 'Workshop Housekeeping', 'NONPROD', 'Y', 100 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_NONJOB_ACTIVITY WHERE NJ_CODE = 'NJ-WSH');

COMMIT;
