-- ============================================================================
--  Phase 6: Job Cost Posting — time sheets → Yes ERP finance (GL JV).
--    Every time-sheet save rebuilds that sheet's JV(s) in FINT_TRANSACTION_*:
--    one GL/JRV voucher per sheet per calendar month (the GL period of the
--    work dates). Absorption entry, so payroll is not booked twice:
--        Dr  job cost account (per cost code)      FITD_TA_CODE1 = job code
--        Cr  absorbed / recovery contra account    (no job code)
--    Voiding the sheet deletes its JVs. Non-job sheets and idle hours are
--    never posted. OT hours use the same flat rate; food allowance is not posted.
--
--    * TM_JCP_SETUP      – single row: on/off, company/branch, JV type, cut-over date
--    * TM_JCP_COST_CODE  – rate + Dr/Cr account per cost code, effective-dated
--          LABOUR     one row per job location (TM_JOB_LOCATION.LOC_CODE) — hours
--          TRAVEL_HR  travel hours
--          TRAVEL_KM  vehicle km on the Field Service Report
--    * TM_JCP_POSTING    – sheet × period → the ERP voucher it owns
--
--  Additive only (new tables). Idempotent. Accounts are left empty and posting
--  is OFF until an admin completes Job Cost Posting Setup.
-- ============================================================================
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_JCP_SETUP';
  IF n = 0 THEN
    EXECUTE IMMEDIATE q'[
      CREATE TABLE TM_JCP_SETUP
      (
        JCS_ID             NUMBER(2)      NOT NULL,   -- always 1
        JCS_ENABLED_YN     VARCHAR2(1)    DEFAULT 'N' NOT NULL,
        JCS_COMP_CODE      VARCHAR2(10)   DEFAULT '01' NOT NULL,
        JCS_OFF_CODE       VARCHAR2(10)   DEFAULT '01' NOT NULL,   -- FITH_OFF_CODE (branch)
        JCS_JRNL_TYPE      VARCHAR2(6)    DEFAULT 'JRV' NOT NULL,  -- GL journal type
        JCS_START_DATE     DATE,                                   -- only work dates on/after this are posted
        UPDATE_USER_ID     NUMBER(10),
        UPDATE_DATE        DATE,
        CONSTRAINT TM_JCP_SETUP_PK PRIMARY KEY (JCS_ID),
        CONSTRAINT TM_JCP_SETUP_CK CHECK (JCS_ID = 1)
      )]';
  END IF;

  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_JCP_COST_CODE';
  IF n = 0 THEN
    EXECUTE IMMEDIATE q'[
      CREATE TABLE TM_JCP_COST_CODE
      (
        JCC_ID             NUMBER(10)     NOT NULL,
        JCC_TYPE           VARCHAR2(10)   NOT NULL,   -- LABOUR | TRAVEL_HR | TRAVEL_KM
        JCC_LOCATION       VARCHAR2(20),              -- LABOUR only: TM_JOB_LOCATION.LOC_CODE
        JCC_DESC           VARCHAR2(100)  NOT NULL,
        JCC_RATE           NUMBER(12,3)   NOT NULL,   -- OMR per hour / per km
        JCC_DR_VEL_CODE    NUMBER(10),                -- FINM_ACCOUNT_MASTER.FNAM_VEL_CODE (job cost)
        JCC_CR_VEL_CODE    NUMBER(10),                -- FINM_ACCOUNT_MASTER.FNAM_VEL_CODE (absorbed)
        JCC_EFFECTIVE_FROM DATE           DEFAULT DATE '2000-01-01' NOT NULL,
        ACTIVE_YN          VARCHAR2(1)    DEFAULT 'Y',
        CREATION_USER_ID   NUMBER(10),
        CREATION_DATE      DATE           DEFAULT SYSDATE,
        UPDATE_USER_ID     NUMBER(10),
        UPDATE_DATE        DATE,
        CONSTRAINT TM_JCP_COST_CODE_PK PRIMARY KEY (JCC_ID),
        CONSTRAINT TM_JCP_COST_CODE_CK CHECK (JCC_TYPE IN ('LABOUR','TRAVEL_HR','TRAVEL_KM'))
      )]';
    EXECUTE IMMEDIATE 'CREATE SEQUENCE TM_JCP_COST_CODE_SEQ START WITH 1 INCREMENT BY 1 NOCACHE';
  END IF;

  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_JCP_POSTING';
  IF n = 0 THEN
    EXECUTE IMMEDIATE q'[
      CREATE TABLE TM_JCP_POSTING
      (
        JCP_TS_ID          NUMBER(12)     NOT NULL,   -- TM_TIME_SHEET.TS_ID
        JCP_PERIOD_SYS_ID  NUMBER(6)      NOT NULL,   -- FINM_CALENDAR_DETAILS.FCD_PERIOD_SYS_ID
        JCP_FITH_SYS_ID    NUMBER(8)      NOT NULL,   -- FINT_TRANSACTION_HEADER.FITH_SYS_ID
        JCP_FITH_SRL_NO    VARCHAR2(30)   NOT NULL,   -- voucher no
        JCP_FITH_DT        DATE           NOT NULL,
        JCP_AMOUNT         NUMBER(17,3)   NOT NULL,   -- total debit
        CREATION_USER_ID   NUMBER(10),
        CREATION_DATE      DATE           DEFAULT SYSDATE,
        UPDATE_USER_ID     NUMBER(10),
        UPDATE_DATE        DATE,
        CONSTRAINT TM_JCP_POSTING_PK PRIMARY KEY (JCP_TS_ID, JCP_PERIOD_SYS_ID)
      )]';
  END IF;
