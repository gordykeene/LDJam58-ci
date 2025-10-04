using UnityEngine;

namespace Game.Messages
{
    public class ExhibitPlaced
    {
        public GameObject exhibitInstance;

        public ExhibitPlaced(GameObject exhibitInstance)
        {
            this.exhibitInstance = exhibitInstance;
        }
    }
}