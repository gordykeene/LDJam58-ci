using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.TilePlacement
{
    public class GhostTile : MonoBehaviour
    {
        [SerializeField]
        private string ghostLayerMask;
        [SerializeField]
        private Material errorMaterial;
        [SerializeField]
        private Material ghostMaterial;
        
        private GameObject ghostPlaceable;
        private List<Renderer> ghostRenderers;

        public bool IsOverlapping { get; private set; }
        
        private void OnCollisionEnter(Collision other)
        {
            IsOverlapping = true;
            UpdateMaterial(errorMaterial);
        }

        private void OnCollisionExit(Collision other)
        {
            IsOverlapping = false;
            UpdateMaterial(ghostMaterial);
        }

        private void OnCollisionStay(Collision other)
        {
            if(!IsOverlapping) UpdateMaterial(errorMaterial);
            IsOverlapping = true;
        }


        private void Awake()
        {
            ghostRenderers = new List<Renderer>();
        }

        public void UpdatePlaceable(GameObject placeable)
        {
            if(ghostPlaceable != null) Destroy(ghostPlaceable);
            
            ghostPlaceable = Instantiate(placeable, transform);
            //change all the instance layers to Ghost
            SetLayerRecursively(ghostPlaceable, LayerMask.NameToLayer(ghostLayerMask));
            CollectRenderers(ghostPlaceable);
            UpdateMaterial(ghostMaterial);
        }

        public void DisablePlaceable()
        {
            if(ghostPlaceable != null) ghostPlaceable.SetActive(false);
        }

        public void EnablePlaceable()
        {
            if(ghostPlaceable != null) ghostPlaceable.SetActive(true);
        }
        
        private void UpdateMaterial(Material material)
        {
            foreach (var renderer in ghostRenderers)
            {
                renderer.material = material;
            }
        }

        private void CollectRenderers(GameObject placeable)
        {
            ghostRenderers.Clear();
            var mainRend =  placeable.GetComponent<Renderer>();
            var childRend = placeable.GetComponentsInChildren<Renderer>(placeable);
            
            ghostRenderers.Add(mainRend);
            ghostRenderers.AddRange(childRend);
        }
        
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}