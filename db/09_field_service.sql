-- ============================================================================
--  Phase 3: Field Service Report (FSR) — field jobs get their own entry screen
--  shaped like the paper Field Service Report, while their hours stay in
--  TM_TIME_LINE so every existing report keeps working.
--
--    * TM_TIME_SHEET.TS_SHEET_TYPE   – 'STANDARD' | 'FIELD' (backfilled from TS_LOCATION)
--    * TM_TIME_LINE.TL_SEGMENT_TYPE  – 'TRAVEL' | 'WORK' (existing lines → 'WORK')
--    * TM_TIME_LINE.TL_IDLE_HOURS / TL_IDLE_REASON – waiting time inside a segment
--    * TM_TIME_SHEET.TS_TOTAL_IDLE_HOURS / TS_VEHICLE_COST
--    * TM_LABOUR_RATE.LR_KM_RATE     – vehicle cost per km (default for the FSR)
--    * TM_FIELD_REPORT               – one FSR per field sheet (1:1 with TS_ID):
--                                      contact, machine, vehicle/km, narrative,
--                                      completion flags, sign-off
--    * TM_FIELD_REPORT_CREW          – technicians on the trip (+ FA code)
--
--  Additive only: no existing column changes, no rows deleted.
--  Idempotent: re-running skips objects/columns that already exist.
-- ============================================================================
DECLARE
  PROCEDURE add_col(p_table VARCHAR2, p_col VARCHAR2, p_ddl VARCHAR2) IS
    n NUMBER;
  BEGIN
    SELECT COUNT(*) INTO n FROM user_tab_columns
     WHERE table_name = p_table AND column_name = p_col;
    IF n = 0 THEN
      EXECUTE IMMEDIATE 'ALTER TABLE ' || p_table || ' ADD (' || p_ddl || ')';
    END IF;
  END;
BEGIN
  add_col('TM_TIME_SHEET', 'TS_SHEET_TYPE',       'TS_SHEET_TYPE VARCHAR2(10) DEFAULT ''STANDARD''');
  add_col('TM_TIME_SHEET', 'TS_TOTAL_IDLE_HOURS', 'TS_TOTAL_IDLE_HOURS NUMBER(10,2) DEFAULT 0');
  add_col('TM_TIME_SHEET', 'TS_VEHICLE_COST',     'TS_VEHICLE_COST NUMBER(14,2) DEFAULT 0');

  add_col('TM_TIME_LINE',  'TL_SEGMENT_TYPE',     'TL_SEGMENT_TYPE VARCHAR2(10) DEFAULT ''WORK''');
  add_col('TM_TIME_LINE',  'TL_IDLE_HOURS',       'TL_IDLE_HOURS NUMBER(6,2) DEFAULT 0');
  add_col('TM_TIME_LINE',  'TL_IDLE_REASON',      'TL_IDLE_REASON VARCHAR2(200)');

  add_col('TM_LABOUR_RATE','LR_KM_RATE',          'LR_KM_RATE NUMBER(10,3) DEFAULT 0');
END;
/

DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_FIELD_REPORT';
  IF n = 0 THEN
    EXECUTE IMMEDIATE q'[
      CREATE TABLE TM_FIELD_REPORT
      (
        FR_TS_ID              NUMBER(10)      NOT NULL,   -- TM_TIME_SHEET.TS_ID (one FSR per field sheet)
        FR_REPORT_NO          VARCHAR2(25),               -- printed "No." on the paper form
        FR_REPORT_DATE        DATE,
        FR_JOB_SITE           VARCHAR2(200),              -- where the machine is (customer site)
        -- Customer contact
        FR_CONTACT_PERSON     VARCHAR2(100),
        FR_CONTACT_POSITION   VARCHAR2(100),
        FR_CONTACT_PHONE      VARCHAR2(50),
        -- Machine / engine (make + serial come from the job)
        FR_OEM                VARCHAR2(100),
        FR_MODEL              VARCHAR2(100),
        FR_SMU_HOURS          NUMBER(12,1),
        -- Work order block
        FR_UNDER_WARRANTY_YN  VARCHAR2(1)     DEFAULT 'N',
        FR_TAI_REPORT         VARCHAR2(50),
        FR_JOB_TYPE           VARCHAR2(50),
        FR_FLEET_NO           VARCHAR2(50),
        -- Transport vehicle
        FR_PLATE_NO           VARCHAR2(30),
        FR_DRIVER_NAME        VARCHAR2(100),
        FR_KM_START           NUMBER(12,1),
        FR_KM_END             NUMBER(12,1),
        FR_TOTAL_KM           NUMBER(12,1)    DEFAULT 0,
        FR_KM_RATE            NUMBER(10,3)    DEFAULT 0,
        FR_VEHICLE_COST       NUMBER(14,2)    DEFAULT 0,
        -- Narrative
        FR_COMPLAINT          VARCHAR2(2000),
        FR_PROBLEMS_FOUND     VARCHAR2(2000),
        FR_PROBLEM_CAUSES     VARCHAR2(2000),
        FR_CORRECTION         VARCHAR2(2000),
        FR_OTHER_ITEMS        VARCHAR2(2000),
        FR_JOB_COMPLETED_YN   VARCHAR2(1)     DEFAULT 'N',
        FR_TO_WORKSHOP_YN     VARCHAR2(1)     DEFAULT 'N',
        -- Sign-off
        FR_SERVICE_ENGINEER   VARCHAR2(100),
        FR_SERVICE_MANAGER    VARCHAR2(100),
        FR_SIGN_DATE          DATE,
        FR_CUST_COMMENTS      VARCHAR2(1000),
        FR_CUST_REP           VARCHAR2(100),
        FR_CUST_REP_POSITION  VARCHAR2(100),
        FR_CUST_SIGN_DATE     DATE,
        FR_CREATION_USER_ID   NUMBER(10),
        FR_CREATION_DATE      DATE            DEFAULT SYSDATE,
        FR_UPDATE_USER_ID     NUMBER(10),
        FR_UPDATE_DATE        DATE,
        CONSTRAINT TM_FIELD_REPORT_PK PRIMARY KEY (FR_TS_ID),
        CONSTRAINT TM_FIELD_REPORT_FK FOREIGN KEY (FR_TS_ID) REFERENCES TM_TIME_SHEET (TS_ID)
      )]';
  END IF;

  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_FIELD_REPORT_CREW';
  IF n = 0 THEN
    EXECUTE IMMEDIATE q'[
      CREATE TABLE TM_FIELD_REPORT_CREW
      (
        FRC_TS_ID             NUMBER(10)      NOT NULL,   -- TM_FIELD_REPORT.FR_TS_ID
        FRC_SEQ               NUMBER(3)       NOT NULL,
        FRC_EMP_CODE          VARCHAR2(15)    NOT NULL,   -- PPM_EMPLOYEE_DETAILS.PEMP_EMP_CODE
        FRC_EMP_NAME          VARCHAR2(200),
        FRC_FA_CODE           VARCHAR2(30),
        CONSTRAINT TM_FIELD_REPORT_CREW_PK PRIMARY KEY (FRC_TS_ID, FRC_EMP_CODE),
        CONSTRAINT TM_FIELD_REPORT_CREW_FK FOREIGN KEY (FRC_TS_ID) REFERENCES TM_FIELD_REPORT (FR_TS_ID)
      )]';
  END IF;
END;
/

-- ---------- Backfill ---------------------------------------------------------
-- Sheets already posted for Field jobs open in the new Field Service screen.
UPDATE TM_TIME_SHEET SET TS_SHEET_TYPE = 'FIELD'
 WHERE UPPER(TS_LOCATION) = 'FIELD' AND NVL(TS_SHEET_TYPE,'STANDARD') <> 'FIELD';
UPDATE TM_TIME_SHEET SET TS_SHEET_TYPE = 'STANDARD' WHERE TS_SHEET_TYPE IS NULL;
UPDATE TM_TIME_LINE  SET TL_SEGMENT_TYPE = 'WORK'   WHERE TL_SEGMENT_TYPE IS NULL;
UPDATE TM_TIME_LINE  SET TL_IDLE_HOURS = 0          WHERE TL_IDLE_HOURS IS NULL;

COMMIT;
