using Dapper;
using YesTm.Web.Common.Data;

namespace YesTm.Web.Features.Masters;

// ---------------------------------------------------------------------------
// Lookup row reused by several master screens (customers, industries, tasks).
// ---------------------------------------------------------------------------
public sealed record CodeName(string CODE, string NAME);

public static class MasterRegistration
{
    public static IServiceCollection AddMasterRepositories(this IServiceCollection s)
    {
        s.AddScoped<IWorkTypeRepository, WorkTypeRepository>();
        s.AddScoped<IIndustryRepository, IndustryRepository>();
        s.AddScoped<ITaskRepository, TaskRepository>();
        s.AddScoped<ILabourRateRepository, LabourRateRepository>();
        s.AddScoped<IDutyTimingRepository, DutyTimingRepository>();
        s.AddScoped<ICustomerIndustryRepository, CustomerIndustryRepository>();
        s.AddScoped<ITaskStdRepository, TaskStdRepository>();
        s.AddScoped<IJobLocationRepository, JobLocationRepository>();
        return s;
    }
}

// ===================== Job Location =====================
public interface IJobLocationRepository
{
    Task<IReadOnlyList<TM_JOB_LOCATION>> GetAllAsync(CancellationToken ct = default);
    Task<TM_JOB_LOCATION?> GetAsync(string code, CancellationToken ct = default);
    Task UpsertAsync(TM_JOB_LOCATION e, decimal userId, bool isNew, CancellationToken ct = default);
    Task SetActiveAsync(string code, bool active, decimal userId, CancellationToken ct = default);
    /// <summary>Active locations for dropdowns (code + name), sorted.</summary>
    Task<IReadOnlyList<CodeName>> GetActiveAsync(CancellationToken ct = default);
}

public sealed class JobLocationRepository(IDbConnectionFactory db) : IJobLocationRepository
{
    public async Task<IReadOnlyList<TM_JOB_LOCATION>> GetAllAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<TM_JOB_LOCATION>(new CommandDefinition(
            "SELECT LOC_CODE, LOC_NAME, SORT_ORDER, ACTIVE_YN FROM TM_JOB_LOCATION ORDER BY SORT_ORDER NULLS LAST, LOC_NAME",
            cancellationToken: ct))).AsList();
    }

    public async Task<TM_JOB_LOCATION?> GetAsync(string code, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<TM_JOB_LOCATION>(new CommandDefinition(
            "SELECT LOC_CODE, LOC_NAME, SORT_ORDER, ACTIVE_YN FROM TM_JOB_LOCATION WHERE LOC_CODE = :code",
            new { code }, cancellationToken: ct));
    }

    public async Task UpsertAsync(TM_JOB_LOCATION e, decimal userId, bool isNew, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        var sql = isNew
            ? @"INSERT INTO TM_JOB_LOCATION (LOC_CODE, LOC_NAME, SORT_ORDER, ACTIVE_YN, CREATION_USER_ID, CREATION_DATE)
                VALUES (:LOC_CODE, :LOC_NAME, :SORT_ORDER, :ACTIVE_YN, :userId, SYSDATE)"
            : @"UPDATE TM_JOB_LOCATION SET LOC_NAME=:LOC_NAME, SORT_ORDER=:SORT_ORDER, ACTIVE_YN=:ACTIVE_YN,
                   UPDATE_USER_ID=:userId, UPDATE_DATE=SYSDATE WHERE LOC_CODE=:LOC_CODE";
        await c.ExecuteAsync(new CommandDefinition(sql,
            new { e.LOC_CODE, e.LOC_NAME, e.SORT_ORDER, e.ACTIVE_YN, userId }, cancellationToken: ct));
    }

    public async Task SetActiveAsync(string code, bool active, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_JOB_LOCATION SET ACTIVE_YN=:yn, UPDATE_USER_ID=:userId, UPDATE_DATE=SYSDATE WHERE LOC_CODE=:code",
            new { yn = active ? "Y" : "N", userId, code }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<CodeName>> GetActiveAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<CodeName>(new CommandDefinition(
            "SELECT LOC_CODE AS CODE, LOC_NAME AS NAME FROM TM_JOB_LOCATION WHERE NVL(ACTIVE_YN,'Y')='Y' ORDER BY SORT_ORDER NULLS LAST, LOC_NAME",
            cancellationToken: ct))).AsList();
    }
}

