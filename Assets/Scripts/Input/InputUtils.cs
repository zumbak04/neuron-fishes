using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Input
{
    public static class InputUtils
    {
        public static bool IsPointerOverUI()
        {
            var module = EventSystem.current.currentInputModule as InputSystemUIInputModule;
            if (!module) {
                return true;
            }
            RaycastResult lastResult = module.GetLastRaycastResult(0);
            GameObject? lastObject = lastResult.gameObject;
            return lastObject != null && lastObject.layer == LayerMask.NameToLayer("UI");
        }
    }
}