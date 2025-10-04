using Assets.Scripts;

public class AdvancePeriod
{

}

public class GameWon
{

}

public class GameLost
{

}

public class ExhibitPicked
{
    public ExhibitTileType Exhibit;

    public ExhibitPicked(ExhibitTileType exhibit)
    {
        Exhibit = exhibit;
    }
}

public class PeriodInitiatized 
{
    public ProgressionPeriodConfig Period;

    public PeriodInitiatized(ProgressionPeriodConfig period)
    {
        Period = period;
    }
}

public class LockCameraMovement
{

}

public class UnlockCameraMovement
{

}
