using YesTm.Web.Common.TimeCalc;
using YesTm.Web.Features.Masters;

namespace YesTm.Web.Features.TimeBooking;

/// <summary>How a non-job activity changes the calculated figures of a line.</summary>
public static class NonJobRules
{
    /// <summary>
    /// Non-job time is measured in hours only — it is not job cost, so rates, labour cost and food are 0.
    /// Absence (holiday / leave / week off) is plain time: every net hour is Normal, never OT, even on a
    /// weekend. Idle / non-productive lines keep the duty-window Normal/OT split (time spent at work).
    /// </summary>
    public static TimeLineCalcResult Apply(TimeLineCalcResult c, TM_NONJOB_ACTIVITY activity)
    {
        if (NonJobCategories.IsAbsence(activity.NJ_CATEGORY))
            c = c with
            {
                NormalHours = c.NetHours,
                OtHours = 0m,
                TimeType = TimeType.Normal
            };
        return c with { NormalRate = 0m, OtRate = 0m, LabourCost = 0m };
    }
}
