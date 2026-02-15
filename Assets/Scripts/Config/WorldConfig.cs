using Unity.Mathematics;

namespace Config
{
    [System.Serializable]
    public struct WorldConfig
    {
        public int2 _bounds;
        public bool _impassibleBounds;
    }
}