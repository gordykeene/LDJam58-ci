using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// RTS-style top-down camera controller supporting:
    /// - WASD/arrow movement
    /// - Screen-edge scrolling
    /// - Middle-mouse drag panning
    /// - Mouse wheel zoom (orthographic or perspective)
    /// - Optional yaw rotation (Q/E)
    /// - Smoothing and world-bounds clamping
    /// Attach to a Camera. For top-down, set a tilt (e.g., 60°) and position above ground.
    /// </summary>
    public class RtsCameraController : OnMessage<LockCameraMovement, UnlockCameraMovement>
    {
        private bool _movementEnabled = true;
        
        [Header("Movement (Planar)")]
        [SerializeField] private float _moveSpeed = 20f;
        [SerializeField] private bool _useEdgeScroll = true;
        [SerializeField, Range(0f, 0.5f)] private float _edgeThicknessPercent = 0.04f; // % of screen

        [Header("Rotation")]
        [SerializeField] private bool _allowRotation = true;
        [SerializeField] private float _rotationSpeed = 90f; // degrees per second, Q/E

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 200f; // wheel delta applied to distance/size
        [SerializeField] private float _minZoom = 10f;     // for perspective: min distance; for ortho: min size
        [SerializeField] private float _maxZoom = 120f;    // for perspective: max distance; for ortho: max size
        [SerializeField] private float _zoomSmoothing = 0.12f; // 0: instant, higher = smoother

        [Header("Smoothing")]
        [SerializeField] private float _positionSmoothing = 0.08f; // 0: instant

        [Header("Bounds (World-space)")]
        [SerializeField] private bool _clampToBounds;
        [SerializeField] private Vector2 _xBounds = new Vector2(-200f, 200f);
        [SerializeField] private Vector2 _zBounds = new Vector2(-200f, 200f);

        private Camera _cachedCamera;
        private Vector3 _desiredPosition;
        private float _desiredZoom; // perspective: desired height; ortho: size
        private bool _hasMoveInputThisFrame;
        private bool _hasZoomInputThisFrame;
        

        private void Awake()
        {
            _cachedCamera = GetComponent<Camera>();
            if (_cachedCamera == null)
                _cachedCamera = Camera.main;

            _desiredPosition = transform.position;

            if (_cachedCamera != null && _cachedCamera.orthographic)
            {
                _desiredZoom = Mathf.Clamp(_cachedCamera.orthographicSize, _minZoom, _maxZoom);
            }
            else
            {
                // For perspective, treat desired zoom as desired height
                _desiredZoom = Mathf.Clamp(transform.position.y, _minZoom, _maxZoom);
            }
        }

        private void Update()
        {
            if (!_movementEnabled)
                return;

            HandleMovementInput();
            HandleRotationInput();
            HandleZoomInput();
            ApplyDesiredTransform();
        }

        private void HandleMovementInput()
        {
            Vector3 inputMove = Vector3.zero;
            _hasMoveInputThisFrame = false;

            // WASD / Arrow keys: move in camera's local XZ plane (ignoring pitch)
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            // Derive planar axes from yaw so forward/back still works when looking straight down
            GetPlanarAxes(out Vector3 forward, out Vector3 right);
            inputMove += (forward * vertical + right * horizontal);

            // Edge scrolling (sole input for movement)
            if (_useEdgeScroll)
            {
                Vector2 edgeMove = GetEdgeScrollDirection2D();
                // Use proportional magnitude based on proximity to edges
                inputMove += (forward * edgeMove.y + right * edgeMove.x);
            }

            // Mark if there is any movement input
            _hasMoveInputThisFrame = inputMove.sqrMagnitude > 1e-6f;

            // Apply movement solely when there is input; otherwise freeze desired XZ to prevent drift
            if (_hasMoveInputThisFrame)
            {
                float speed = _moveSpeed;
                _desiredPosition += inputMove * speed * Time.unscaledDeltaTime;
            }
            else
            {
                _desiredPosition.x = transform.position.x;
                _desiredPosition.z = transform.position.z;
            }

            if (_clampToBounds)
            {
                _desiredPosition.x = Mathf.Clamp(_desiredPosition.x, _xBounds.x, _xBounds.y);
                _desiredPosition.z = Mathf.Clamp(_desiredPosition.z, _zBounds.x, _zBounds.y);
            }
        }

        private void HandleRotationInput()
        {
            if (!_allowRotation)
                return;

            float rotate = 0f;
            if (Input.GetKey(KeyCode.Q)) rotate -= 1f;
            if (Input.GetKey(KeyCode.E)) rotate += 1f;
            if (Mathf.Abs(rotate) > 0.01f)
            {
                transform.Rotate(Vector3.up, rotate * _rotationSpeed * Time.unscaledDeltaTime, Space.World);
            }
        }

        private void HandleZoomInput()
        {
            float wheel = Input.mouseScrollDelta.y;
            _hasZoomInputThisFrame = Mathf.Abs(wheel) >= 0.001f;
            if (!_hasZoomInputThisFrame)
                return;

            if (_cachedCamera != null && _cachedCamera.orthographic)
            {
                _desiredZoom = Mathf.Clamp(_desiredZoom - wheel * (_zoomSpeed * 0.01f), _minZoom, _maxZoom);
            }
            else
            {
                // For perspective, adjust desired height based on scroll
                _desiredZoom = Mathf.Clamp(_desiredZoom - wheel * (_zoomSpeed * 0.1f), _minZoom, _maxZoom);
            }
        }

        private void ApplyDesiredTransform()
        {
            // Zoom application (orthographic and perspective)
            if (_cachedCamera != null && _cachedCamera.orthographic)
            {
                if (_hasZoomInputThisFrame && _zoomSmoothing > 0.0001f)
                {
                    float current = _cachedCamera.orthographicSize;
                    float target = _desiredZoom;
                    _cachedCamera.orthographicSize = Mathf.Lerp(current, target, 1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(0.0001f, _zoomSmoothing)));
                }
                else
                {
                    _cachedCamera.orthographicSize = _desiredZoom;
                }
            }
            else
            {
                // For perspective: only adjust Y while zoom input is active; otherwise freeze Y
                if (_hasZoomInputThisFrame && _zoomSmoothing > 0.0001f)
                {
                    float currentY = transform.position.y;
                    float targetY = _desiredZoom;
                    float newY = Mathf.Lerp(currentY, targetY, 1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(0.0001f, _zoomSmoothing)));
                    _desiredPosition.y = newY;
                }
                else
                {
                    _desiredPosition.y = _hasZoomInputThisFrame ? _desiredZoom : transform.position.y;
                }
            }

            // Position smoothing (freeze when neutral: no movement, no zoom)
            bool neutral = !_hasMoveInputThisFrame && !_hasZoomInputThisFrame;
            if (neutral || _positionSmoothing <= 0.0001f)
            {
                transform.position = _desiredPosition;
            }
            else
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    _desiredPosition,
                    1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(0.0001f, _positionSmoothing)));
            }
        }

    private void GetPlanarAxes(out Vector3 forward, out Vector3 right)
    {
        float yawRad = transform.eulerAngles.y * Mathf.Deg2Rad;
        forward = new Vector3(Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
        right = new Vector3(Mathf.Cos(yawRad), 0f, -Mathf.Sin(yawRad));
    }

        private Vector2 GetEdgeScrollDirection2D()
        {
            if (!Application.isFocused)
                return Vector2.zero;

            Vector2 dir = Vector2.zero;
            Vector3 mouse = Input.mousePosition;
            float w = Screen.width;
            float h = Screen.height;
            float pct = Mathf.Clamp01(_edgeThicknessPercent);
            float tX = Mathf.Max(1f, w * pct);
            float tY = Mathf.Max(1f, h * pct);

            if (mouse.x <= tX) dir.x = -(tX - mouse.x) / tX; // -1..0
            else if (mouse.x >= w - tX) dir.x = (mouse.x - (w - tX)) / tX; // 0..1

            if (mouse.y <= tY) dir.y = -(tY - mouse.y) / tY; // -1..0 (down)
            else if (mouse.y >= h - tY) dir.y = (mouse.y - (h - tY)) / tY; // 0..1 (up)

            return dir;
        }

        // Public API
        public void SetBounds(Vector2 xRange, Vector2 zRange)
        {
            _xBounds = xRange;
            _zBounds = zRange;
            _clampToBounds = true;
        }

        protected override void Execute(LockCameraMovement msg)
        {
            _movementEnabled = false;
        }

        protected override void Execute(UnlockCameraMovement msg)
        {
            _movementEnabled = true;
        }
    }
}


