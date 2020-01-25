using UnityEngine;


public class Graph
{
    private Texture2D m_GraphTexture;
    private int m_XAxisOffset;
    private int m_YAxisOffset;
    private int m_GraphScale;

    public Graph(int graphWidth, int graphHeight, int xAxisOffset, int yAxisOffset, int graphScale)
    {
        m_XAxisOffset = xAxisOffset;
        m_YAxisOffset = yAxisOffset;
        m_GraphScale = graphScale;
        m_GraphTexture = new Texture2D(graphWidth * m_GraphScale, graphHeight * m_GraphScale, TextureFormat.RGBA32, false);
        m_GraphTexture.wrapMode = TextureWrapMode.Clamp;
        m_GraphTexture.filterMode = FilterMode.Point;
    }

    public Texture2D GetGraph() => m_GraphTexture;

    public void DrawBox(int startX, int startY, int width, int height, Color color)
    {
        for (int x = startX; x < GetCorrectedXValue(startX + width); ++x)
        {
            for (int y = startY; y < GetCorrectedYValue(startY + height); ++y)
            {
                m_GraphTexture.SetPixel(x, y, color);
            }
        }
        m_GraphTexture.Apply();
    }

    public void DrawBox(Vector2Int centre, int radius, Color color)
    {
        int correctedX = GetCorrectedXValue(centre.x);
        int correctedY = GetCorrectedXValue(centre.y);
        for(int x = -radius; x < radius; ++x)
        {
            for(int y = -radius; y < radius; ++y)
            {
                int drawX = correctedX + x;
                if(drawX < 0 || drawX >= m_GraphTexture.width) { continue; }
                int drawY = correctedY + y;
                if (drawY < 0 || drawY >= m_GraphTexture.height) { continue; }
                m_GraphTexture.SetPixel(drawX, drawY, color);
            }
        }
        m_GraphTexture.Apply();
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
                    m_GraphTexture.SetPixel(x, y, color);
                }
            }
        }
        for (int i = 0; i < vertices.Length; ++i)
        {
            m_GraphTexture.SetPixel(GetCorrectedXValue(vertices[i].x), GetCorrectedYValue(vertices[i].y), Color.magenta);
            if(i > 0)
            {
                DrawLine(vertices[i], vertices[i - 1], Color.black);
            }
        }
        m_GraphTexture.Apply();
    }

    public void DrawLine(Vector2Int start, Vector2Int end, Color col)
    {
        int x0 = GetCorrectedXValue(start.x);
        int x1 = GetCorrectedXValue(end.x);
        int y0 = GetCorrectedXValue(start.y);
        int y1 = GetCorrectedXValue(end.y);
        int dy = y1 - y0;
        int dx = x1 - x0;
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;

        m_GraphTexture.SetPixel(x0, y0, col);
        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;
                m_GraphTexture.SetPixel(x0, y0, col);
            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;
                m_GraphTexture.SetPixel(x0, y0, col);
            }
        }
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
        return (value + m_XAxisOffset) * m_GraphScale;
    }

    private int GetCorrectedYValue(int value)
    {
        return (value + m_YAxisOffset) * m_GraphScale;
    }
}
