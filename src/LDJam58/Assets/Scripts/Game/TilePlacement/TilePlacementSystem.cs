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
        [SerializeField]
        private LayerMask tileLayerMask;
        [SerializeField]
        private string ghostLayerMask;
        
        
        [Header("Materials")]
        [SerializeField]
        private Material ghostMaterial;
        [SerializeField]
        private Material errorMaterial;
        
        [Header("Placeable (to be removed)")]
        [AssetsOnly]
        public GameObject placeable;
        
        private PlacementState currentState;
        
        private Quaternion targetRotation;
        private GameObject ghostTile;
        private Material currentTileMaterial;
        private bool canBePlaced;
        
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
        
        private Vector3 rayOrigin;
        private void HandlePlacement()
        {
            var ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            var isHit = Physics.Raycast(ray, out hit,  Mathf.Infinity, placementLayerMask);
            if (!isHit)
            {
                if(currentState == PlacementState.GhostPlacement) DestroyGhostObject();
                currentState = PlacementState.NoTarget;
                return;
            }
            
            if(currentState == PlacementState.NoTarget) CreateGhostObject();
            currentState = PlacementState.GhostPlacement;
            
            var cellSize = grid.cellSize;
            var targetCell = grid.WorldToCell(hit.point+cellSize/2);
            var targetPosition = grid.CellToWorld(targetCell);
            
            //make a ray from target position and upward

            rayOrigin = targetPosition + Vector3.up * 100f;
            var overlapRay = new Ray(rayOrigin, Vector3.down);
            
            var oldCantBePlaced = canBePlaced;
            canBePlaced = !Physics.Raycast(overlapRay, out hit, 200f, tileLayerMask);
            
            ghostTile.transform.position = targetPosition;
            if(oldCantBePlaced != canBePlaced) SetErrorMaterial(!canBePlaced);

            if(!canBePlaced) return;
            
            //unghost on click
            if (Input.GetMouseButtonDown(0))
            {
                currentState = PlacementState.NoTarget;
                var inst= Instantiate(placeable, ghostTile.transform.position, ghostTile.transform.rotation);
                inst.transform.SetParent(grid.transform);
                DestroyGhostObject();
            }
        }

        private void SetErrorMaterial(bool isErroring)
        {
            var rend =  ghostTile.GetComponent<Renderer>();
            rend.material = isErroring ? errorMaterial : ghostMaterial;
        }
        
        private void CreateGhostObject()
        {
            //De burgir has a ghost
            ghostTile = Instantiate(placeable, grid.transform);
            ghostTile.transform.rotation = targetRotation;
            ghostTile.layer = LayerMask.NameToLayer(ghostLayerMask);
            var rend =  ghostTile.GetComponent<Renderer>();
            currentTileMaterial = rend.material;
            rend.material = ghostMaterial;
        }

        private void DestroyGhostObject()
        {
            if(ghostTile != null) Destroy(ghostTile);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayOrigin, Vector3.up);
        }
    }
}