// ===================== Work Type =====================
public interface IWorkTypeRepository
{
    Task<IReadOnlyList<TM_WORK_TYPE>> GetAllAsync(CancellationToken ct = default);
    Task<TM_WORK_TYPE?> GetAsync(string code, CancellationToken ct = default);
    Task UpsertAsync(TM_WORK_TYPE e, decimal userId, bool isNew, CancellationToken ct = default);
    Task SetActiveAsync(string code, bool active, decimal userId, CancellationToken ct = default);
}

public sealed class WorkTypeRepository(IDbConnectionFactory db) : IWorkTypeRepository
{
    public async Task<IReadOnlyList<TM_WORK_TYPE>> GetAllAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<TM_WORK_TYPE>(new CommandDefinition(
            "SELECT WORK_TYPE_CODE, WORK_TYPE_NAME, CATEGORY, COST_TREATMENT, SUB_TYPE, ACTIVE_YN FROM TM_WORK_TYPE ORDER BY WORK_TYPE_NAME",
            cancellationToken: ct))).AsList();
    }

    public async Task<TM_WORK_TYPE?> GetAsync(string code, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<TM_WORK_TYPE>(new CommandDefinition(
            "SELECT WORK_TYPE_CODE, WORK_TYPE_NAME, CATEGORY, COST_TREATMENT, SUB_TYPE, ACTIVE_YN FROM TM_WORK_TYPE WHERE WORK_TYPE_CODE = :code",
            new { code }, cancellationToken: ct));
    }

    public async Task UpsertAsync(TM_WORK_TYPE e, decimal userId, bool isNew, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        var sql = isNew
            ? @"INSERT INTO TM_WORK_TYPE (WORK_TYPE_CODE, WORK_TYPE_NAME, CATEGORY, COST_TREATMENT, SUB_TYPE, ACTIVE_YN, CREATION_USER_ID, CREATION_DATE)
                VALUES (:WORK_TYPE_CODE, :WORK_TYPE_NAME, :CATEGORY, :COST_TREATMENT, :SUB_TYPE, :ACTIVE_YN, :userId, SYSDATE)"
            : @"UPDATE TM_WORK_TYPE SET WORK_TYPE_NAME=:WORK_TYPE_NAME, CATEGORY=:CATEGORY, COST_TREATMENT=:COST_TREATMENT,
                   SUB_TYPE=:SUB_TYPE, ACTIVE_YN=:ACTIVE_YN, UPDATE_USER_ID=:userId, UPDATE_DATE=SYSDATE
                WHERE WORK_TYPE_CODE=:WORK_TYPE_CODE";
        await c.ExecuteAsync(new CommandDefinition(sql,
            new { e.WORK_TYPE_CODE, e.WORK_TYPE_NAME, e.CATEGORY, e.COST_TREATMENT, e.SUB_TYPE, e.ACTIVE_YN, userId },
            cancellationToken: ct));
    }

    public async Task SetActiveAsync(string code, bool active, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_WORK_TYPE SET ACTIVE_YN=:yn, UPDATE_USER_ID=:userId, UPDATE_DATE=SYSDATE WHERE WORK_TYPE_CODE=:code",
            new { yn = active ? "Y" : "N", userId, code }, cancellationToken: ct));
    }
}

// ===================== Industry =====================
public interface IIndustryRepository
{
    Task<IReadOnlyList<TM_INDUSTRY>> GetAllAsync(CancellationToken ct = default);
    Task<TM_INDUSTRY?> GetAsync(string code, CancellationToken ct = default);
    Task UpsertAsync(TM_INDUSTRY e, decimal userId, bool isNew, CancellationToken ct = default);
    Task SetActiveAsync(string code, bool active, decimal userId, CancellationToken ct = default);
}

