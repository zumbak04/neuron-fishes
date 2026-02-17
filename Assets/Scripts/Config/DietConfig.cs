namespace Config
{
    [System.Serializable]
    public struct DietConfig
    {
        public float _minNutrients;
        public float _maxNutrients;
        public float _nutrientLossPerSecond;

        public DietSynthesizingConfig _synthesizing;
    }
}