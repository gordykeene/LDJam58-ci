using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Defines a placeable exhibit tile type as a ScriptableObject so designers can author content.
    /// </summary>
    [CreateAssetMenu(fileName = "ExhibitTileType", menuName = "Exhibit", order = -1)]
    public class ExhibitTileType : ScriptableObject
    {
        

        [Header("Identity")]
        [SerializeField] private string _displayName = "New Exhibit";
        [SerializeField] private List<ExhibitTag> _tags = new List<ExhibitTag>();

        [AssetsOnly]
        [SerializeField] private GameObject exhibitPrefab;
        [AssetsOnly]
        [SerializeField] private Sprite exhibitSprite; 
        
        [Header("Footprint (grid units)")]
        [SerializeField] private Vector2Int _size = new Vector2Int(1, 1);
        
        
        [Header("Meta")]
        [SerializeField] private ExhibitRarity _rarity = ExhibitRarity.Common;
        [SerializeField, Range(0, 100)] private int _enjoyment = 50;
        [SerializeField, Range(0, 100)] private int _popularity = 50;
        
        public string DisplayName => _displayName;
        public IReadOnlyList<ExhibitTag> Tags => _tags;
        public Vector2Int Size => _size;
        public ExhibitRarity Rarity => _rarity;
        public int Enjoyment => _enjoyment;
        public int Popularity => _popularity;

        public GameObject ExhibitPrefab => exhibitPrefab;
        public Sprite ExhibitSprite => exhibitSprite;
        
        private void OnValidate()
        {
            // Throw error if x or y is out of [1,5] range
            if (_size.x < 1 || _size.x > 5 || _size.y < 1 || _size.y > 5)
            {
                throw new System.ArgumentOutOfRangeException(
                    "_size",
                    $"ExhibitTileType size must be between 1 and 5 for both x and y. Got: ({_size.x}, {_size.y})"
                );
            }

            // Clamp to sensible bounds and avoid zero/negative sizes
            var w = Mathf.Max(1, _size.x);
            var h = Mathf.Max(1, _size.y);
            _size = new Vector2Int(w, h);
            _enjoyment = Mathf.Clamp(_enjoyment, 0, 100);
            _popularity = Mathf.Clamp(_popularity, 0, 100);
        }

        protected bool Equals(ExhibitTileType other)
        {
            return base.Equals(other) && _displayName == other._displayName;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ExhibitTileType)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), _displayName);
        }
    }
}
