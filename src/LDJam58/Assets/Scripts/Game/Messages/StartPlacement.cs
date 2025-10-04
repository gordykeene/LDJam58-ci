using Assets.Scripts;

namespace Game.Messages
{
    public class StartPlacement
    {
        public ExhibitTileType exhibit;

        public StartPlacement(ExhibitTileType exhibit)
        {
            this.exhibit = exhibit;
        }
    }
}