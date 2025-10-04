using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.TilePlacement
{
    public class TilePlacementSystem : MonoBehaviour
    {
        [Header("Scene objects")]
        [SerializeField]
        private Camera raycastCamera;
        [SerializeField]
        private Grid grid;

        [Header("Settings")] 
        [SerializeField] private float rotationAngle;
        [SerializeField]
        private LayerMask placementLayerMask;
        
        
        [Header("Materials")]
        [SerializeField]
        private GhostTile ghostTile;
        
        [Header("Placeable (to be removed)")]
        [AssetsOnly]
        public GameObject placeable;
        
        private PlacementState currentState;
        
        private Quaternion targetRotation;
        private Material currentTileMaterial;

        
        private enum PlacementState
        {
            Disabled,
            NoTarget,
            GhostPlacement
        }

        [Button]
        public void StartPlacing()
        {
            targetRotation =  Quaternion.identity;
            currentState = PlacementState.NoTarget;
            ghostTile.UpdatePlaceable(placeable);
        }

        [Button]
        public void StopPlacing()
        {
            currentState = PlacementState.Disabled;
        }
        
        private void Update()
        {
            if(currentState == PlacementState.Disabled) return;
            
            HandlePlacement();
            HandleRotation();
        }

        private void HandleRotation()
        {
            if(currentState != PlacementState.GhostPlacement) return;
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                targetRotation *= Quaternion.Euler(0f, -90f, 0f);
                ghostTile.transform.rotation = targetRotation;
            }
        }
        
        private void HandlePlacement()
        {
            var ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            var isHit = Physics.Raycast(ray, out hit,  Mathf.Infinity, placementLayerMask);
            if (!isHit)
            {
                if(currentState == PlacementState.GhostPlacement) DisableGhostObject();
                currentState = PlacementState.NoTarget;
                return;
            }
            
            if(currentState == PlacementState.NoTarget) EnableGhostObject();
            currentState = PlacementState.GhostPlacement;
            
            var cellSize = grid.cellSize;
            var targetCell = grid.WorldToCell(hit.point+cellSize/2);
            var targetPosition = grid.CellToWorld(targetCell);
            
            //make a ray from target position and upward
            
            ghostTile.transform.position = targetPosition;

            if(ghostTile.IsOverlapping) return;
            
            //unghost on click
            if (Input.GetMouseButtonDown(0))
            {
                currentState = PlacementState.NoTarget;
                var inst= Instantiate(placeable, ghostTile.transform.position, ghostTile.transform.rotation);
                inst.transform.SetParent(grid.transform);
                DisableGhostObject();
            }
        }

        
        private void EnableGhostObject()
        {
            ghostTile.EnablePlaceable();
        }

        private void DisableGhostObject()
        {
            ghostTile.DisablePlaceable();
        }
    }
}