public sealed class IndustryRepository(IDbConnectionFactory db) : IIndustryRepository
{
    public async Task<IReadOnlyList<TM_INDUSTRY>> GetAllAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<TM_INDUSTRY>(new CommandDefinition(
            "SELECT INDUSTRY_CODE, INDUSTRY_NAME, ACTIVE_YN FROM TM_INDUSTRY ORDER BY INDUSTRY_NAME",
            cancellationToken: ct))).AsList();
    }

    public async Task<TM_INDUSTRY?> GetAsync(string code, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<TM_INDUSTRY>(new CommandDefinition(
            "SELECT INDUSTRY_CODE, INDUSTRY_NAME, ACTIVE_YN FROM TM_INDUSTRY WHERE INDUSTRY_CODE = :code",
            new { code }, cancellationToken: ct));
    }

    public async Task UpsertAsync(TM_INDUSTRY e, decimal userId, bool isNew, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        var sql = isNew
            ? @"INSERT INTO TM_INDUSTRY (INDUSTRY_CODE, INDUSTRY_NAME, ACTIVE_YN, CREATION_USER_ID, CREATION_DATE)
                VALUES (:INDUSTRY_CODE, :INDUSTRY_NAME, :ACTIVE_YN, :userId, SYSDATE)"
            : @"UPDATE TM_INDUSTRY SET INDUSTRY_NAME=:INDUSTRY_NAME, ACTIVE_YN=:ACTIVE_YN,
                   UPDATE_USER_ID=:userId, UPDATE_DATE=SYSDATE WHERE INDUSTRY_CODE=:INDUSTRY_CODE";
        await c.ExecuteAsync(new CommandDefinition(sql,
            new { e.INDUSTRY_CODE, e.INDUSTRY_NAME, e.ACTIVE_YN, userId }, cancellationToken: ct));
    }

    public async Task SetActiveAsync(string code, bool active, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_INDUSTRY SET ACTIVE_YN=:yn, UPDATE_USER_ID=:userId, UPDATE_DATE=SYSDATE WHERE INDUSTRY_CODE=:code",
            new { yn = active ? "Y" : "N", userId, code }, cancellationToken: ct));
    }
}

// ===================== Task =====================
public interface ITaskRepository
{
    Task<IReadOnlyList<TM_TASK>> GetAllAsync(CancellationToken ct = default);
    Task<TM_TASK?> GetAsync(string code, CancellationToken ct = default);
    Task UpsertAsync(TM_TASK e, decimal userId, bool isNew, CancellationToken ct = default);
    Task SetActiveAsync(string code, bool active, decimal userId, CancellationToken ct = default);
    Task<IReadOnlyList<CodeName>> GetActiveCodeNamesAsync(CancellationToken ct = default);
}

