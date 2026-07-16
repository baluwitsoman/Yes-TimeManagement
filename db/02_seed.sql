-- ============================================================================
--  YES ERP · Technician Time Management — Phase 0 seed data
--  Run after 01_tm_schema.sql.
-- ============================================================================

-- ---------------------------------------------------------------------------
-- 1) Bootstrap SITE ADMIN.
--    Grants the SITEADMIN app-role to an existing ERP login (no fabricated
--    user row — AMM_USER_DETAILS has NOT NULL / FK columns). Defaults to the
--    'ADMIN' account; change v_user_code if you want a different bootstrap user.
--    The site admin then assigns ADMIN/USER to others from the "User Roles" screen.
-- ---------------------------------------------------------------------------
DECLARE
  v_user_code CONSTANT VARCHAR2(30) := 'ADMIN';   -- <-- existing AMM_USER_DETAILS.USER_CODE
  v_user_id   NUMBER;
BEGIN
  SELECT MIN(USER_ID) INTO v_user_id
  FROM   AMM_USER_DETAILS
  WHERE  UPPER(USER_CODE) = UPPER(v_user_code);

  IF v_user_id IS NULL THEN
    SELECT MIN(USER_ID) INTO v_user_id FROM AMM_USER_DETAILS;  -- fallback: first user
  END IF;

  MERGE INTO TM_USER_ROLE r
  USING (SELECT v_user_id AS MUID FROM DUAL) s
  ON (r.TUR_USER_ID = s.MUID AND r.TUR_ROLE_CODE = 'SITEADMIN' AND NVL(r.TUR_ACTIVE_YN,'Y') = 'Y')
  WHEN NOT MATCHED THEN
    INSERT (TUR_ID, TUR_USER_ID, TUR_ROLE_CODE, TUR_ACTIVE_YN, TUR_CREATION_USER_ID, TUR_CREATION_DATE)
    VALUES (TM_USER_ROLE_SEQ.NEXTVAL, v_user_id, 'SITEADMIN', 'Y', v_user_id, SYSDATE);
END;
/

-- All seed inserts below are idempotent (guarded by NOT EXISTS) so the
-- script can be re-run safely without creating duplicate rows.

-- ---------------------------------------------------------------------------
-- 2) Work / Labour Types
-- ---------------------------------------------------------------------------
INSERT INTO TM_WORK_TYPE (WORK_TYPE_CODE, WORK_TYPE_NAME, CATEGORY, COST_TREATMENT, SUB_TYPE)
  SELECT 'REP', 'Repair - Revenue (Chargeable)', 'CHARGEABLE', 'REVENUE', NULL FROM DUAL
   WHERE NOT EXISTS (SELECT 1 FROM TM_WORK_TYPE WHERE WORK_TYPE_CODE='REP');
INSERT INTO TM_WORK_TYPE (WORK_TYPE_CODE, WORK_TYPE_NAME, CATEGORY, COST_TREATMENT, SUB_TYPE)
  SELECT 'WAR', 'Warranty', 'WARRANTY', 'EXPENSE', NULL FROM DUAL
   WHERE NOT EXISTS (SELECT 1 FROM TM_WORK_TYPE WHERE WORK_TYPE_CODE='WAR');
INSERT INTO TM_WORK_TYPE (WORK_TYPE_CODE, WORK_TYPE_NAME, CATEGORY, COST_TREATMENT, SUB_TYPE)
  SELECT 'NCT', 'Non-Chargeable - Training', 'NONCHARGEABLE', 'EXPENSE', 'Training' FROM DUAL
   WHERE NOT EXISTS (SELECT 1 FROM TM_WORK_TYPE WHERE WORK_TYPE_CODE='NCT');
INSERT INTO TM_WORK_TYPE (WORK_TYPE_CODE, WORK_TYPE_NAME, CATEGORY, COST_TREATMENT, SUB_TYPE)
  SELECT 'IDL', 'Idle / Loss Time', 'NONCHARGEABLE', 'EXPENSE', 'Idle' FROM DUAL
   WHERE NOT EXISTS (SELECT 1 FROM TM_WORK_TYPE WHERE WORK_TYPE_CODE='IDL');

-- ---------------------------------------------------------------------------
-- 3) Industries
-- ---------------------------------------------------------------------------
INSERT INTO TM_INDUSTRY (INDUSTRY_CODE, INDUSTRY_NAME)
  SELECT 'OG', 'Oil & Gas' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_INDUSTRY WHERE INDUSTRY_CODE='OG');
INSERT INTO TM_INDUSTRY (INDUSTRY_CODE, INDUSTRY_NAME)
  SELECT 'GC', 'General Contracting' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_INDUSTRY WHERE INDUSTRY_CODE='GC');