END;
/

INSERT INTO TM_JCP_SETUP (JCS_ID, JCS_ENABLED_YN, JCS_COMP_CODE, JCS_OFF_CODE, JCS_JRNL_TYPE, JCS_START_DATE)
  SELECT 1, 'N', '01', '01', 'JRV', TRUNC(SYSDATE, 'MM') FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_JCP_SETUP);

-- Costing rates from the Yellow team (Oct-2026). Accounts are chosen in the setup screen.
INSERT INTO TM_JCP_COST_CODE (JCC_ID, JCC_TYPE, JCC_LOCATION, JCC_DESC, JCC_RATE)
  SELECT TM_JCP_COST_CODE_SEQ.NEXTVAL, 'LABOUR', 'Workshop', 'Workshop labour', 10 FROM DUAL
  WHERE NOT EXISTS (SELECT 1 FROM TM_JCP_COST_CODE WHERE JCC_TYPE = 'LABOUR' AND JCC_LOCATION = 'Workshop');
INSERT INTO TM_JCP_COST_CODE (JCC_ID, JCC_TYPE, JCC_LOCATION, JCC_DESC, JCC_RATE)
  SELECT TM_JCP_COST_CODE_SEQ.NEXTVAL, 'LABOUR', 'Field', 'Field service labour', 12 FROM DUAL
  WHERE NOT EXISTS (SELECT 1 FROM TM_JCP_COST_CODE WHERE JCC_TYPE = 'LABOUR' AND JCC_LOCATION = 'Field');
INSERT INTO TM_JCP_COST_CODE (JCC_ID, JCC_TYPE, JCC_LOCATION, JCC_DESC, JCC_RATE)
  SELECT TM_JCP_COST_CODE_SEQ.NEXTVAL, 'TRAVEL_HR', NULL, 'Travel time', 8 FROM DUAL
  WHERE NOT EXISTS (SELECT 1 FROM TM_JCP_COST_CODE WHERE JCC_TYPE = 'TRAVEL_HR');
INSERT INTO TM_JCP_COST_CODE (JCC_ID, JCC_TYPE, JCC_LOCATION, JCC_DESC, JCC_RATE)
  SELECT TM_JCP_COST_CODE_SEQ.NEXTVAL, 'TRAVEL_KM', NULL, 'Vehicle km', 0.2 FROM DUAL
  WHERE NOT EXISTS (SELECT 1 FROM TM_JCP_COST_CODE WHERE JCC_TYPE = 'TRAVEL_KM');

COMMIT;
