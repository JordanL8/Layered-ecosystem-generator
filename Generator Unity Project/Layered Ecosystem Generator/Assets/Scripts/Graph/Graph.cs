using UnityEngine;


public class Graph
{
    private Texture2D m_graphTexture;
    private int m_xAxisOffset;
    private int m_yAxisOffset;
    private int m_xAxisScale;

    public Graph(int graphWidth, int graphHeight, int xAxisOffset, int yAxisOffset, int xScale = 10)
    {
        m_xAxisOffset = xAxisOffset;
        m_yAxisOffset = yAxisOffset;
        m_xAxisScale = xScale;
        m_graphTexture = new Texture2D(graphWidth, graphHeight, TextureFormat.RGBA32, false);
        m_graphTexture.wrapMode = TextureWrapMode.Clamp;
        m_graphTexture.filterMode = FilterMode.Point;
    }

    public Texture2D GetGraph() => m_graphTexture;

    public void Fill(Color color)
    {
        for (int x = 0; x < m_graphTexture.width; ++x)
        {
            for (int y = 0; y < m_graphTexture.height; ++y)
            {
                m_graphTexture.SetPixel(x, y, color);
            }
        }
    }

    public void DrawBox(int startX, int startY, int width, int height, Color color)
    {
        int correctedX = GetCorrectedXValue(startX);
        int correctedY = GetCorrectedXValue(startY);
        for (int x = correctedX; x < GetCorrectedXValue(startX + width); ++x)
        {
            for (int y = correctedY; y < GetCorrectedYValue(startY + height); ++y)
            {
                m_graphTexture.SetPixel(x, y, color);
            }
        }
    }

    public void DrawCrossBox(int startX, int startY, int width, int height, Color color)
    {
        for (int x = startX; x < (startX + width); ++x)
        {
            for (int y = startY; y < (startY + height); ++y)
            {
                m_graphTexture.SetPixel(x, y, color);
            }
        }
    }

    public void DrawCross(Vector2Int centre, int size, int width, Color color)
    {
        int correctedX = GetCorrectedXValue(centre.x);
        int correctedY = GetCorrectedYValue(centre.y);
        DrawCrossBox(correctedX - size, correctedY - width, (size * 2) + 1, (width * 2) + 1, color);
        DrawCrossBox(correctedX - width, correctedY - size, (width * 2) + 1, (size * 2) + 1, color);
    }

    public void DrawPolygon(Vector2Int[] vertices, Color color)
    {
        GraphBoundingBox polygonBoundingBox = new GraphBoundingBox(vertices);

        for (int x = GetCorrectedXValue(polygonBoundingBox.MinX); x < GetCorrectedXValue(polygonBoundingBox.MaxX); ++x)
        {
            for (int y = GetCorrectedYValue(polygonBoundingBox.MinY); y < GetCorrectedYValue(polygonBoundingBox.MaxY); ++y)
            {
                if (IsInPolygon(x, y, vertices))
                {
                    m_graphTexture.SetPixel(x, y, color);
                }
            }
        }
    }

    public void Apply()
    {
        m_graphTexture.Apply();
    }

    public bool IsInPolygon(int y, int x, Vector2Int[] vertices)   // X and Y is passed in flipped for this function.
    {
        int i, j = 0;
        bool result = false;
        for (i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            int curX = GetCorrectedXValue(vertices[i].x);
            int compX = GetCorrectedXValue(vertices[j].x);
            int curY = GetCorrectedYValue(vertices[i].y);
            int compY = GetCorrectedYValue(vertices[j].y);
            if ((((curX <= y) && (y < compX)) ||
                ((compX <= y) && (y < curX))) &&
                (x < (compY - curY) * (y - curX) / (compX - curX) + curY))
            {
                result = !result;
            }
        }
        return result;
    }

    private int GetCorrectedXValue(int value)
    {
        return (value + m_xAxisOffset) * m_xAxisScale;
    }

    private int GetCorrectedYValue(int value)
    {
        return (value + m_yAxisOffset);
    }
}
