-- ============================================================================
--  Phase 2 change: per-task line enhancements on TM_TIME_LINE
--    * TL_WORK_DATE            – the task date (start/end are times on this date)
--    * TL_NORMAL_HOURS/OT_HOURS – Net hours split into Normal and Overtime
--                                (auto-calculated, editable by the entry user)
--    * TL_OT_RATE              – overtime rate (normal rate x OT multiplier)
--    * TL_FOOD_ALLOWANCE       – per-line food allowance (default from labour rate)
--    * TL_TOTAL_COST           – labour cost + food allowance
--    * TL_TRAVEL_*             – travel site, travel start/end and total travel hrs
--    * TL_OVERRIDE_YN          – 'Y' when the user adjusted an auto-calculated value
--    * TL_MERGED_FROM_TS_ID    – original sheet when a line was merged (08 script)
--  Header totals for food allowance / travel / total cost, plus a food allowance
--  default on the Labour Rate master.
--  Idempotent: adds each column only if it does not already exist; backfills only
--  rows that have not been populated yet.
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
  add_col('TM_TIME_LINE',  'TL_WORK_DATE',           'TL_WORK_DATE DATE');
  add_col('TM_TIME_LINE',  'TL_NORMAL_HOURS',        'TL_NORMAL_HOURS NUMBER(10,2) DEFAULT 0');
  add_col('TM_TIME_LINE',  'TL_OT_HOURS',            'TL_OT_HOURS NUMBER(10,2) DEFAULT 0');
  add_col('TM_TIME_LINE',  'TL_OT_RATE',             'TL_OT_RATE NUMBER(12,2) DEFAULT 0');
  add_col('TM_TIME_LINE',  'TL_FOOD_ALLOWANCE',      'TL_FOOD_ALLOWANCE NUMBER(12,2) DEFAULT 0');
  add_col('TM_TIME_LINE',  'TL_TOTAL_COST',          'TL_TOTAL_COST NUMBER(14,2) DEFAULT 0');
  add_col('TM_TIME_LINE',  'TL_TRAVEL_SITE',         'TL_TRAVEL_SITE VARCHAR2(200)');
  add_col('TM_TIME_LINE',  'TL_TRAVEL_START',        'TL_TRAVEL_START DATE');
  add_col('TM_TIME_LINE',  'TL_TRAVEL_END',          'TL_TRAVEL_END DATE');
  add_col('TM_TIME_LINE',  'TL_TRAVEL_HOURS',        'TL_TRAVEL_HOURS NUMBER(6,2) DEFAULT 0');
  add_col('TM_TIME_LINE',  'TL_OVERRIDE_YN',         'TL_OVERRIDE_YN VARCHAR2(1) DEFAULT ''N''');
  add_col('TM_TIME_LINE',  'TL_MERGED_FROM_TS_ID',   'TL_MERGED_FROM_TS_ID NUMBER(10)');

  add_col('TM_TIME_SHEET', 'TS_TOTAL_FOOD_ALLOWANCE','TS_TOTAL_FOOD_ALLOWANCE NUMBER(14,2) DEFAULT 0');
  add_col('TM_TIME_SHEET', 'TS_TOTAL_TRAVEL_HOURS',  'TS_TOTAL_TRAVEL_HOURS NUMBER(10,2) DEFAULT 0');
  add_col('TM_TIME_SHEET', 'TS_TOTAL_COST',          'TS_TOTAL_COST NUMBER(14,2) DEFAULT 0');
  add_col('TM_TIME_SHEET', 'TS_MERGED_INTO_TS_ID',   'TS_MERGED_INTO_TS_ID NUMBER(10)');

  add_col('TM_LABOUR_RATE','LR_FOOD_ALLOWANCE',      'LR_FOOD_ALLOWANCE NUMBER(12,2) DEFAULT 0');
END;
/

-- ---------- Backfill existing lines ------------------------------------------
-- Work date = the date part of the start date-time.
UPDATE TM_TIME_LINE SET TL_WORK_DATE = TRUNC(TL_START_DT) WHERE TL_WORK_DATE IS NULL;

-- Old lines were classified whole-line NORMAL or OVERTIME: map that onto the split.
-- For OVERTIME lines TL_RATE already holds the applied (multiplied) rate, so it is
-- carried as the OT rate; the labour cost is unchanged either way.
UPDATE TM_TIME_LINE
   SET TL_OT_HOURS     = TL_NET_HOURS,
       TL_NORMAL_HOURS = 0,
       TL_OT_RATE      = TL_RATE
 WHERE TL_TIME_TYPE = 'OVERTIME'
   AND NVL(TL_NORMAL_HOURS,0) = 0 AND NVL(TL_OT_HOURS,0) = 0 AND NVL(TL_NET_HOURS,0) > 0;

UPDATE TM_TIME_LINE
   SET TL_NORMAL_HOURS = TL_NET_HOURS,
       TL_OT_HOURS     = 0,
       TL_OT_RATE      = ROUND(TL_RATE * 1.5, 2)
 WHERE NVL(TL_TIME_TYPE,'NORMAL') <> 'OVERTIME'
   AND NVL(TL_NORMAL_HOURS,0) = 0 AND NVL(TL_OT_HOURS,0) = 0 AND NVL(TL_NET_HOURS,0) > 0;

UPDATE TM_TIME_LINE
   SET TL_TOTAL_COST = NVL(TL_LABOUR_COST,0) + NVL(TL_FOOD_ALLOWANCE,0)
 WHERE NVL(TL_TOTAL_COST,0) = 0;

-- ---------- Backfill header totals from their lines ---------------------------
UPDATE TM_TIME_SHEET k
   SET (TS_NORMAL_HOURS, TS_OT_HOURS, TS_TOTAL_FOOD_ALLOWANCE, TS_TOTAL_TRAVEL_HOURS, TS_TOTAL_COST) =
       (SELECT NVL(SUM(TL_NORMAL_HOURS),0), NVL(SUM(TL_OT_HOURS),0),
               NVL(SUM(TL_FOOD_ALLOWANCE),0), NVL(SUM(TL_TRAVEL_HOURS),0),
               NVL(SUM(TL_LABOUR_COST),0) + NVL(SUM(TL_FOOD_ALLOWANCE),0)
          FROM TM_TIME_LINE l WHERE l.TL_TS_ID = k.TS_ID)
 WHERE NVL(TS_TOTAL_COST,0) = 0
   AND EXISTS (SELECT 1 FROM TM_TIME_LINE l WHERE l.TL_TS_ID = k.TS_ID);

COMMIT;
