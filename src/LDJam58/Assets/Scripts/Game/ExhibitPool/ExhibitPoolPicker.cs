using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Game.Messages;
using UnityEngine;

namespace Game.ExhibitPool
{
    
    public class ExhibitPoolPicker : OnMessage<StartExhibitPick>
    {
        [SerializeField] private int outputSize;
        [SerializeField] private ExhibitPoolObject exhibitPoolObject;

        private List<ExhibitTileType> exhibitPool;

        [SerializeField]
        private float rareChance;
        [SerializeField]
        private float exoticChance;
        [SerializeField]
        private float mythicChance;
        
        private HashSet<ExhibitTag> viableTags;
        
        private void Start()
        {
            exhibitPool = new List<ExhibitTileType>();
            viableTags = new HashSet<ExhibitTag>();
            foreach (var exhibit in exhibitPoolObject.Exhibits)
            {
                exhibitPool.Add(exhibit);
            }
            //RecalculateViableTags();
        }

        private void RecalculateViableTags(List<ExhibitTileType> pool)
        {
            viableTags.Clear();
            foreach (var exhibit in pool)
            {
                foreach (var tag in exhibit.Tags)
                {
                    viableTags.Add(tag);        
                }
            }
        }

        public List<ExhibitTileType> PickRandomExhibits()
        {
            var list = new List<ExhibitTileType>();
            for (int i = 0; i < outputSize; i++)
            {
                var randomTile = GetRandomExhibitTile();
                list.Add(randomTile);
                RemoveFromPool(randomTile);
            }

            foreach (var exhibit in list)
            {
                ReturnToPool(exhibit);
            }
            
            return list;
        }

        private void ReturnToPool(ExhibitTileType exhibit)
        {
            exhibitPool.Add(exhibit);
        }
        
        public void RemoveFromPool(ExhibitTileType exhibitTile)
        {
            exhibitPool.Remove(exhibitTile);
        }
        
        private ExhibitTileType GetRandomExhibitTile()
        {
            var rarity = PickRandomExhibitRarity();
            var rarityPool = GetRareExhibitPool(rarity);
            RecalculateViableTags(rarityPool);
            var tag = viableTags.ToList().DrawRandom();
            var tagPool = rarityPool.Where(x => x.Tags.Contains(tag)).ToList();
            return tagPool.Random();
        }

        private ExhibitRarity PickRandomExhibitRarity(float bonusChance = 0f)
        {
            var rarity = ExhibitRarity.Common;

            var random = Rng.Float() - bonusChance;

            if (random < mythicChance)
            {
                rarity = ExhibitRarity.Mythic;
                if (GetRareExhibitCount(rarity) > 0) return rarity;
            }

            if (random < exoticChance)
            {
                rarity = ExhibitRarity.Exotic;
                if (GetRareExhibitCount(rarity) > 0) return rarity;
            }

            if (random < rareChance)
            {
                rarity = ExhibitRarity.Rare;
                if (GetRareExhibitCount(rarity) > 0) return rarity;
            }
            
            rarity = ExhibitRarity.Common;
            if (GetRareExhibitCount(rarity) > 0) return rarity;

            //If case there's no commons left, we roll again but this time with greater chance for rares
            return PickRandomExhibitRarity( bonusChance + 0.1f);
        }

        private List<ExhibitTileType> GetRareExhibitPool(ExhibitRarity rarity)
        {
            return exhibitPool.Where(x => x.Rarity == rarity).ToList();
        }
        
        /// <summary>
        /// returns how many exhibits of that rarity is left in the pool
        /// </summary>
        /// <param name="rarity"></param>
        private int GetRareExhibitCount(ExhibitRarity rarity)
        {
            return exhibitPool.Count(x => x.Rarity == rarity);
        }

        protected override void Execute(StartExhibitPick msg)
        {
            var list = PickRandomExhibits();
            var payload = new BeginPickThree(list.ToArray());
            Message.Publish(payload);
        }
    }
}
