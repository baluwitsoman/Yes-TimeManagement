namespace YesTm.Web.Common.Data;

/// <summary>
/// The ERP employee table holds the same person twice for several technicians: the original code
/// (e.g. YES0023, used on all time sheets) and a later HRMS-style copy (e.g. YES010, created
/// 06-Oct-2026, inactive). The codes are not being changed, so TM hides the copies.
///
/// Rule — among employees with the same name (case/spacing ignored) keep ONE:
///   1. the one with booked time lines, 2. an active one, 3. the oldest record, 4. the lowest code.
/// Employees without a name are never merged.
/// </summary>
public static class EmployeeSql
{
    /// <summary>Inline view: PEMP_EMP_CODE, PEMP_EMP_NAME, PEMP_EMP_ACTIVE of the visible (de-duplicated) employees.</summary>
    public const string Distinct = @"
        (SELECT PEMP_EMP_CODE, PEMP_EMP_NAME, PEMP_EMP_ACTIVE
         FROM  (SELECT e.PEMP_EMP_CODE, e.PEMP_EMP_NAME, e.PEMP_EMP_ACTIVE,
                       ROW_NUMBER() OVER (
                           PARTITION BY NVL(UPPER(REGEXP_REPLACE(TRIM(e.PEMP_EMP_NAME), '\s+', ' ')), '#' || e.PEMP_EMP_CODE)
                           ORDER BY CASE WHEN bk.TL_TECH_CODE IS NOT NULL THEN 0 ELSE 1 END,
                                    CASE WHEN NVL(e.PEMP_EMP_ACTIVE,'Y') = 'Y' THEN 0 ELSE 1 END,
                                    e.PEMP_CREATION_DATE NULLS LAST, e.PEMP_EMP_CODE) AS DUP_RANK
                FROM   PPM_EMPLOYEE_DETAILS e
                LEFT JOIN (SELECT DISTINCT TL_TECH_CODE FROM TM_TIME_LINE) bk ON bk.TL_TECH_CODE = e.PEMP_EMP_CODE
                WHERE  e.PEMP_EMP_CODE IS NOT NULL)
         WHERE DUP_RANK = 1)";
}
