-- ============================================================================
--  Phase 2 change: ONE active time sheet per Job.
--    1. Merge duplicates: for every job with more than one active TM_TIME_SHEET,
--       keep the FIRST sheet (lowest TS_ID) and move every TM_TIME_LINE of the
--       later sheets into it (line numbers continue after the keeper's last
--       line; TL_MERGED_FROM_TS_ID remembers where each line came from).
--       The later sheets are marked inactive (TS_ACTIVE_YN = 'N') with
--       TS_MERGED_INTO_TS_ID pointing at the keeper — nothing is deleted.
--    2. Recompute the keeper's totals from its (now larger) set of lines.
--    3. Enforce the rule with a function-based unique index over the job code of
--       ACTIVE sheets only (voided / merged sheets do not block a new sheet).
--  Requires 07_timeline_enhancements.sql (TL_MERGED_FROM_TS_ID, TS_MERGED_INTO_TS_ID).
--  Idempotent: re-running finds no duplicates and skips the existing index.
-- ============================================================================
DECLARE
  v_line_no     NUMBER;
  v_keep_no     TM_TIME_SHEET.TS_SHEET_NO%TYPE;
  v_sheet_list  VARCHAR2(2000);
  v_moved       NUMBER := 0;
  v_merged      NUMBER := 0;
BEGIN
  FOR j IN (SELECT TS_JOB_CODE, MIN(TS_ID) AS keep_id
              FROM TM_TIME_SHEET
             WHERE NVL(TS_ACTIVE_YN,'Y') = 'Y'
             GROUP BY TS_JOB_CODE
            HAVING COUNT(*) > 1)
  LOOP
    SELECT TS_SHEET_NO INTO v_keep_no FROM TM_TIME_SHEET WHERE TS_ID = j.keep_id;
    SELECT NVL(MAX(TL_LINE_NO),0) INTO v_line_no FROM TM_TIME_LINE WHERE TL_TS_ID = j.keep_id;
    v_sheet_list := NULL;

    FOR s IN (SELECT TS_ID, TS_SHEET_NO
                FROM TM_TIME_SHEET
               WHERE TS_JOB_CODE = j.TS_JOB_CODE
                 AND NVL(TS_ACTIVE_YN,'Y') = 'Y'
                 AND TS_ID <> j.keep_id
               ORDER BY TS_ID)
    LOOP
      FOR l IN (SELECT TL_ID FROM TM_TIME_LINE WHERE TL_TS_ID = s.TS_ID ORDER BY TL_LINE_NO, TL_ID)
      LOOP
        v_line_no := v_line_no + 1;
        UPDATE TM_TIME_LINE
           SET TL_TS_ID = j.keep_id,
               TL_LINE_NO = v_line_no,
               TL_MERGED_FROM_TS_ID = s.TS_ID
         WHERE TL_ID = l.TL_ID;
        v_moved := v_moved + 1;
      END LOOP;

      UPDATE TM_TIME_SHEET
         SET TS_ACTIVE_YN = 'N',
             TS_MERGED_INTO_TS_ID = j.keep_id,
             TS_UPDATE_DATE = SYSDATE,
             TS_REMARKS = SUBSTR('[Merged into ' || v_keep_no || '] ' || TS_REMARKS, 1, 1000)
       WHERE TS_ID = s.TS_ID;
      v_merged := v_merged + 1;

      v_sheet_list := v_sheet_list || CASE WHEN v_sheet_list IS NULL THEN '' ELSE ', ' END || s.TS_SHEET_NO;
    END LOOP;

    -- Keeper totals = sum over all of its lines (original + merged).
    UPDATE TM_TIME_SHEET k
       SET (TS_TOTAL_NET_HOURS, TS_TOTAL_LABOUR_COST, TS_NORMAL_HOURS, TS_OT_HOURS, TS_STD_HOURS,
            TS_TOTAL_FOOD_ALLOWANCE, TS_TOTAL_TRAVEL_HOURS, TS_TOTAL_COST) =
           (SELECT NVL(SUM(TL_NET_HOURS),0), NVL(SUM(TL_LABOUR_COST),0),
                   NVL(SUM(TL_NORMAL_HOURS),0), NVL(SUM(TL_OT_HOURS),0), NVL(SUM(TL_STD_HOURS),0),
                   NVL(SUM(TL_FOOD_ALLOWANCE),0), NVL(SUM(TL_TRAVEL_HOURS),0),
                   NVL(SUM(TL_LABOUR_COST),0) + NVL(SUM(TL_FOOD_ALLOWANCE),0)
              FROM TM_TIME_LINE l WHERE l.TL_TS_ID = k.TS_ID),
           TS_REMARKS = SUBSTR(TRIM(TS_REMARKS || ' [Merged sheets: ' || v_sheet_list || ']'), 1, 1000),
           TS_UPDATE_DATE = SYSDATE
     WHERE TS_ID = j.keep_id;
  END LOOP;

  DBMS_OUTPUT.PUT_LINE('Merged ' || v_merged || ' sheet(s), moved ' || v_moved || ' line(s).');
END;
/

-- ---------- One ACTIVE sheet per job ----------------------------------------
-- Function-based unique index: only active rows yield a non-NULL key, so voided /
-- merged sheets never collide with a fresh sheet for the same job.
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_indexes WHERE index_name = 'TM_TIME_SHEET_UK_JOB_ACTIVE';
  IF n = 0 THEN
    EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX TM_TIME_SHEET_UK_JOB_ACTIVE ON TM_TIME_SHEET '
                   || '(CASE WHEN NVL(TS_ACTIVE_YN,''Y'') = ''Y'' THEN TS_JOB_CODE END)';
  END IF;
END;
/
COMMIT;
