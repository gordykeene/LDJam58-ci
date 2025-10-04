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

public class BeginPickThree 
{
    public ExhibitTileType[] Exhibits;

    public BeginPickThree(ExhibitTileType[] exhibits)
    {
        Exhibits = exhibits;
    }
}

public class ExhibitPicked
{
    public ExhibitTileType Exhibit;

    public ExhibitPicked(ExhibitTileType exhibit)
    {
        Exhibit = exhibit;
    }
}
