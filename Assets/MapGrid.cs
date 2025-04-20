using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MapGrid : MonoBehaviour
{
   [Header("Линии сетки")]
    public Material lineMaterial;
    public float lineWidth = 0.02f;

    [Header("Настройки подписей")]
    public Font labelFont;

    public void DrawGrid(int zoom, double centerLat, double centerLon, int tileRangeX, int tileRangeY)
    {
        ClearGrid();

        TileLoader tileLoader = GetComponent<TileLoader>();
        if (tileLoader == null) return;

        int centerX = tileLoader.LonToTileX(centerLon, zoom);
        int centerY = tileLoader.LatToTileY(centerLat, zoom);

        for (int dx = -tileRangeX; dx <= tileRangeX; dx++)
        {
            for (int dy = -tileRangeY; dy <= tileRangeY; dy++)
            {
                int tileX = centerX + dx;
                int tileY = centerY + dy;

                Vector3 tilePos = new Vector3(dx * 1.0f, 0.02f, -dy * 1.0f);

                // Горизонтальная и вертикальная линии
                CreateLine(tilePos + new Vector3(-0.5f, 0, -0.5f), tilePos + new Vector3(0.5f, 0, -0.5f)); // верхняя граница
                CreateLine(tilePos + new Vector3(-0.5f, 0, -0.5f), tilePos + new Vector3(-0.5f, 0, 0.5f)); // левая граница

                // Подписи координат
                if (dx == -tileRangeX)
                {
                    double lat = tileLoader.WorldToLatLon(tilePos).x;
                    CreateLabel(tilePos + new Vector3(-0.6f, 0, -0.6f), $"{lat:F2}°");
                }

                if (dy == -tileRangeY)
                {
                    double lon = tileLoader.WorldToLatLon(tilePos).y;
                    CreateLabel(tilePos + new Vector3(-0.6f, 0, 0.6f), $"{lon:F2}°");
                }
            }
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = transform;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = lineMaterial;
        lr.widthMultiplier = lineWidth;
        lr.useWorldSpace = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
    }

    void CreateLabel(Vector3 position, string text)
    {
        GameObject labelObj = new GameObject("GridLabel");
        labelObj.transform.parent = transform;
        labelObj.transform.position = position;
        labelObj.transform.eulerAngles = new Vector3(90, 0, 0);

        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 10;
        textMesh.characterSize = 0.1f;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        if (labelFont != null)
        {
            textMesh.font = labelFont;
            textMesh.GetComponent<MeshRenderer>().material = labelFont.material;
        }
    }

    public void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            if(child.name =="GridLabel")
            Destroy(child.gameObject);
        }
    }
}