using YesTm.Web.Features.JobCostPosting;

namespace YesTm.Tests;

public class JobCostCalculatorTests
{
    // Yellow team rates: workshop 10, field 12, travel 8 per hour, 0.200 per km.
    // Dr accounts 501..504, all credit the same absorbed account 900 except km (901).
    private static readonly IReadOnlyList<TM_JCP_COST_CODE> Codes =
    [
        new() { JCC_ID = 1, JCC_TYPE = CostTypes.Labour, JCC_LOCATION = "Workshop", JCC_DESC = "Workshop labour", JCC_RATE = 10m, JCC_DR_VEL_CODE = 501, JCC_CR_VEL_CODE = 900 },
        new() { JCC_ID = 2, JCC_TYPE = CostTypes.Labour, JCC_LOCATION = "Field", JCC_DESC = "Field service labour", JCC_RATE = 12m, JCC_DR_VEL_CODE = 502, JCC_CR_VEL_CODE = 900 },
        new() { JCC_ID = 3, JCC_TYPE = CostTypes.TravelHours, JCC_DESC = "Travel time", JCC_RATE = 8m, JCC_DR_VEL_CODE = 503, JCC_CR_VEL_CODE = 900 },
        new() { JCC_ID = 4, JCC_TYPE = CostTypes.TravelKm, JCC_DESC = "Vehicle km", JCC_RATE = 0.2m, JCC_DR_VEL_CODE = 504, JCC_CR_VEL_CODE = 901 },
    ];

    private static CostLineInput Work(string tech, int day, string loc, decimal net, decimal idle = 0, decimal travel = 0, int month = 10) =>
        new(tech, tech + " name", new DateTime(2026, month, day), loc, "WORK", net, idle, travel);

    private static CostLineInput Travel(string tech, int day, decimal net, int month = 10) =>
        new(tech, tech + " name", new DateTime(2026, month, day), "Field", "TRAVEL", net, 0, net);

    [Fact]
    public void WorkedExample_FromTheDesignDoc_BalancesAndTagsTheJob()
    {
        var r = JobCostCalculator.Calculate("TS-1", "JCD-0012", "Field",
            [Work("A", 5, "Workshop", 6m), Work("B", 6, "Field", 4m), Travel("B", 6, 2m)],
            new CostKmInput(80m, new DateTime(2026, 10, 6), "FSR-9", "1234 AB"), Codes, null);

        var jv = Assert.Single(r.Vouchers);
        Assert.Equal(140m, jv.Total);                                      // 60 + 48 + 16 + 16
        Assert.Equal(0m, jv.Lines.Sum(l => l.Amount));                     // balanced
        Assert.All(jv.Lines.Where(l => l.Amount > 0), l => Assert.Equal("JCD-0012", l.JobCode));
        Assert.All(jv.Lines.Where(l => l.Amount < 0), l => Assert.Null(l.JobCode));   // absorbed lines never hit the job
        Assert.Equal(-124m, jv.Lines.Single(l => l.VelCode == 900).Amount);
        Assert.Equal(-16m, jv.Lines.Single(l => l.VelCode == 901).Amount);
        Assert.Equal(new DateTime(2026, 10, 6), jv.JvDate);
        Assert.Empty(r.Warnings);
    }

    [Fact]
    public void OneDebitLinePerEmployeeAndCostCode()
    {
        var r = JobCostCalculator.Calculate("TS-1", "J1", "Workshop",
            [Work("A", 1, "Workshop", 3m), Work("A", 2, "Workshop", 5m), Work("B", 2, "Workshop", 2m)], null, Codes, null);

        var dr = r.Vouchers.Single().Lines.Where(l => l.Amount > 0).ToList();
        Assert.Equal(2, dr.Count);
        Assert.Equal(80m, dr[0].Amount);   // A: 8 h × 10
        Assert.Equal(20m, dr[1].Amount);   // B: 2 h × 10
        Assert.Contains("A A name", dr[0].Narration);
    }

    [Fact]
    public void IdleIsNotCharged_OvertimeUsesFlatRate_TravelOnWorkLineIsTravel()
    {
        // 9 net (incl. OT) − 1 idle = 8 h × 12; 1.5 h travel recorded on the work line × 8.
        var r = JobCostCalculator.Calculate("TS-1", "J1", "Field", [Work("A", 3, "Field", 9m, idle: 1m, travel: 1.5m)], null, Codes, null);
        var items = r.Vouchers.Single().Items;
        Assert.Equal(96m, items.Single(i => i.CostDesc == "Field service labour").Amount);
        Assert.Equal(12m, items.Single(i => i.CostDesc == "Travel time").Amount);
    }

    [Fact]
    public void LinesInTwoMonths_MakeTwoVouchers()
    {
        var r = JobCostCalculator.Calculate("TS-1", "J1", "Workshop",
            [Work("A", 30, "Workshop", 4m, month: 10), Work("A", 2, "Workshop", 4m, month: 11)], null, Codes, null);
        Assert.Equal(2, r.Vouchers.Count);
        Assert.Equal(new DateTime(2026, 10, 30), r.Vouchers[0].JvDate);
        Assert.Equal(new DateTime(2026, 11, 2), r.Vouchers[1].JvDate);
    }

    [Fact]
    public void BeforeCutOver_IsNotPosted()
    {
        var r = JobCostCalculator.Calculate("TS-1", "J1", "Workshop",
            [Work("A", 15, "Workshop", 4m), Work("A", 20, "Workshop", 4m)], null, Codes, new DateTime(2026, 10, 18));
        Assert.Equal(40m, r.Vouchers.Single().Total);
    }

    [Fact]
    public void LocationWithoutRate_IsReportedNotPosted()
    {
        var r = JobCostCalculator.Calculate("TS-1", "J1", "Service", [Work("A", 5, "Service", 3m)], null, Codes, null);
        Assert.Empty(r.Vouchers);
        Assert.Contains(r.Warnings, w => w.Contains("Service labour") && w.Contains("no Job Cost rate"));
    }

    [Fact]
    public void RateWithoutAccount_IsReportedNotPosted()
    {
        var codes = Codes.Select(c => c.JCC_ID == 1 ? new TM_JCP_COST_CODE
        {
            JCC_ID = 1, JCC_TYPE = c.JCC_TYPE, JCC_LOCATION = c.JCC_LOCATION, JCC_DESC = c.JCC_DESC, JCC_RATE = c.JCC_RATE
        } : c).ToList();
        var r = JobCostCalculator.Calculate("TS-1", "J1", "Workshop", [Work("A", 5, "Workshop", 3m)], null, codes, null);
        Assert.Empty(r.Vouchers);
        Assert.Contains(r.Warnings, w => w.Contains("no GL account"));
    }

    [Fact]
    public void LaterEffectiveRate_AppliesFromItsDate()
    {
        var codes = Codes.Append(new TM_JCP_COST_CODE
        {
            JCC_ID = 9, JCC_TYPE = CostTypes.Labour, JCC_LOCATION = "Workshop", JCC_DESC = "Workshop labour", JCC_RATE = 11m,
            JCC_DR_VEL_CODE = 501, JCC_CR_VEL_CODE = 900, JCC_EFFECTIVE_FROM = new DateTime(2026, 10, 15)
        }).ToList();
        var r = JobCostCalculator.Calculate("TS-1", "J1", "Workshop",
            [Work("A", 10, "Workshop", 1m), Work("A", 20, "Workshop", 1m)], null, codes, null);
        Assert.Equal(21m, r.Vouchers.Single().Total);   // 1 × 10 before, 1 × 11 after
    }
}