-- ---------------------------------------------------------------------------
-- 4) Duty timing & benchmark (one default profile)
-- ---------------------------------------------------------------------------
INSERT INTO TM_DUTY_TIMING (DT_ID, DT_NAME, DT_START, DT_END, DT_LUNCH_HOURS, DT_WEEKEND_DAYS, DT_MONTHLY_BENCHMARK)
  SELECT TM_DUTY_TIMING_SEQ.NEXTVAL, 'Standard', '08:00', '17:00', 0.5, 'Friday,Saturday', 192 FROM DUAL
   WHERE NOT EXISTS (SELECT 1 FROM TM_DUTY_TIMING WHERE DT_NAME='Standard');

-- ---------------------------------------------------------------------------
-- 5) Labour rates (Field $15, Workshop $12 per the requirement)
-- ---------------------------------------------------------------------------
INSERT INTO TM_LABOUR_RATE (LR_ID, LR_LOCATION, LR_INDUSTRY_CODE, LR_TIME_TYPE, LR_RATE, LR_COST_RATE, LR_OT_MULTIPLIER, LR_EFFECTIVE_FROM)
  SELECT TM_LABOUR_RATE_SEQ.NEXTVAL, 'Field', 'OG', 'NORMAL', 15, 12, 1.5, SYSDATE FROM DUAL
   WHERE NOT EXISTS (SELECT 1 FROM TM_LABOUR_RATE WHERE LR_LOCATION='Field' AND LR_INDUSTRY_CODE='OG' AND LR_TIME_TYPE='NORMAL');
INSERT INTO TM_LABOUR_RATE (LR_ID, LR_LOCATION, LR_INDUSTRY_CODE, LR_TIME_TYPE, LR_RATE, LR_COST_RATE, LR_OT_MULTIPLIER, LR_EFFECTIVE_FROM)
  SELECT TM_LABOUR_RATE_SEQ.NEXTVAL, 'Workshop', 'GC', 'NORMAL', 12, 10, 1.5, SYSDATE FROM DUAL
   WHERE NOT EXISTS (SELECT 1 FROM TM_LABOUR_RATE WHERE LR_LOCATION='Workshop' AND LR_INDUSTRY_CODE='GC' AND LR_TIME_TYPE='NORMAL');
INSERT INTO TM_LABOUR_RATE (LR_ID, LR_LOCATION, LR_INDUSTRY_CODE, LR_TIME_TYPE, LR_RATE, LR_COST_RATE, LR_OT_MULTIPLIER, LR_EFFECTIVE_FROM)
  SELECT TM_LABOUR_RATE_SEQ.NEXTVAL, 'Service', NULL, 'NORMAL', 13, 11, 1.5, SYSDATE FROM DUAL
   WHERE NOT EXISTS (SELECT 1 FROM TM_LABOUR_RATE WHERE LR_LOCATION='Service' AND LR_INDUSTRY_CODE IS NULL AND LR_TIME_TYPE='NORMAL');

-- ---------------------------------------------------------------------------
-- 6) Tasks (bookable) with default standard hours
-- ---------------------------------------------------------------------------
INSERT INTO TM_TASK (TASK_CODE, TASK_NAME, TASK_GROUP, SEQ_NO, DEFAULT_SKILL, STD_HOURS, CHARGEABLE_YN)
  SELECT 'DIS', 'Disassemble', 'Recondition', 1, 'Assistant', 16, 'Y' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_TASK WHERE TASK_CODE='DIS');
INSERT INTO TM_TASK (TASK_CODE, TASK_NAME, TASK_GROUP, SEQ_NO, DEFAULT_SKILL, STD_HOURS, CHARGEABLE_YN)
  SELECT 'CLN', 'Cleaning & Reusability', 'Recondition', 2, 'Assistant', 6, 'Y' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_TASK WHERE TASK_CODE='CLN');
INSERT INTO TM_TASK (TASK_CODE, TASK_NAME, TASK_GROUP, SEQ_NO, DEFAULT_SKILL, STD_HOURS, CHARGEABLE_YN)
  SELECT 'ASM', 'Assemble', 'Recondition', 3, 'Lead Hand', 19, 'Y' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_TASK WHERE TASK_CODE='ASM');
INSERT INTO TM_TASK (TASK_CODE, TASK_NAME, TASK_GROUP, SEQ_NO, DEFAULT_SKILL, STD_HOURS, CHARGEABLE_YN)
  SELECT 'TST', 'Testing', 'Recondition', 4, 'Lead Hand', 8, 'Y' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_TASK WHERE TASK_CODE='TST');
INSERT INTO TM_TASK (TASK_CODE, TASK_NAME, TASK_GROUP, SEQ_NO, DEFAULT_SKILL, STD_HOURS, CHARGEABLE_YN)
  SELECT 'INS', 'Inspection', 'Recondition', 5, 'Technician', 4, 'Y' FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM TM_TASK WHERE TASK_CODE='INS');

COMMIT;
-- End of seed
