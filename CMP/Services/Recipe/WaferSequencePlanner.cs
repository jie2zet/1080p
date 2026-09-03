using CMP.Models.Recipe.Sequence;
using CMP.Models.Resource;

namespace CMP.Services.Recipe;

internal sealed record ChamberOperation(
    string ChamberName,
    Func<SequenceRecipeData, SequenceType> TypeSelector,
    Func<SequenceRecipeData, bool> CheckedSelector
);

internal sealed record ChamberGroup(
    Func<SequenceRecipeData, SequenceType> MainTypeSelector,
    Func<SequenceRecipeData, bool> ASideEnabledSelector,
    Func<SequenceRecipeData, bool> BSideEnabledSelector,
    ChamberOperation[] ASideOperations,
    ChamberOperation[] BSideOperations
);

/// <summary>
/// Sequence配置驱动引擎，根据元数据注册表计算Wafer的Chamber执行步骤
/// 新增Chamber只需在ChamberGroups注册表中添加一行记录，无需修改算法
/// </summary>
public static class WaferSequencePlanner
{
    private static readonly ChamberGroup[] ChamberGroups =
    [
        new ChamberGroup(
            data => data.PolishMainType,
            data => data.PolishMainASideChecked,
            data => data.PolishMainBSideChecked,
            [
                new("PolishB",data => SequenceType.None,data => data.PolishASideOperationBChecked),
                new("PolishA",data => data.PolishASideOperationType,data => data.PolishASideOperationAChecked),
            ],
            [
                new("PolishD",data => SequenceType.None,data => data.PolishBSideOperationDChecked),
                new("PolishC",data => data.PolishBSideOperationType,data => data.PolishBSideOperationCChecked),
            ]
        ),
        new ChamberGroup(
            data => data.CleanMainType,
            data => data.CleanMainASideChecked,
            data => data.CleanMainBSideChecked,
            [
                new("RollerA1", data => data.CleanASideRollerOp1Type, data => data.CleanASideRollerOp1Checked),
                new("RollerA2", data => data.CleanASideRollerOp1Type, data => data.CleanASideRollerOp2Checked),
                new("RollerA3", data => data.CleanASideRollerOp2Type, data => data.CleanASideRollerOp3Checked),
            ],
            [
                new("RollerB1", data => data.CleanBSideRollerOp1Type, data => data.CleanBSideRollerOp1Checked),
                new("RollerB2", data => data.CleanBSideRollerOp1Type, data => data.CleanBSideRollerOp2Checked),
                new("RollerB3", data => data.CleanBSideRollerOp2Type, data => data.CleanBSideRollerOp3Checked),
            ]
        ),
        new ChamberGroup(
            data => data.CleanMainType,
            data => data.CleanMainASideChecked,
            data => data.CleanMainBSideChecked,
            [
                new("PenA",data => SequenceType.None, data => data.CleanASidePenOp1Checked),
            ],
            [
                new("PenB", data => SequenceType.None, data => data.CleanBSidePenOp1Checked),
            ]
        ),
        new ChamberGroup(
            data => data.CleanMainType,
            data => data.CleanMainASideChecked,
            data => data.CleanMainBSideChecked,
            [
                new("DryA", data => SequenceType.None, data => data.CleanASideDryOpChecked)
            ],
            [
                new("DryB", data => SequenceType.None, data => data.CleanBSideDryOpChecked)
            ]
        ),
    ];

    public static WaferSequencePlan BuildPlan(SequenceRecipeData data)
    {
        var plan = new WaferSequencePlan
        {
            WaferSequenceModels = []
        };

        ProcessSequence(plan.WaferSequenceModels, data);
        ComputeSequnceValues(plan);
        return plan;
    }

    private static void ProcessSequence(List<WaferSequenceModel> models, SequenceRecipeData data)
    {
        var currentMaxStep = -1;
        var isNext = false;
        foreach (var group in ChamberGroups)
        {
            var mainType = group.MainTypeSelector(data);
            var baseStep = currentMaxStep + 1;
            isNext = true;
            if (mainType == SequenceType.Parallel)
            {
                if (group.ASideEnabledSelector(data))
                    ProcessSideOps(models, data, group.ASideOperations, baseStep, ref currentMaxStep, isNext);
                if (group.BSideEnabledSelector(data))
                    ProcessSideOps(models, data, group.BSideOperations, baseStep, ref currentMaxStep, isNext);
            }
            else if (mainType == SequenceType.Serial)
            {
                if (group.ASideEnabledSelector(data))
                    ProcessSideOps(models, data, group.ASideOperations, baseStep, ref currentMaxStep, isNext);
                if (group.BSideEnabledSelector(data))
                {
                    baseStep = currentMaxStep + 1;
                    isNext = true;
                    ProcessSideOps(models, data, group.BSideOperations, baseStep, ref currentMaxStep, isNext);
                }
            }
        }
    }

    private static void ProcessSideOps(List<WaferSequenceModel> models, SequenceRecipeData data, ChamberOperation[] ops, int baseStep, ref int currentMaxStep, bool isNext)
    {
        var nextOpIndex = 0;
        while (nextOpIndex < ops.Length)
        {
            var op = ops[nextOpIndex];
            switch (op.TypeSelector(data))
            {
                case SequenceType.Serial:
                    if (op.CheckedSelector(data))
                    {
                        if (!isNext)
                        {
                            baseStep++;
                        }

                        isNext = false;
                        models.Add(new WaferSequenceModel { ChamberName = op.ChamberName, ChamberSequnceStep = baseStep });
                        currentMaxStep = Math.Max(baseStep, currentMaxStep);
                    }

                    break;
                case SequenceType.Parallel:
                case SequenceType.None:
                    if (op.CheckedSelector(data))
                    {
                        isNext = false;
                        models.Add(new WaferSequenceModel { ChamberName = op.ChamberName, ChamberSequnceStep = baseStep });
                        currentMaxStep = Math.Max(baseStep, currentMaxStep);
                    }

                    break;
            }

            nextOpIndex++;
        }
    }

    private static void ComputeSequnceValues(WaferSequencePlan plan)
    {
        for (var i = 0; i < plan.SequnceValue.Length; i++)
        {
            var list = plan.WaferSequenceModels.Where(x => x.ChamberSequnceStep == i).ToList();
            if (list.Count == 0)
            {
                plan.SequnceValue[i] = 0;
                continue;
            }

            var val = 0;
            foreach (var item in list)
            {
                val += item.ChamberStepValue;
            }

            plan.SequnceValue[i] = val;
        }
    }
}
