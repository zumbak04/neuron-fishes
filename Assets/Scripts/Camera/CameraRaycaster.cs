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
        [Inject] private SelectionService _selectionService;

        private UnityEngine.Camera _camera;

        private InputAction _pointAction;
        private InputAction _selectAction;

        private Vector3 PointWorldPosition => _camera.ScreenToWorldPoint(_pointAction.ReadValue<Vector2>());

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
    }
}