public sealed class TaskRepository(IDbConnectionFactory db) : ITaskRepository
{
    public async Task<IReadOnlyList<TM_TASK>> GetAllAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<TM_TASK>(new CommandDefinition(
            "SELECT TASK_CODE, TASK_NAME, TASK_GROUP, SEQ_NO, DEFAULT_SKILL, STD_HOURS, CHARGEABLE_YN, ACTIVE_YN FROM TM_TASK ORDER BY SEQ_NO NULLS LAST, TASK_NAME",
            cancellationToken: ct))).AsList();
    }

    public async Task<TM_TASK?> GetAsync(string code, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<TM_TASK>(new CommandDefinition(
            "SELECT TASK_CODE, TASK_NAME, TASK_GROUP, SEQ_NO, DEFAULT_SKILL, STD_HOURS, CHARGEABLE_YN, ACTIVE_YN FROM TM_TASK WHERE TASK_CODE = :code",
            new { code }, cancellationToken: ct));
    }

    public async Task UpsertAsync(TM_TASK e, decimal userId, bool isNew, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        var sql = isNew
            ? @"INSERT INTO TM_TASK (TASK_CODE, TASK_NAME, TASK_GROUP, SEQ_NO, DEFAULT_SKILL, STD_HOURS, CHARGEABLE_YN, ACTIVE_YN, CREATION_USER_ID, CREATION_DATE)
                VALUES (:TASK_CODE, :TASK_NAME, :TASK_GROUP, :SEQ_NO, :DEFAULT_SKILL, :STD_HOURS, :CHARGEABLE_YN, :ACTIVE_YN, :userId, SYSDATE)"
            : @"UPDATE TM_TASK SET TASK_NAME=:TASK_NAME, TASK_GROUP=:TASK_GROUP, SEQ_NO=:SEQ_NO, DEFAULT_SKILL=:DEFAULT_SKILL,
                   STD_HOURS=:STD_HOURS, CHARGEABLE_YN=:CHARGEABLE_YN, ACTIVE_YN=:ACTIVE_YN, UPDATE_USER_ID=:userId, UPDATE_DATE=SYSDATE
                WHERE TASK_CODE=:TASK_CODE";
        await c.ExecuteAsync(new CommandDefinition(sql,
            new { e.TASK_CODE, e.TASK_NAME, e.TASK_GROUP, e.SEQ_NO, e.DEFAULT_SKILL, e.STD_HOURS, e.CHARGEABLE_YN, e.ACTIVE_YN, userId },
            cancellationToken: ct));
    }

    public async Task SetActiveAsync(string code, bool active, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_TASK SET ACTIVE_YN=:yn, UPDATE_USER_ID=:userId, UPDATE_DATE=SYSDATE WHERE TASK_CODE=:code",
            new { yn = active ? "Y" : "N", userId, code }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<CodeName>> GetActiveCodeNamesAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<CodeName>(new CommandDefinition(
            "SELECT TASK_CODE AS CODE, TASK_NAME AS NAME FROM TM_TASK WHERE NVL(ACTIVE_YN,'Y')='Y' ORDER BY TASK_NAME",
            cancellationToken: ct))).AsList();
    }
}

// ===================== Labour Rate =====================
public interface ILabourRateRepository
{
    Task<IReadOnlyList<TM_LABOUR_RATE>> GetAllAsync(CancellationToken ct = default);
    Task<TM_LABOUR_RATE?> GetAsync(decimal id, CancellationToken ct = default);
    Task<decimal> InsertAsync(TM_LABOUR_RATE e, decimal userId, CancellationToken ct = default);
    Task UpdateAsync(TM_LABOUR_RATE e, decimal userId, CancellationToken ct = default);
    Task SetActiveAsync(decimal id, bool active, CancellationToken ct = default);
}

