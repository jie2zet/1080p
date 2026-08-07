namespace CMP.Services.Tracking;

public static class WaferChamberVisitBuilder
{
    public static IReadOnlyList<WaferChamberVisitRecord> Build(
        IReadOnlyList<DateTime?> stepTimes,
        IReadOnlyDictionary<string, string> recipeMap,
        IReadOnlySet<string>? plannedChambers = null)
    {
        if (stepTimes.Count == 0)
        {
            return [];
        }

        var visits = new List<WaferChamberVisitRecord>(ChamberStepMappingProvider.GetMappings().Count);

        foreach (var mapping in ChamberStepMappingProvider.GetMappings())
        {
            var recipeName = recipeMap.TryGetValue(mapping.ChamberName, out var recipe) ? recipe : string.Empty;
            var planned = plannedChambers?.Contains(mapping.ChamberName) == true;

            var enterTime = GetFirstStepTimeInRange(stepTimes, mapping.EnterStartStep, mapping.ExitEndStep);
            var exitTime = GetLastStepTimeInRange(stepTimes, mapping.EnterStartStep, mapping.ExitEndStep);
            var processStart = mapping.ProcessStartStep > 0
                ? GetFirstStepTimeInRange(stepTimes, mapping.ProcessStartStep, mapping.ProcessEndStep)
                : null;
            var processEnd = mapping.ProcessEndStep > 0
                ? GetLastStepTimeInRange(stepTimes, mapping.ProcessStartStep, mapping.ProcessEndStep)
                : null;

            var status = ResolveStatus(enterTime, exitTime);

            if (!planned && !enterTime.HasValue && !exitTime.HasValue)
            {
                continue;
            }

            visits.Add(new WaferChamberVisitRecord
            {
                ChamberName = mapping.ChamberName,
                RecipeName = recipeName,
                Planned = planned,
                EnterTime = enterTime,
                ExitTime = exitTime,
                ProcessStartTime = processStart,
                ProcessEndTime = processEnd,
                Status = status
            });
        }

        return visits;
    }

    private static DateTime? GetFirstStepTimeInRange(IReadOnlyList<DateTime?> stepTimes, int startStep, int endStep)
    {
        if (startStep <= 0 || endStep <= 0 || startStep > endStep)
        {
            return null;
        }

        var start = Math.Max(startStep, 1);
        var end = Math.Min(endStep, stepTimes.Count);
        for (var step = start; step <= end; step++)
        {
            var time = stepTimes[step - 1];
            if (time.HasValue)
            {
                return time;
            }
        }

        return null;
    }

    private static DateTime? GetLastStepTimeInRange(IReadOnlyList<DateTime?> stepTimes, int startStep, int endStep)
    {
        if (startStep <= 0 || endStep <= 0 || startStep > endStep)
        {
            return null;
        }

        var start = Math.Max(startStep, 1);
        var end = Math.Min(endStep, stepTimes.Count);
        for (var step = end; step >= start; step--)
        {
            var time = stepTimes[step - 1];
            if (time.HasValue)
            {
                return time;
            }
        }

        return null;
    }

    private static ChamberVisitStatus ResolveStatus(DateTime? enterTime, DateTime? exitTime)
    {
        if (enterTime.HasValue && exitTime.HasValue)
        {
            return ChamberVisitStatus.Completed;
        }

        if (enterTime.HasValue || exitTime.HasValue)
        {
            return ChamberVisitStatus.Partial;
        }

        return ChamberVisitStatus.Skipped;
    }
}
