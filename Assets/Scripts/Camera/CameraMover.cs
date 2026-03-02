using Input;
using Math;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using World;

namespace Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraMover : MonoBehaviour
    {
        [field: SerializeField] public float ZoomSpeed { get; private set; } = 2;

        [field: SerializeField] public int MaxZoom { get; private set; } = 30;

        [field: SerializeField] public int MinZoom { get; private set; } = 5;
        
        private UnityEngine.Camera _camera;
        private InputAction _activatePanAction;
        private InputAction _pointAction;
        private InputAction _zoomAction;
        private EntityQuery _worldConfigQuery;
        
        private Vector2 _prevPanPosition;

        public bool Panning { get; private set; }

        private Vector2 PointWorldPosition => _camera.ScreenToWorldPoint(_pointAction.ReadValue<Vector2>());

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();

            _activatePanAction = InputSystem.actions.FindAction(InputActionConsts.WORLD_ACTIVATE_PAN_NAME, true);
            _pointAction = InputSystem.actions.FindAction(InputActionConsts.WORLD_POINT_NAME, true);
            _zoomAction = InputSystem.actions.FindAction(InputActionConsts.WORLD_ZOOM_NAME, true);
            
            EntityManager em = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            _worldConfigQuery = em.CreateEntityQuery(ComponentType.ReadOnly<WorldConfig>());
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
            if (InputUtils.IsPointerOverUI(0)) {
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
            WorldConfig worldConfig = _worldConfigQuery.GetSingleton<WorldConfig>();

            Vector2 panPosition = _camera.ScreenToWorldPoint(context.ReadValue<Vector2>());
            Vector2 panDelta = _prevPanPosition - panPosition;

            Vector3 targetPosition = transform.position + new Vector3(panDelta.x, panDelta.y);
            if (worldConfig.ImpassibleBounds) {
                float2 topRightCorner = WorldBoundsUtils.GetTopRightCorner(worldConfig.Bounds);
                float2 botLeftCorner = WorldBoundsUtils.GetBotLeftCorner(worldConfig.Bounds);
                targetPosition = MathUtils.Clamp(targetPosition, botLeftCorner, topRightCorner);
            }
            transform.position = targetPosition;

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
            _camera.orthographicSize = Mathf.Clamp(
                _camera.orthographicSize - context.ReadValue<Vector2>().y * ZoomSpeed,
                MinZoom, MaxZoom);
        }
    }
}