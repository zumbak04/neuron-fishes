using UnityEngine;

namespace Parallax
{
    public class ParallaxLayer : MonoBehaviour
    {
        [field: SerializeField] public float Factor { get; private set; }

        [field: SerializeField] public Sprite Image { get; private set; } = null!;

        private Bounds _bounds;
        private Bounds _cameraBounds;

        public void Init(int orderInLayer)
        {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }

            GameObject firstImageObject = new("Image 1");
            firstImageObject.transform.SetParent(transform);
            var firstRenderer = firstImageObject.AddComponent<SpriteRenderer>();
            firstRenderer.sprite = Image;
            firstRenderer.sortingOrder = orderInLayer;

            Vector3 rendererSize = firstRenderer.bounds.size;

            firstImageObject.transform.localPosition = rendererSize / 2;

            InstantiateImageObject("Image 2", new Vector2(-rendererSize.x / 2, rendererSize.y / 2), firstImageObject);
            InstantiateImageObject("Image 3", -rendererSize / 2, firstImageObject);
            InstantiateImageObject("Image 4", new Vector2(rendererSize.x / 2, -rendererSize.y / 2), firstImageObject);

            _bounds.extents = rendererSize;
            _bounds.center = transform.position;
            _cameraBounds.extents = rendererSize / 2;
            _cameraBounds.center = transform.position;
        }

        public void DoUpdate(Vector3 cameraPosition, Vector3 cameraDelta)
        {
            transform.Translate(cameraDelta * Factor, Space.World);

            UpdateBoundsCenter();

            if (_cameraBounds.extents == Vector3.zero) {
                return;
            }

            if (_cameraBounds.max.x < cameraPosition.x) {
                transform.Translate(new Vector3(_bounds.size.x / 2, 0));
            }
            else if (_cameraBounds.min.x > cameraPosition.x) {
                transform.Translate(new Vector3(-_bounds.size.x / 2, 0));
            }

            if (_cameraBounds.max.y < cameraPosition.y) {
                transform.Translate(new Vector3(0, _bounds.size.y / 2));
            }
            else if (_cameraBounds.min.y > cameraPosition.y) {
                transform.Translate(new Vector3(0, -_bounds.size.y / 2));
            }

            UpdateBoundsCenter();
        }

        private void InstantiateImageObject(string instanceName, Vector2 offset, GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, transform, true);
            instance.name = instanceName;
            instance.transform.localPosition = offset;
        }

        private void UpdateBoundsCenter()
        {
            _bounds.center = transform.position;
            _cameraBounds.center = transform.position;
        }
    }
}