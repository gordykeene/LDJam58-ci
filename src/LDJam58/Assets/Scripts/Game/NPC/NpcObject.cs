using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;

namespace Game.NPC
{
    [CreateAssetMenu(fileName = "NPC", menuName = "NPC/Create NPC")]
    public class NpcObject : ScriptableObject
    {
        public string name;
        public GameObject prefab;
        
        public ExhibitTag personalFavourite;
        public List<ExhibitTag> likes;
        public List<ExhibitTag> dislikes;


        public int GetExhibitScore(ExhibitTileType tile)
        {
            var baseScore = 0;
            
            //if it's a personal favourite, give a large bonus
            if (tile.Tags.Contains(personalFavourite))
            {
                baseScore = 4;
            }
            else
            {
                // set base to 1

                baseScore = 1;
                
                // check for dislikes
                
                foreach (var tag in dislikes)
                {
                    if (tile.Tags.Contains(tag))
                    {
                        baseScore = -3;
                        break;
                    }
                }

                //likes override dislikes and give bonus
                foreach (var tag in likes)
                {
                    if (tile.Tags.Contains(tag))
                    {
                        baseScore = 2;
                        break;
                    }
                }
                
            }
            
            
            //if score is positive, multiply it by rarity

            var finalScore = baseScore;

            if (baseScore > 0) finalScore *= (int)tile.Rarity;
                
            //if score is negative, add rarity to it to offset it
                
            if(baseScore < 0) finalScore += (int)tile.Rarity;
                
            return finalScore;
        }
    }
}