public sealed class LabourRateRepository(IDbConnectionFactory db) : ILabourRateRepository
{
    public async Task<IReadOnlyList<TM_LABOUR_RATE>> GetAllAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<TM_LABOUR_RATE>(new CommandDefinition(
            "SELECT LR_ID, LR_LOCATION, LR_INDUSTRY_CODE, LR_TIME_TYPE, LR_RATE, LR_COST_RATE, LR_OT_MULTIPLIER, LR_EFFECTIVE_FROM, LR_ACTIVE_YN FROM TM_LABOUR_RATE ORDER BY LR_LOCATION, LR_INDUSTRY_CODE",
            cancellationToken: ct))).AsList();
    }

    public async Task<TM_LABOUR_RATE?> GetAsync(decimal id, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<TM_LABOUR_RATE>(new CommandDefinition(
            "SELECT LR_ID, LR_LOCATION, LR_INDUSTRY_CODE, LR_TIME_TYPE, LR_RATE, LR_COST_RATE, LR_OT_MULTIPLIER, LR_EFFECTIVE_FROM, LR_ACTIVE_YN FROM TM_LABOUR_RATE WHERE LR_ID = :id",
            new { id }, cancellationToken: ct));
    }

    public async Task<decimal> InsertAsync(TM_LABOUR_RATE e, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        var id = await c.ExecuteScalarAsync<decimal>(new CommandDefinition(
            "SELECT TM_LABOUR_RATE_SEQ.NEXTVAL FROM DUAL", cancellationToken: ct));
        await c.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO TM_LABOUR_RATE (LR_ID, LR_LOCATION, LR_INDUSTRY_CODE, LR_TIME_TYPE, LR_RATE, LR_COST_RATE, LR_OT_MULTIPLIER, LR_EFFECTIVE_FROM, LR_ACTIVE_YN, LR_CREATION_USER_ID, LR_CREATION_DATE)
              VALUES (:id, :LR_LOCATION, :LR_INDUSTRY_CODE, :LR_TIME_TYPE, :LR_RATE, :LR_COST_RATE, :LR_OT_MULTIPLIER, :LR_EFFECTIVE_FROM, :LR_ACTIVE_YN, :userId, SYSDATE)",
            new { id, e.LR_LOCATION, e.LR_INDUSTRY_CODE, e.LR_TIME_TYPE, e.LR_RATE, e.LR_COST_RATE, e.LR_OT_MULTIPLIER, e.LR_EFFECTIVE_FROM, e.LR_ACTIVE_YN, userId },
            cancellationToken: ct));
        return id;
    }

    public async Task UpdateAsync(TM_LABOUR_RATE e, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            @"UPDATE TM_LABOUR_RATE SET LR_LOCATION=:LR_LOCATION, LR_INDUSTRY_CODE=:LR_INDUSTRY_CODE, LR_TIME_TYPE=:LR_TIME_TYPE,
                 LR_RATE=:LR_RATE, LR_COST_RATE=:LR_COST_RATE, LR_OT_MULTIPLIER=:LR_OT_MULTIPLIER, LR_EFFECTIVE_FROM=:LR_EFFECTIVE_FROM, LR_ACTIVE_YN=:LR_ACTIVE_YN
              WHERE LR_ID=:LR_ID",
            new { e.LR_LOCATION, e.LR_INDUSTRY_CODE, e.LR_TIME_TYPE, e.LR_RATE, e.LR_COST_RATE, e.LR_OT_MULTIPLIER, e.LR_EFFECTIVE_FROM, e.LR_ACTIVE_YN, e.LR_ID },
            cancellationToken: ct));
    }

    public async Task SetActiveAsync(decimal id, bool active, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_LABOUR_RATE SET LR_ACTIVE_YN=:yn WHERE LR_ID=:id",
            new { yn = active ? "Y" : "N", id }, cancellationToken: ct));
    }
}

// ===================== Duty Timing =====================
public interface IDutyTimingRepository
{
    Task<IReadOnlyList<TM_DUTY_TIMING>> GetAllAsync(CancellationToken ct = default);
    Task<TM_DUTY_TIMING?> GetAsync(decimal id, CancellationToken ct = default);
    Task<decimal> InsertAsync(TM_DUTY_TIMING e, decimal userId, CancellationToken ct = default);
    Task UpdateAsync(TM_DUTY_TIMING e, decimal userId, CancellationToken ct = default);
    Task SetActiveAsync(decimal id, bool active, CancellationToken ct = default);
}

