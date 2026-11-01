# Job Cost Posting — Time Management → Yes ERP Finance

*Agreed design · 31-Oct-2026 · built in TM (db/12 not yet run on the live DB)*

## 1. Purpose

Each job's P&L in Yes ERP must show the cost of the technicians' time and travel booked on it in Time Management (TM).

Rates from the Yellow team (held in **Job Cost Posting Setup**, effective-dated):

| Cost code | Driver (from TM) | Rate |
|---|---|---|
| Workshop labour | Work hours on Workshop lines | 10.000 OMR / hr |
| Field service labour | Work hours on Field lines | 12.000 OMR / hr |
| Travel time | Travel hours (TRAVEL segments + travel hours on work lines) | 8.000 OMR / hr |
| Vehicle km | Km on the Field Service Report | 0.200 OMR / km |

Labour rates are kept **per job location** (`TM_JOB_LOCATION`). Service, Parts or any new location gets its rate by adding a row; until then its hours are reported as "not posted".

## 2. Accounting method: absorption (reclass) — agreed

Staff are already paid through payroll / petty cash, so the posting never creates a new expense or payable:

```
Dr  Job cost account (per cost code)   FITD_TA_CODE1 = job code   → job P&L / Job GP (OTHER_EXP)
    Cr  Absorbed / recovery contra account   (no job code)        → sits under staff / vehicle cost
```

- Company P&L effect is zero. Cost moves from staff / vehicle cost into the jobs.
- Salaries minus absorbed cost = unabsorbed time (idle, leave, non-job), visible in the GL.
- The Job GP views (`V_JOB_GP_LINES`) pick up any E-type line tagged with `FITD_TA_CODE1`, so the Dr lines appear in job GP. The Cr lines carry no job, so they don't cancel it.

## 3. Rules — agreed

| Topic | Decision |
|---|---|
| Idle time on a job | **Not charged.** Hours = net − idle. |
| OT hours | **Flat rate** (same 10 / 12 / 8). |
| Food allowance | **Not posted** (stays in staff cost). |
| Non-job sheets (leave, holiday …) | Never posted. |
| Service / other locations | Rate read from the setup table per location. |
| P&L vs WIP | **P&L only** for now. |
| GL lines | **One Dr line per employee × cost code** on the job; km is one line per sheet. One Cr line per absorbed account. |
| When | **Automatically on every save** of a time sheet / field service report. |
| Months | **One JV per sheet per month.** Each JV is dated on the latest work date in that month. |
| Edit | The same JV is reused: unpost → replace lines → post again. A JV that would come out identical is left untouched. |
| Void (delete) sheet | The sheet's JVs are **deleted**. |
| Closed GL period | A save or void that would change that month's JV is **refused** and the sheet stays unsaved, with a message. |
| Cut-over | Setup has a **"Post work from"** date. Older work is never posted, so re-saving an old sheet doesn't post history. |

## 4. How it works

1. The user saves a time sheet or FSR. In the **same database transaction** as the sheet, TM:
   - recalculates the cost for each month, then
   - creates, rewrites or deletes that month's ERP voucher.
   If any step fails, the sheet save rolls back too, so TM and finance never disagree.
2. The voucher is written the way the ERP itself posts material issues (`PRO_POST_MATERIAL_ISSUE_TO_FIN`):
   - number from `PRO_FIN_GEN_DOC_NO` (GL / JRV, branch 01, numbering row locked while numbering);
   - `FINT_TRANSACTION_HEADER` inserted unposted;
   - `FINT_TRANSACTION_DETAIL` lines inserted;
   - `FITH_STATUS` set to `'P'` (the header trigger runs the GL post).
3. Markers:
   - `FITH_REFERENCE` = TM sheet no;
   - `FITH_SOURCE_JRNL_ORGN = 'TM'`;
   - `FITH_AUTHORIZED_BY5 = 'TM-JCP'`;
   - `TM_JCP_POSTING` maps sheet × GL period → voucher. There is one row per sheet and period (primary key), which is what prevents duplicates.
4. If finance deletes a TM voucher in the ERP, the next save of the sheet posts a new one.

## 5. Screens

- **Masters → Job Cost Posting Setup** (admin+):
  - Settings: on/off switch, company, branch, JV type, "Post work from" date.
  - Cost codes: rate, Dr account and Cr account (VEL code dropdowns of E/I accounts, party control accounts excluded), effective date.
  - Posting cannot be switched on while an active cost code has no account.
- **Time Booking / Field Service Report → "Finance JV"** button (admin+, saved sheets). It shows:
  - each voucher read live from the ERP: number, date, Posted/Unposted, period closed, lines with account / job / narration / Dr / Cr;
  - the calculation behind it: employee × qty × rate per month, plus anything not posted and why.
- After each save, an info banner says what happened, e.g. "Finance JV JRV226001043 posted (140.000 OMR)", or which hours were not costed.

## 6. Tables (db/12_job_cost_posting.sql — additive)

- `TM_JCP_SETUP`: single row. Created with posting **OFF** and cut-over = first day of the current month.
- `TM_JCP_COST_CODE`: rates + Dr/Cr VEL codes. Seeded with the four Yellow-team rates; accounts are empty.
- `TM_JCP_POSTING`: sheet × period → voucher.

## 7. Go-live checklist

1. Finance creates (or confirms) the accounts:
   - Dr: Direct labour – workshop / field / travel, and Job vehicle cost (type E, cost of sales).
   - Cr: Labour cost absorbed and Vehicle cost absorbed (type E contra). `JOB EXPENSES` (VEL 173) exists today if one shared Dr account is preferred.
2. Run `db/12_job_cost_posting.sql`.
3. In Setup: pick the accounts, add rates for Service (and any other location used), set "Post work from", then switch posting ON.
4. Save one sheet, open **Finance JV**, and check the voucher in the ERP GL / Job GP report.

## 8. Notes

- The ERP's summary table `FINC_PERIOD_WISE_SUMMARY` is not reversed on unpost (same as the ERP's own unpost screen). Nothing reads it today: the P&L / TB / BS readers are commented out, and reports work from the voucher lines.
- `WISE_SYS_PARAMS.VALIDATE_JOBCODE = 'N'` today. The credit lines deliberately carry no job code.
- `FITD_TA_CODE1` is the job code from `MTL_TRANSACTION_JOB_OTHER_DTLS`. Job GP reads it directly; the ERP's GL "project" popup lists `MTL_TRANSACTION_REFERENCE`, which TM-created jobs may not be in. That affects only that popup, not the posting.
