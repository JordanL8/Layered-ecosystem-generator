using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct BiomeTextureDimensions
{
    public int m_PositiveX;
    public int m_NegativeX;
    public int m_PositiveY;
    public int m_NegativeY;

    public int Width => m_PositiveX - m_NegativeX; 
    public int Height => m_PositiveY - m_NegativeY;
    public int TemperatureOffset => m_NegativeX < 0 ? -m_NegativeX : 0;

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
}

[CustomEditor(typeof(EcosystemGenerator))]
public class EcosystemGeneratorEditor : Editor
{
    private Texture2D m_BiomesRepresentationTexture;
    //private int m_TextureBorderX = 10;
    //private int m_TextureBorderY = 10;
    private int m_textureScale = 5;
    private EcosystemGenerator m_TargetComponent;

    private void OnEnable()
    {
        m_TargetComponent = (EcosystemGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawBiomesRepresentationTexture();
        GUILayout.Label(m_BiomesRepresentationTexture);
    }

    private void DrawBiomesRepresentationTexture()
    {
        if (m_TargetComponent == null || m_TargetComponent.m_Biomes.Length == 0) { return; }
        BiomeTextureDimensions textureDimensions = GetTextureDimensions();

        m_BiomesRepresentationTexture = new Texture2D(textureDimensions.Width * m_textureScale,
            textureDimensions.Height * m_textureScale, TextureFormat.RGBA32, false);
        m_BiomesRepresentationTexture.wrapMode = TextureWrapMode.Clamp;
        m_BiomesRepresentationTexture.filterMode = FilterMode.Point;

        DrawBox(0, 0, m_BiomesRepresentationTexture.width, m_BiomesRepresentationTexture.height, m_BiomesRepresentationTexture, Color.white);
        
        foreach (Biome biome in m_TargetComponent.m_Biomes)
        {
            DrawBox(biome.m_TemperatureRange.x + textureDimensions.TemperatureOffset,
                biome.m_RainfallRange.x,
                (biome.m_TemperatureRange.y - biome.m_TemperatureRange.x) * m_textureScale,
                (biome.m_RainfallRange.y - biome.m_RainfallRange.x) * m_textureScale,
                m_BiomesRepresentationTexture,
                biome.m_ColorOnGraph);
        }

        m_BiomesRepresentationTexture.Apply();
    }

    private void DrawBox(int startX, int startY, int width, int height, Texture2D target, Color color)
    {
        for (int x = startX; x < startX + width; ++x)
        {
            for (int y = startY; y < startY + height; ++y)
            {
                target.SetPixel(x, y, color);
            }
        }
    }

    private BiomeTextureDimensions GetTextureDimensions()
    {
        BiomeTextureDimensions textureDimensions = BiomeTextureDimensions.Empty();
        if(m_TargetComponent == null || m_TargetComponent.m_Biomes.Length == 0) { return textureDimensions; }
        foreach (Biome biome in m_TargetComponent.m_Biomes)
        {
            if(biome.m_TemperatureRange.x < textureDimensions.m_NegativeX) { textureDimensions.m_NegativeX = biome.m_TemperatureRange.x; }
            if(biome.m_TemperatureRange.y > textureDimensions.m_PositiveX) { textureDimensions.m_PositiveX = biome.m_TemperatureRange.y; }
            if(biome.m_RainfallRange.x < textureDimensions.m_NegativeY) { textureDimensions.m_NegativeY = biome.m_RainfallRange.x; }
            if(biome.m_RainfallRange.y > textureDimensions.m_PositiveY) { textureDimensions.m_PositiveY = biome.m_RainfallRange.y; }
        }
        return textureDimensions;
    }
}
