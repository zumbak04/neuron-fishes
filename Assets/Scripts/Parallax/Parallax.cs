using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Parallax
{
    public class Parallax : MonoBehaviour
    {
        private const int START_ORDER_IN_LAYER = -1;

        private List<ParallaxLayer> _layers = null!;
        private Vector3? _prevCameraPosition;

        public void Awake()
        {
            _layers = GetComponentsInChildren<ParallaxLayer>().ToList();
            for (var i = 0; i < _layers.Count; i++) {
                _layers[i].Init(START_ORDER_IN_LAYER - i);
            }
        }

        private void Update()
        {
            // todo zumbak использовать current камеру
            UnityEngine.Camera? mainCamera = UnityEngine.Camera.main;
            if (!mainCamera) {
                return;
            }

            Vector3 currentCameraPosition = mainCamera.transform.position;
            Vector3 cameraDelta = _prevCameraPosition.HasValue
                ? currentCameraPosition - _prevCameraPosition.Value
                : Vector3.zero;
            foreach (ParallaxLayer layer in _layers) {
                layer.DoUpdate(currentCameraPosition, cameraDelta);
            }

            _prevCameraPosition = currentCameraPosition;
        }
    }
}