public sealed class DutyTimingRepository(IDbConnectionFactory db) : IDutyTimingRepository
{
    public async Task<IReadOnlyList<TM_DUTY_TIMING>> GetAllAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<TM_DUTY_TIMING>(new CommandDefinition(
            "SELECT DT_ID, DT_NAME, DT_START, DT_END, DT_LUNCH_HOURS, DT_WEEKEND_DAYS, DT_MONTHLY_BENCHMARK, ACTIVE_YN FROM TM_DUTY_TIMING ORDER BY DT_ID",
            cancellationToken: ct))).AsList();
    }

    public async Task<TM_DUTY_TIMING?> GetAsync(decimal id, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<TM_DUTY_TIMING>(new CommandDefinition(
            "SELECT DT_ID, DT_NAME, DT_START, DT_END, DT_LUNCH_HOURS, DT_WEEKEND_DAYS, DT_MONTHLY_BENCHMARK, ACTIVE_YN FROM TM_DUTY_TIMING WHERE DT_ID = :id",
            new { id }, cancellationToken: ct));
    }

    public async Task<decimal> InsertAsync(TM_DUTY_TIMING e, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        var id = await c.ExecuteScalarAsync<decimal>(new CommandDefinition(
            "SELECT TM_DUTY_TIMING_SEQ.NEXTVAL FROM DUAL", cancellationToken: ct));
        await c.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO TM_DUTY_TIMING (DT_ID, DT_NAME, DT_START, DT_END, DT_LUNCH_HOURS, DT_WEEKEND_DAYS, DT_MONTHLY_BENCHMARK, ACTIVE_YN, CREATION_USER_ID, CREATION_DATE)
              VALUES (:id, :DT_NAME, :DT_START, :DT_END, :DT_LUNCH_HOURS, :DT_WEEKEND_DAYS, :DT_MONTHLY_BENCHMARK, :ACTIVE_YN, :userId, SYSDATE)",
            new { id, e.DT_NAME, e.DT_START, e.DT_END, e.DT_LUNCH_HOURS, e.DT_WEEKEND_DAYS, e.DT_MONTHLY_BENCHMARK, e.ACTIVE_YN, userId },
            cancellationToken: ct));
        return id;
    }

    public async Task UpdateAsync(TM_DUTY_TIMING e, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            @"UPDATE TM_DUTY_TIMING SET DT_NAME=:DT_NAME, DT_START=:DT_START, DT_END=:DT_END, DT_LUNCH_HOURS=:DT_LUNCH_HOURS,
                 DT_WEEKEND_DAYS=:DT_WEEKEND_DAYS, DT_MONTHLY_BENCHMARK=:DT_MONTHLY_BENCHMARK, ACTIVE_YN=:ACTIVE_YN
              WHERE DT_ID=:DT_ID",
            new { e.DT_NAME, e.DT_START, e.DT_END, e.DT_LUNCH_HOURS, e.DT_WEEKEND_DAYS, e.DT_MONTHLY_BENCHMARK, e.ACTIVE_YN, e.DT_ID },
            cancellationToken: ct));
    }

    public async Task SetActiveAsync(decimal id, bool active, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_DUTY_TIMING SET ACTIVE_YN=:yn WHERE DT_ID=:id",
            new { yn = active ? "Y" : "N", id }, cancellationToken: ct));
    }
}

// ===================== Customer -> Industry =====================
public interface ICustomerIndustryRepository
{
    Task<IReadOnlyList<TM_CUST_INDUSTRY_MAP>> GetAllAsync(CancellationToken ct = default);
    Task<TM_CUST_INDUSTRY_MAP?> GetAsync(decimal id, CancellationToken ct = default);
    Task<decimal> InsertAsync(TM_CUST_INDUSTRY_MAP e, decimal userId, CancellationToken ct = default);
    Task UpdateAsync(TM_CUST_INDUSTRY_MAP e, decimal userId, CancellationToken ct = default);
    Task SetActiveAsync(decimal id, bool active, CancellationToken ct = default);
    Task<IReadOnlyList<CodeName>> GetCustomersAsync(CancellationToken ct = default);
}

