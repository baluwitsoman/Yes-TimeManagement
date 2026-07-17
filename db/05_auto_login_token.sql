-- ============================================================================
--  Auto-login (SSO) handoff table. The legacy ERP writes one short-lived,
--  single-use token row here after a user picks a Time-Management role at the
--  ERP login; the TM app (Authentication/AutoLogin) redeems it to sign the
--  user in — the two apps can't share auth cookies but share this Oracle DB.
--  Idempotent.
-- ============================================================================
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_tables WHERE table_name = 'TM_AUTO_LOGIN_TOKEN';
  IF n = 0 THEN
    EXECUTE IMMEDIATE q'[
      CREATE TABLE TM_AUTO_LOGIN_TOKEN
      (
        TAT_TOKEN          VARCHAR2(64)  NOT NULL,   -- opaque random handoff token (PK)
        TAT_USER_ID        NUMBER(10)    NOT NULL,   -- AMM_USER_DETAILS.USER_ID
        TAT_ERP_ROLE_NAME  VARCHAR2(50)  NOT NULL,   -- TimeManagement | TM-Admin | TM-SiteAdmin
        TAT_USED_YN        VARCHAR2(1)   DEFAULT 'N' NOT NULL,
        TAT_EXPIRY_DATE    DATE          NOT NULL,    -- SYSDATE + ~2 min at issue time
        TAT_CREATION_DATE  DATE          DEFAULT SYSDATE,
        CONSTRAINT TM_AUTO_LOGIN_TOKEN_PK PRIMARY KEY (TAT_TOKEN)
      )]';
  END IF;
END;
/

-- Prune expired/used rows by expiry; skipped (ORA-955) on re-run.
CREATE INDEX TM_AUTO_LOGIN_TOKEN_IX1 ON TM_AUTO_LOGIN_TOKEN (TAT_EXPIRY_DATE);

COMMIT;
