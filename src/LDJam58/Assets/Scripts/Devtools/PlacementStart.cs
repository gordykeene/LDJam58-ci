using Game.Messages;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Devtools
{
    public class PlacementStart : MonoBehaviour
    {
        [Button]
        private void StartPlacing()
        {
            Message.Publish(new StartExhibitPick());   
        }
    }
}
