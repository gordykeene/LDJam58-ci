using UnityEngine;

namespace Assets.Scripts.Game.Progression
{
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "ProgressionConfig")]
    public class ProgressionConfig : ScriptableObject
    {
        [SerializeField] private ProgressionPeriodConfig[] _periods = new ProgressionPeriodConfig[0];

        public ProgressionPeriodConfig[] Periods => _periods;

        public int Count => _periods?.Length ?? 0;

        public ProgressionPeriodConfig GetPeriod(int index)
        {
            if (_periods == null || index < 0 || index >= _periods.Length) return null;
            return _periods[index];
        }
    }
}


