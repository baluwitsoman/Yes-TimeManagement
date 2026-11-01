-- ============================================================================
--  Phase 6b: New GL accounts for Job Cost Posting (Yes ERP chart of accounts).
--    Creates each account the way the ERP screens do, in the same order:
--      1. COA component value (segment 2)      FINM_COA_COMPNT_VALUES  (Account Component Values)
--      2. BS/PL attributes for that value      FINM_BSPL_DESCRIPTOR    (Account Component BSPL Link)
--      3. Account batch                        FINM_ACCOUNT_BATCH      (Account Batch)
--      4. Generate the account + VEL code      PRO_BATCH_ACOUNT_CREATION (Generate Chart of Accounts)
--      5. Confirm the batch                    FNAM_CONFIRM_YN / FAB_CONFIRM_YN = 'Y' (Confirm Batch Account)
--    Account code = 01/<new segment-2 value>/01/01/01, description YES/<name>/GEN/GEN/GEN,
--    segment-2 value = highest numeric value + 1, VEL code = MAX + 1 (VELOCITY_AUTOGEN = Y).
--
--    Accounts (all type E):
--      Direct Labour - Workshop        DIRECTEXP / LABORCHGS   Dr  Workshop labour
--      Direct Labour - Field Service   DIRECTEXP / LABORCHGS   Dr  Field service labour
--      Direct Labour - Service         DIRECTEXP / LABORCHGS   Dr  (spare: Service location, map when its rate is added)
--      Direct Labour - Travel Time     DIRECTEXP / LABORCHGS   Dr  Travel time
--      Job Vehicle Cost (KM)           DIRECTEXP / DTRANEXP    Dr  Vehicle km
--      Labour Cost Absorbed            DIRECTEXP / SALWAGES    Cr  contra, sits with Salary And Wages
--      Vehicle Cost Absorbed           INDIREXP  / VEHICLEXP   Cr  contra, sits with Vehicle Expense
--
--    Then, if db/12 has been run, maps them onto the seeded TM_JCP_COST_CODE rows that have
--    no account yet (an account already chosen in the setup screen is never overwritten).
--
--  Idempotent: an account whose segment-2 description already exists is reused, not duplicated.
--  Created as ERP user 2 (ADMIN), batch TMJCP1.
-- ============================================================================
DECLARE
  c_user   CONSTANT NUMBER       := 2;          -- AMM_USER_DETAILS.USER_ID (ADMIN)
  c_batch  CONSTANT VARCHAR2(6)  := 'TMJCP1';
  c_comp   CONSTANT VARCHAR2(10) := '01';
  n        NUMBER;

  v_wsh  NUMBER; v_fld NUMBER; v_svc NUMBER; v_trv NUMBER; v_km NUMBER;
  v_lababs NUMBER; v_vehabs NUMBER;

  FUNCTION create_account (p_name VARCHAR2, p_group VARCHAR2, p_sub VARCHAR2, p_contra VARCHAR2) RETURN NUMBER
  IS
    v_val   VARCHAR2(30);
    v_vel   NUMBER;
    v_resp  VARCHAR2(4000);
  BEGIN
    -- Reuse: same segment-2 description already in the chart.
    BEGIN
      SELECT FCCV_COMPNT_VAL INTO v_val
        FROM FINM_COA_COMPNT_VALUES
       WHERE FCCV_STRUCT_ID = 'STR001' AND FCCV_COMPNT_SEQ = 2 AND UPPER(TRIM(FCCV_DESC_IN_AC)) = UPPER(p_name)
         AND ROWNUM = 1;
    EXCEPTION WHEN NO_DATA_FOUND THEN v_val := NULL;
    END;

    IF v_val IS NULL THEN
      SELECT TO_CHAR(NVL(MAX(TO_NUMBER(FCCV_COMPNT_VAL)), 3000000) + 1) INTO v_val
        FROM FINM_COA_COMPNT_VALUES
       WHERE FCCV_STRUCT_ID = 'STR001' AND FCCV_COMPNT_SEQ = 2 AND REGEXP_LIKE(FCCV_COMPNT_VAL, '^[0-9]+$');

      INSERT INTO FINM_COA_COMPNT_VALUES
        (FCCV_STRUCT_ID, FCCV_COMPNT_SEQ, FCCV_COMPNT_VAL, FCCV_COMPNT_VAL_DESC, FCCV_DESC_IN_AC,
         FCCV_CONTRA_YN, FCCV_ACTIVE, FCCV_CREATION_USER_ID, FCCV_CREATION_DATE)
      VALUES ('STR001', 2, v_val, p_name, p_name, p_contra, 'Y', c_user, SYSDATE);
    END IF;

    SELECT COUNT(*) INTO n FROM FINM_BSPL_DESCRIPTOR
     WHERE FBSD_STRUCT_ID = 'STR001' AND FBSD_COMPNT_SEQ = 2 AND FBSD_COMPNT_VAL = v_val;
    IF n = 0 THEN
      INSERT INTO FINM_BSPL_DESCRIPTOR
        (FBSD_STRUCT_ID, FBSD_COMPNT_SEQ, FBSD_COMPNT_VAL, FBSD_AC_TYPE, FBSD_AC_GROUP, FBSD_AC_SUB_TYPE,
         FBSD_CREATION_USER_ID, FBSD_CREATION_DATE)
      VALUES ('STR001', 2, v_val, 'E', p_group, p_sub, c_user, SYSDATE);
    END IF;

    -- The ERP generator: skips the account if 01/<val>/01/01/01 already exists.
    PRO_BATCH_ACOUNT_CREATION(1, TO_NUMBER(v_val), 1, 1, 1,
                              1, TO_NUMBER(v_val), 1, 1, 1,
                              TRUNC(SYSDATE), ADD_MONTHS(TRUNC(SYSDATE), 1200),
                              c_batch, c_user, v_resp);

    BEGIN
      SELECT FNAM_VEL_CODE INTO v_vel FROM FINM_ACCOUNT_MASTER
       WHERE FNAM_FNAM_CODE = c_comp || '/' || v_val || '/01/01/01';
    EXCEPTION WHEN NO_DATA_FOUND THEN
      RAISE_APPLICATION_ERROR(-20001, 'Account not generated for ' || p_name || ': ' || v_resp);
    END;
    DBMS_OUTPUT.PUT_LINE(RPAD(p_name, 32) || ' 01/' || v_val || '/01/01/01  VEL ' || v_vel);
    RETURN v_vel;
  END;

  PROCEDURE map_code (p_type VARCHAR2, p_loc VARCHAR2, p_dr NUMBER, p_cr NUMBER)
  IS
  BEGIN
    EXECUTE IMMEDIATE q'[
      UPDATE TM_JCP_COST_CODE
         SET JCC_DR_VEL_CODE = NVL(JCC_DR_VEL_CODE, :dr),
             JCC_CR_VEL_CODE = NVL(JCC_CR_VEL_CODE, :cr),
             UPDATE_USER_ID = :u, UPDATE_DATE = SYSDATE
       WHERE JCC_TYPE = :t AND NVL(JCC_LOCATION, '~') = NVL(:l, '~')]'
      USING p_dr, p_cr, c_user, p_type, p_loc;
  END;
