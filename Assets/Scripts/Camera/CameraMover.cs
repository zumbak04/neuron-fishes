using Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraMover : MonoBehaviour
    {
        [field: SerializeField]
        public float ZoomSpeed { get; private set; } = 2;
        [field: SerializeField]
        public int MaxZoom { get; private set; } = 30;
        [field: SerializeField]
        public int MinZoom { get; private set; } = 5;
        
        public bool Panning { get; private set; }
        
        private Vector2 _prevPanPosition;
        private UnityEngine.Camera _camera;
        
        private InputAction _activatePanAction;
        private InputAction _pointAction;
        private InputAction _zoomAction;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            
            _activatePanAction = InputSystem.actions.FindAction(InputActionConsts.WORLD_ACTIVATE_PAN_NAME, true);
            _pointAction = InputSystem.actions.FindAction(InputActionConsts.WORLD_POINT_NAME, true);
            _zoomAction = InputSystem.actions.FindAction(InputActionConsts.WORLD_ZOOM_NAME, true);
        }

        private void OnEnable()
        {
            _activatePanAction.performed += OnPanStarted;
            _pointAction.performed += OnPointPerformed;
            _activatePanAction.canceled += OnPanEnded;
            _zoomAction.performed += OnZoomPerformed;
        }

        private void OnDisable()
        {
            _activatePanAction.performed -= OnPanStarted;
            _pointAction.performed -= OnPointPerformed;
            _activatePanAction.canceled -= OnPanEnded;
            _zoomAction.performed -= OnZoomPerformed;
            
            Panning = false;
        }

        private void OnPanStarted(InputAction.CallbackContext _)
        {
            if (InputUtils.IsPointerOverUI()) {
                return;
            }
            Panning = true;
            _prevPanPosition = PointWorldPosition;
            Debug.Log("Pan started");
        }

        private void OnPointPerformed(InputAction.CallbackContext context)
        {
            if (!Panning) {
                return;
            }
            Vector2 panPosition = _camera.ScreenToWorldPoint(context.ReadValue<Vector2>());
            Vector2 panDelta = _prevPanPosition - panPosition;
            
            transform.Translate(panDelta, Space.World);
            
            _prevPanPosition = panPosition + panDelta;
        }

        private void OnPanEnded(InputAction.CallbackContext _)
        {
            if (!Panning) {
                return;
            }
            Panning = false;
            Debug.Log("Pan ended");
        }

        private void OnZoomPerformed(InputAction.CallbackContext context)
        {
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - context.ReadValue<Vector2>().y * ZoomSpeed, MinZoom, MaxZoom);
        }

        private Vector2 PointWorldPosition => _camera.ScreenToWorldPoint(_pointAction.ReadValue<Vector2>());
    }
}