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
            DrawBiome(biome);
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

    private void DrawBiome(Biome biome)
    {
        //float[] ycoords = new float[5];
        //float[] xcoords = new float[5];
        //for (int i = 0; i < biome.m_TemperatureAndRainfallSamplePoints.Length; ++i)
        //{
        //    xcoords[i] = (float)biome.m_TemperatureAndRainfallSamplePoints[i].m_Temperature;
        //    ycoords[i] = (float)biome.m_TemperatureAndRainfallSamplePoints[i].m_Rainfall;
        //}

        for (int x = 0; x < m_BiomesRepresentationTexture.width; ++x)
        {
            for(int y = 0; y < m_BiomesRepresentationTexture.height; ++y)
            {
                //if(IsInPolygon(biome.m_TemperatureAndRainfallSamplePoints.Length,xcoords, ycoords, (float)x, (float)y))
                
                if (IsInPolygon(x, y, biome.m_TemperatureAndRainfallSamplePoints))
                {
                    m_BiomesRepresentationTexture.SetPixel(x, y, biome.m_ColorOnGraph);
                }
            }
        }
    }

    bool IsInPolygon(int x, int y, TemperatureRainfallSamplePoints[] vertices)
    {
        int i, j = 0;
        bool result = false;
        for(i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            int curRainfall = vertices[i].m_Rainfall * m_textureScale;
            int compRainfall = vertices[j].m_Rainfall * m_textureScale;
            if ((((vertices[i].m_Rainfall <= y) && ( y < vertices[j].m_Rainfall)) ||
                ((vertices[j].m_Rainfall <= y) && (y < vertices[i].m_Rainfall))) &&
                (x < (vertices[j].m_Temperature - vertices[i].m_Temperature) * (y - vertices[i].m_Rainfall) / (vertices[j].m_Rainfall - vertices[i].m_Rainfall) + vertices[i].m_Temperature))
            {
                result = !result;
            }
        }
        return result;
    }

    //bool IsInPolygon(int npol, float[] xp, float[] yp, float x, float y)
    //{
    //    int i, j = 0;
    //    bool c = false;
    //    for (i = 0, j = npol - 1; i < npol; j = i++)
    //    {
    //        if ((((yp[i] <= y) && (y < yp[j])) ||
    //             ((yp[j] <= y) && (y < yp[i]))) &&
    //            (x < (xp[j] - xp[i]) * (y - yp[i]) / (yp[j] - yp[i]) + xp[i]))
    //            c = !c;
    //    }
    //    return c;
    //}

    private BiomeTextureDimensions GetTextureDimensions()
    {
        BiomeTextureDimensions textureDimensions = BiomeTextureDimensions.Empty();
        if(m_TargetComponent == null || m_TargetComponent.m_Biomes.Length == 0) { return textureDimensions; }
        for(int currentBiome = 0; currentBiome < m_TargetComponent.m_Biomes.Length; ++currentBiome)
        {
            Biome biome = m_TargetComponent.m_Biomes[currentBiome];
            for (int currentSamplePoint = 0; currentSamplePoint < biome.m_TemperatureAndRainfallSamplePoints.Length; ++currentSamplePoint)
            {
                int temperatureValue = biome.m_TemperatureAndRainfallSamplePoints[currentSamplePoint].m_Temperature;
                if(temperatureValue < textureDimensions.m_NegativeX) { textureDimensions.m_NegativeX = temperatureValue; }
                else if(temperatureValue > textureDimensions.m_PositiveX) { textureDimensions.m_PositiveX = temperatureValue; }

                int rainfallValue = biome.m_TemperatureAndRainfallSamplePoints[currentSamplePoint].m_Rainfall;
                if(rainfallValue < textureDimensions.m_NegativeY) { textureDimensions.m_NegativeY = rainfallValue; }
                else if(rainfallValue > textureDimensions.m_PositiveY) { textureDimensions.m_PositiveY = rainfallValue; }
            }
        }
        return textureDimensions;
    }
}