public sealed class CustomerIndustryRepository(IDbConnectionFactory db) : ICustomerIndustryRepository
{
    public async Task<IReadOnlyList<TM_CUST_INDUSTRY_MAP>> GetAllAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<TM_CUST_INDUSTRY_MAP>(new CommandDefinition(
            @"SELECT m.CIM_ID, m.CIM_CUSTOMER_CODE, m.CIM_INDUSTRY_CODE, m.CIM_DEFAULT_LOCATION, m.CIM_EFFECTIVE_FROM, m.CIM_ACTIVE_YN,
                     p.MPD_BRANCH_NAME AS CUSTOMER_NAME, i.INDUSTRY_NAME
              FROM   TM_CUST_INDUSTRY_MAP m
              LEFT JOIN MMM_PARTY_DETAILS p ON p.MPD_PARTY_CODE = m.CIM_CUSTOMER_CODE AND p.MPD_PARTY_TYPE = 'AR'
              LEFT JOIN TM_INDUSTRY i ON i.INDUSTRY_CODE = m.CIM_INDUSTRY_CODE
              ORDER BY m.CIM_CUSTOMER_CODE",
            cancellationToken: ct))).AsList();
    }

    public async Task<TM_CUST_INDUSTRY_MAP?> GetAsync(decimal id, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<TM_CUST_INDUSTRY_MAP>(new CommandDefinition(
            "SELECT CIM_ID, CIM_CUSTOMER_CODE, CIM_INDUSTRY_CODE, CIM_DEFAULT_LOCATION, CIM_EFFECTIVE_FROM, CIM_ACTIVE_YN FROM TM_CUST_INDUSTRY_MAP WHERE CIM_ID = :id",
            new { id }, cancellationToken: ct));
    }

    public async Task<decimal> InsertAsync(TM_CUST_INDUSTRY_MAP e, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        var id = await c.ExecuteScalarAsync<decimal>(new CommandDefinition(
            "SELECT TM_CUST_IND_SEQ.NEXTVAL FROM DUAL", cancellationToken: ct));
        await c.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO TM_CUST_INDUSTRY_MAP (CIM_ID, CIM_CUSTOMER_CODE, CIM_INDUSTRY_CODE, CIM_DEFAULT_LOCATION, CIM_EFFECTIVE_FROM, CIM_ACTIVE_YN, CIM_CREATION_USER_ID, CIM_CREATION_DATE)
              VALUES (:id, :CIM_CUSTOMER_CODE, :CIM_INDUSTRY_CODE, :CIM_DEFAULT_LOCATION, :CIM_EFFECTIVE_FROM, :CIM_ACTIVE_YN, :userId, SYSDATE)",
            new { id, e.CIM_CUSTOMER_CODE, e.CIM_INDUSTRY_CODE, e.CIM_DEFAULT_LOCATION, e.CIM_EFFECTIVE_FROM, e.CIM_ACTIVE_YN, userId },
            cancellationToken: ct));
        return id;
    }

    public async Task UpdateAsync(TM_CUST_INDUSTRY_MAP e, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            @"UPDATE TM_CUST_INDUSTRY_MAP SET CIM_CUSTOMER_CODE=:CIM_CUSTOMER_CODE, CIM_INDUSTRY_CODE=:CIM_INDUSTRY_CODE,
                 CIM_DEFAULT_LOCATION=:CIM_DEFAULT_LOCATION, CIM_EFFECTIVE_FROM=:CIM_EFFECTIVE_FROM, CIM_ACTIVE_YN=:CIM_ACTIVE_YN
              WHERE CIM_ID=:CIM_ID",
            new { e.CIM_CUSTOMER_CODE, e.CIM_INDUSTRY_CODE, e.CIM_DEFAULT_LOCATION, e.CIM_EFFECTIVE_FROM, e.CIM_ACTIVE_YN, e.CIM_ID },
            cancellationToken: ct));
    }

    public async Task SetActiveAsync(decimal id, bool active, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_CUST_INDUSTRY_MAP SET CIM_ACTIVE_YN=:yn WHERE CIM_ID=:id",
            new { yn = active ? "Y" : "N", id }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<CodeName>> GetCustomersAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<CodeName>(new CommandDefinition(
            @"SELECT MPD_PARTY_CODE AS CODE, MPD_BRANCH_NAME AS NAME FROM MMM_PARTY_DETAILS
              WHERE MPD_PARTY_TYPE = 'AR' AND MPD_PARTY_CODE IS NOT NULL
              ORDER BY MPD_BRANCH_NAME FETCH FIRST 500 ROWS ONLY",
            cancellationToken: ct))).AsList();
    }
}

// ===================== Task Standard Hours =====================
public interface ITaskStdRepository
{
    Task<IReadOnlyList<TM_TASK_STD>> GetAllAsync(CancellationToken ct = default);
    Task<TM_TASK_STD?> GetAsync(decimal id, CancellationToken ct = default);
    Task<decimal> InsertAsync(TM_TASK_STD e, decimal userId, CancellationToken ct = default);
    Task UpdateAsync(TM_TASK_STD e, decimal userId, CancellationToken ct = default);
    Task SetActiveAsync(decimal id, bool active, CancellationToken ct = default);
}

