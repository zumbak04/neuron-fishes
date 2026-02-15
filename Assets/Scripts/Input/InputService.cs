using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Input
{
    public class InputService : IInitializable
    {
        void IInitializable.Initialize()
        {
            InputSystem.actions.Enable();
        }
    }
}