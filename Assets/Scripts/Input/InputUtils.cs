using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Input
{
    public static class InputUtils
    {
        public static bool IsPointerOverUI(int pointerOrTouchId)
        {
            var module = EventSystem.current.currentInputModule as InputSystemUIInputModule;
            if (!module) {
                return false;
            }

            RaycastResult lastResult = module.GetLastRaycastResult(pointerOrTouchId);
            GameObject? lastObject = lastResult.gameObject;
            return lastObject != null && lastObject.layer == LayerMask.NameToLayer("UI");
        }
    }
}