public sealed class TaskStdRepository(IDbConnectionFactory db) : ITaskStdRepository
{
    public async Task<IReadOnlyList<TM_TASK_STD>> GetAllAsync(CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return (await c.QueryAsync<TM_TASK_STD>(new CommandDefinition(
            @"SELECT s.TST_ID, s.TST_MANUFACTURER, s.TST_MODEL, s.TST_SERIES, s.TST_COMPONENT, s.TST_TASK_CODE,
                     s.TST_STD_HOURS, s.TST_ACCEPT_BENCHMARK, s.ACTIVE_YN, t.TASK_NAME
              FROM   TM_TASK_STD s LEFT JOIN TM_TASK t ON t.TASK_CODE = s.TST_TASK_CODE
              ORDER BY s.TST_MANUFACTURER, s.TST_MODEL, s.TST_TASK_CODE",
            cancellationToken: ct))).AsList();
    }

    public async Task<TM_TASK_STD?> GetAsync(decimal id, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        return await c.QueryFirstOrDefaultAsync<TM_TASK_STD>(new CommandDefinition(
            "SELECT TST_ID, TST_MANUFACTURER, TST_MODEL, TST_SERIES, TST_COMPONENT, TST_TASK_CODE, TST_STD_HOURS, TST_ACCEPT_BENCHMARK, ACTIVE_YN FROM TM_TASK_STD WHERE TST_ID = :id",
            new { id }, cancellationToken: ct));
    }

    public async Task<decimal> InsertAsync(TM_TASK_STD e, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        var id = await c.ExecuteScalarAsync<decimal>(new CommandDefinition(
            "SELECT TM_TASK_STD_SEQ.NEXTVAL FROM DUAL", cancellationToken: ct));
        await c.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO TM_TASK_STD (TST_ID, TST_MANUFACTURER, TST_MODEL, TST_SERIES, TST_COMPONENT, TST_TASK_CODE, TST_STD_HOURS, TST_ACCEPT_BENCHMARK, ACTIVE_YN, CREATION_USER_ID, CREATION_DATE)
              VALUES (:id, :TST_MANUFACTURER, :TST_MODEL, :TST_SERIES, :TST_COMPONENT, :TST_TASK_CODE, :TST_STD_HOURS, :TST_ACCEPT_BENCHMARK, :ACTIVE_YN, :userId, SYSDATE)",
            new { id, e.TST_MANUFACTURER, e.TST_MODEL, e.TST_SERIES, e.TST_COMPONENT, e.TST_TASK_CODE, e.TST_STD_HOURS, e.TST_ACCEPT_BENCHMARK, e.ACTIVE_YN, userId },
            cancellationToken: ct));
        return id;
    }

    public async Task UpdateAsync(TM_TASK_STD e, decimal userId, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            @"UPDATE TM_TASK_STD SET TST_MANUFACTURER=:TST_MANUFACTURER, TST_MODEL=:TST_MODEL, TST_SERIES=:TST_SERIES,
                 TST_COMPONENT=:TST_COMPONENT, TST_TASK_CODE=:TST_TASK_CODE, TST_STD_HOURS=:TST_STD_HOURS,
                 TST_ACCEPT_BENCHMARK=:TST_ACCEPT_BENCHMARK, ACTIVE_YN=:ACTIVE_YN
              WHERE TST_ID=:TST_ID",
            new { e.TST_MANUFACTURER, e.TST_MODEL, e.TST_SERIES, e.TST_COMPONENT, e.TST_TASK_CODE, e.TST_STD_HOURS, e.TST_ACCEPT_BENCHMARK, e.ACTIVE_YN, e.TST_ID },
            cancellationToken: ct));
    }

    public async Task SetActiveAsync(decimal id, bool active, CancellationToken ct = default)
    {
        using var c = await db.CreateOpenConnectionAsync(ct);
        await c.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_TASK_STD SET ACTIVE_YN=:yn WHERE TST_ID=:id",
            new { yn = active ? "Y" : "N", id }, cancellationToken: ct));
    }
}
