public struct BiomeTextureDimensions
{
    public int m_PositiveX;
    public int m_NegativeX;
    public int m_PositiveY;
    public int m_NegativeY;

    public int Width => m_PositiveX - m_NegativeX;
    public int Height => m_PositiveY - m_NegativeY;
    public int TemperatureOffset => -m_NegativeX;
    public int RainfallOffset => -m_NegativeY;

    public static BiomeTextureDimensions Empty()
    {
        return new BiomeTextureDimensions()
        {
            m_PositiveX = 0,
            m_NegativeX = 0,
            m_PositiveY = 0,
            m_NegativeY = 0
        };
    }

    public static BiomeTextureDimensions MinMax()
    {
        return new BiomeTextureDimensions()
        {
            m_PositiveX = int.MinValue,
            m_NegativeX = int.MaxValue,
            m_PositiveY = int.MinValue,
            m_NegativeY = int.MaxValue
        };
    }
}