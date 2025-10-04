using UnityEngine;

public class GameManager : OnMessage<AdvancePeriod>
{
    [SerializeField] private ProgressionConfig _progressionConfig;

    private void Start()
    {
        InitStateForCurrentPeriod();
    }

    private void InitStateForCurrentPeriod()
    {
        var currentPeriod = GetCurrentPeriod();
        CurrentGameState.UpdateState(gs => {
            gs.currentAppeal = currentPeriod.TargetAppeal;
            gs.currentNumVisitingGroups = currentPeriod.NumVisitingGroups;
            return gs;
        });
    }

    private ProgressionPeriodConfig GetCurrentPeriod()
    {
        return _progressionConfig.GetPeriod(CurrentGameState.ReadOnly.currentPeriodIndex);
    }

    protected override void Execute(AdvancePeriod msg)
    {
        if (CurrentGameState.ReadOnly.currentPeriodIndex + 1 >= _progressionConfig.Count)
        {
            Message.Publish(new GameWon());
            return;
        } 

        CurrentGameState.UpdateState(gs => {
            gs.currentPeriodIndex++;
            return gs;
        });

        InitStateForCurrentPeriod();
    }
}
