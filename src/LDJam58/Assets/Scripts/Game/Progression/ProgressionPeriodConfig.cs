using UnityEngine;

namespace Assets.Scripts.Game.Progression
{
    [CreateAssetMenu(fileName = "ProgressionPeriodConfig", menuName = "ProgressionPeriodConfig")]
    public class ProgressionPeriodConfig : ScriptableObject
    {
        [SerializeField] private int _targetAppeal = 0;
        [SerializeField] private int _numVisitingGroups = 0;

        public int TargetAppeal => _targetAppeal;
        public int NumVisitingGroups => _numVisitingGroups;
    }
}
