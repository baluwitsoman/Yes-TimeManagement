-- ============================================================================
--  Phase 4: HRMS attendance sync (holidays, weekends/OFF, leave, absence,
--  punched hours) + technician code mapping + standard hours per day.
--
--    * TM_DUTY_TIMING.DT_STD_HOURS_PER_DAY – the "one working day" used for
--                                            available hours (20 days x 8 h)
--    * TM_HRMS_EMPLOYEE   – HRMS employee list (company 39), refreshed by sync
--    * TM_EMP_CODE_MAP    – TM technician code -> HRMS employee code
--                           (auto-suggested by the sync, confirmed by admin)
--    * TM_ATTENDANCE      – one row per HRMS employee per day from V_ATT_TRAN
--    * TM_SYNC_LOG        – every sync run (manual or nightly)
--
--  Additive only. Idempotent.
-- ============================================================================
DECLARE
  n NUMBER;
  PROCEDURE ddl(p_name VARCHAR2, p_type VARCHAR2, p_sql VARCHAR2) IS
    c NUMBER;
  BEGIN
    SELECT COUNT(*) INTO c FROM user_objects WHERE object_name = p_name AND object_type = p_type;
    IF c = 0 THEN EXECUTE IMMEDIATE p_sql; END IF;
  END;
BEGIN
  SELECT COUNT(*) INTO n FROM user_tab_columns
   WHERE table_name = 'TM_DUTY_TIMING' AND column_name = 'DT_STD_HOURS_PER_DAY';
  IF n = 0 THEN
    EXECUTE IMMEDIATE 'ALTER TABLE TM_DUTY_TIMING ADD (DT_STD_HOURS_PER_DAY NUMBER(4,2) DEFAULT 8)';
  END IF;

  ddl('TM_HRMS_EMPLOYEE', 'TABLE', q'[
    CREATE TABLE TM_HRMS_EMPLOYEE
    (
      HE_EMP_CODE        VARCHAR2(20)   NOT NULL,   -- HRMS PPM_EMPLOYEE_DETAILS.PEMP_EMP_CODE
      HE_EMPLOYMENT_NO   VARCHAR2(30),
      HE_EMP_NAME        VARCHAR2(200),
      HE_ACTIVE          VARCHAR2(5),
      HE_BRANCH_CODE     VARCHAR2(15),
      HE_DEPT_NAME       VARCHAR2(100),
      HE_DESIG_NAME      VARCHAR2(100),
      HE_SYNC_DATE       DATE           DEFAULT SYSDATE,
      CONSTRAINT TM_HRMS_EMPLOYEE_PK PRIMARY KEY (HE_EMP_CODE)
    )]');

  ddl('TM_EMP_CODE_MAP', 'TABLE', q'[
    CREATE TABLE TM_EMP_CODE_MAP
    (
      ECM_TM_EMP_CODE    VARCHAR2(20)   NOT NULL,   -- TM technician (PPM_EMPLOYEE_DETAILS / TL_TECH_CODE)
      ECM_HRMS_EMP_CODE  VARCHAR2(20),              -- TM_HRMS_EMPLOYEE.HE_EMP_CODE (NULL = not in HRMS)
      ECM_MATCH_TYPE     VARCHAR2(15),              -- CODE | EMPLOYMENT_NO | NAME | FUZZY_NAME | MANUAL | NONE
      ECM_CONFIDENCE     NUMBER(5,1),               -- 0-100
      ECM_STATUS         VARCHAR2(10)   DEFAULT 'SUGGESTED',   -- SUGGESTED | CONFIRMED | REJECTED
      ECM_UPDATE_USER_ID NUMBER(10),
      ECM_UPDATE_DATE    DATE           DEFAULT SYSDATE,
      CONSTRAINT TM_EMP_CODE_MAP_PK PRIMARY KEY (ECM_TM_EMP_CODE)
    )]');

  ddl('TM_ATTENDANCE', 'TABLE', q'[
    CREATE TABLE TM_ATTENDANCE
    (
      ATT_HRMS_EMP_CODE  VARCHAR2(20)   NOT NULL,
      ATT_DATE           DATE           NOT NULL,
      ATT_DAY_STATUS     VARCHAR2(15),              -- PRESENT | LATE | PERM | SPECIAL | MANUAL | LEAVE | HOLIDAY | OFF | ABSENT | OTHER
      ATT_LEAVE_ID       VARCHAR2(15),
      ATT_LEAVE_NAME     VARCHAR2(100),
      ATT_HOLIDAY_NAME   VARCHAR2(100),
      ATT_LEAVE_DAYS     NUMBER(3,2)    DEFAULT 0,  -- 1 / 0.5
      ATT_HOLIDAY_DAYS   NUMBER(3,2)    DEFAULT 0,
      ATT_ABSENT_DAYS    NUMBER(3,2)    DEFAULT 0,
      ATT_SHIFT_HOURS    NUMBER(5,2)    DEFAULT 0,
      ATT_ACTUAL_HOURS   NUMBER(5,2)    DEFAULT 0,  -- punched hours
      ATT_OT_HOURS       NUMBER(5,2)    DEFAULT 0,
      ATT_FIRST_IN       DATE,
      ATT_LAST_OUT       DATE,
      ATT_SYNC_ID        NUMBER(10),
      CONSTRAINT TM_ATTENDANCE_PK PRIMARY KEY (ATT_HRMS_EMP_CODE, ATT_DATE)
    )]');

  ddl('TM_SYNC_LOG', 'TABLE', q'[
    CREATE TABLE TM_SYNC_LOG
    (
      SL_ID              NUMBER(10)     NOT NULL,
      SL_TRIGGER         VARCHAR2(10),              -- MANUAL | SCHEDULED
      SL_FROM_DATE       DATE,
      SL_TO_DATE         DATE,
      SL_START           DATE           DEFAULT SYSDATE,
      SL_END             DATE,
      SL_STATUS          VARCHAR2(10),              -- RUNNING | OK | FAILED
      SL_EMPLOYEES       NUMBER(6)      DEFAULT 0,
      SL_ATT_ROWS        NUMBER(8)      DEFAULT 0,
      SL_NEW_MATCHES     NUMBER(6)      DEFAULT 0,
      SL_MESSAGE         VARCHAR2(2000),
      SL_USER_ID         NUMBER(10),
      CONSTRAINT TM_SYNC_LOG_PK PRIMARY KEY (SL_ID)
    )]');

  ddl('TM_SYNC_LOG_SEQ', 'SEQUENCE', 'CREATE SEQUENCE TM_SYNC_LOG_SEQ START WITH 1 INCREMENT BY 1 NOCACHE');
END;
/

UPDATE TM_DUTY_TIMING SET DT_STD_HOURS_PER_DAY = 8 WHERE DT_STD_HOURS_PER_DAY IS NULL;

COMMIT;
