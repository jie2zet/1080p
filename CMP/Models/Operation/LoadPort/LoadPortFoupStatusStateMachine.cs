using CMP.Services.LoadPort;

namespace CMP.Models.Operation.LoadPort;

/// <summary>
/// 由 LoadPort 硬件状态（STATE 解析后的 <see cref="LoadPortStatus"/>）推导
/// UI/流程用 <see cref="LoadportFoupStatus"/>。仅依赖本次硬件快照；不保留状态机内部记忆，
///  hysteresis 由调用方传入的 <paramref name="previous"/> 参与决策（如 Error 时保持上一状态）。
/// </summary>
/// <remarks>
/// <see cref="LoadportFoupStatus.WaitUnload"/>、<see cref="LoadportFoupStatus.WaitRemove"/> 等需结合上位指令或配方的事件，
/// 当前硬件推导无法区分时保留 <paramref name="previous"/>，后续可通过 <c>ApplyHostEvent</c> 类扩展接入。
/// </remarks>
internal static class LoadPortFoupStatusStateMachine
{
    /// <param name="hw">当前一帧解析得到的 LoadPort 状态，不可为 null。</param>
    /// <param name="previous">上一流程状态。</param>
    /// <returns>下一流程状态、是否刚进入 <see cref="LoadportFoupStatus.Unload"/>（用于联动 PLC 等副作用）。</returns>
    public static (LoadportFoupStatus Next, bool EnteredUnload) Transition(LoadPortStatus hw, LoadportFoupStatus previous)
    {
        var next = previous;

        switch (hw.foupPlacementStatus)
        {
            case FoupPlacementStatus.NoExistence:
                next = LoadportFoupStatus.WaitPlace;
                break;

            case FoupPlacementStatus.Error:
                // 无更细错误子状态时不乱跳，避免 UI 抖动
                break;

            case FoupPlacementStatus.Normal:
                next = TransitionNormalFoup(hw, previous);
                break;
        }

        var enteredUnload = previous != LoadportFoupStatus.Unload && next == LoadportFoupStatus.Unload;
        return (next, enteredUnload);
    }

    private static LoadportFoupStatus TransitionNormalFoup(LoadPortStatus hw, LoadportFoupStatus previous)
    {
        switch (hw.FoupDockStatus)
        {
            case FoupDockStatus.Undocking:
                return LoadportFoupStatus.Unload;

            case FoupDockStatus.Docking:
                // 上位已请求卸料、等待机构进入 Undocking 前保持 WaitUnload
                if (previous == LoadportFoupStatus.WaitUnload)
                    return LoadportFoupStatus.WaitUnload;
                // 门未开：等待装载/开门后再作业
                if (hw.FIMSDoorStatus == FIMSDoorStatus.Close)
                    return LoadportFoupStatus.WaitLoad;
                return LoadportFoupStatus.Load;

            case FoupDockStatus.Unknown:
            default:
                // FOUP 在位但对接姿态未知：视为已放置、尚未明确 Dock/Undock
                return LoadportFoupStatus.Present;
        }
    }
}