BEGIN
  SELECT COUNT(*) INTO n FROM FINM_ACCOUNT_BATCH WHERE FAB_BATCH_NO = c_batch;
  IF n = 0 THEN
    INSERT INTO FINM_ACCOUNT_BATCH (FAB_BATCH_NO, FAB_DESC, FAB_CONFIRM_YN, FAB_CRE_USER_INIT, FAB_CRE_DT, FAB_COMP_CODE)
    VALUES (c_batch, 'TM Job Cost Posting accounts', 'N', TO_CHAR(c_user), SYSDATE, c_comp);
  END IF;

  v_wsh    := create_account('Direct Labour - Workshop',      'DIRECTEXP', 'LABORCHGS', 'N');
  v_fld    := create_account('Direct Labour - Field Service', 'DIRECTEXP', 'LABORCHGS', 'N');
  v_svc    := create_account('Direct Labour - Service',       'DIRECTEXP', 'LABORCHGS', 'N');
  v_trv    := create_account('Direct Labour - Travel Time',   'DIRECTEXP', 'LABORCHGS', 'N');
  v_km     := create_account('Job Vehicle Cost (KM)',         'DIRECTEXP', 'DTRANEXP',  'N');
  v_lababs := create_account('Labour Cost Absorbed',          'DIRECTEXP', 'SALWAGES',  'Y');
  v_vehabs := create_account('Vehicle Cost Absorbed',         'INDIREXP',  'VEHICLEXP', 'Y');

  -- Confirm the batch (Confirm Batch Account screen).
  UPDATE FINM_ACCOUNT_MASTER SET FNAM_CONFIRM_YN = 'Y', FNAM_UPDATE_USER_ID = c_user, FNAM_UPDATE_DATE = SYSDATE
   WHERE UPPER(FNAM_BATCH_NO) = c_batch AND NVL(FNAM_CONFIRM_YN, 'N') <> 'Y';
  UPDATE FINM_ACCOUNT_BATCH SET FAB_CONFIRM_YN = 'Y', FAB_UPD_USER_INIT = TO_CHAR(c_user), FAB_UPD_DT = SYSDATE
   WHERE UPPER(FAB_BATCH_NO) = c_batch;

  -- Map onto the Job Cost Posting rates (db/12), only where no account is chosen yet.
  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_JCP_COST_CODE';
  IF n > 0 THEN
    map_code('LABOUR',    'Workshop', v_wsh, v_lababs);
    map_code('LABOUR',    'Field',    v_fld, v_lababs);
    map_code('LABOUR',    'Service',  v_svc, v_lababs);   -- no-op until a Service rate row exists
    map_code('TRAVEL_HR', NULL,       v_trv, v_lababs);
    map_code('TRAVEL_KM', NULL,       v_km,  v_vehabs);
  END IF;
END;
/

COMMIT;

-- Check:
-- SELECT FNAM_VEL_CODE, FNAM_FNAM_CODE, FNAM_DESC, FNAM_FNAM_TYPE, FNAM_FNAM_GROUP, FNAM_FNAM_SUB_TYPE, FNAM_CONFIRM_YN
--   FROM FINM_ACCOUNT_MASTER WHERE FNAM_BATCH_NO = 'TMJCP1' ORDER BY FNAM_VEL_CODE;
-- SELECT JCC_DESC, JCC_RATE, JCC_DR_VEL_CODE, JCC_CR_VEL_CODE FROM TM_JCP_COST_CODE;
