using Input;
using Selection;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraRaycaster : MonoBehaviour
    {
        private UnityEngine.Camera _camera;

        [Inject]
        private SelectionService _selectionService;
        
        private InputAction _selectAction;
        private InputAction _pointAction;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();

            _pointAction = InputSystem.actions.FindAction(InputActionConsts.WORLD_POINT_NAME);
            _selectAction = InputSystem.actions.FindAction(InputActionConsts.WORLD_SELECT_NAME);
        }

        private void OnEnable()
        {
            _selectAction.performed += OnSelectPerformed;
        }

        private void OnSelectPerformed(InputAction.CallbackContext _)
        {
            _selectionService.Select(PointWorldPosition);
        }
        
        private Vector3 PointWorldPosition => _camera.ScreenToWorldPoint(_pointAction.ReadValue<Vector2>());
    }
}