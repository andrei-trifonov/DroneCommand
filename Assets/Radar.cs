using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

class TrackedEcho
{
    public Vector2 pixelPos;
    public float timestamp;
    public Color color;
}

public class Radar : MonoBehaviour
{
    [Header("Radar Settings")]
    public int textureSize = 256;
    public float radarRange = 128f;
    public LayerMask obstacleLayer;
    public RawImage radarDisplay;
    public Transform radarOrigin;

    [Header("Scan Settings")]
    public float scanSpeed = 60f; // degrees/sec
    public float scanWidth = 4f;  // in degrees

    [Header("Visual Settings")]
    public float echoFadeDuration = 1.5f;
    public int noisePointsPerFrame = 30;
    public float glowSize = 2f;

    private Texture2D radarTexture;
    private Color32[] pixelBuffer;
    private Vector2 center;
    private float pixelsPerUnit;
    private float currentScanAngle;
    public float offset;
    private List<TrackedEcho> echoBuffer = new List<TrackedEcho>();

    void Start()
    {
        radarTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        radarTexture.filterMode = FilterMode.Point;
        radarDisplay.texture = radarTexture;

        pixelBuffer = new Color32[textureSize * textureSize];
        center = new Vector2(textureSize / 2f, textureSize / 2f);
        pixelsPerUnit = textureSize / (radarRange * 2f);
    }

    void Update()
    {
        currentScanAngle += scanSpeed * Time.deltaTime;
        if (currentScanAngle >= 360f) currentScanAngle -= 360f;

        FadePixels();
        ScanObstacles();
        DrawEchoes();
        AddScanLine();
        AddNoise();

        radarTexture.SetPixels32(pixelBuffer);
        radarTexture.Apply();
    }

    void FadePixels()
    {
        for (int i = 0; i < pixelBuffer.Length; i++)
        {
            Color32 c = pixelBuffer[i];
            c.r = (byte)(c.r * 0.95f);
            c.g = (byte)(c.g * 0.95f);
            c.b = (byte)(c.b * 0.95f);
            c.a = 255;
            pixelBuffer[i] = c;
        }
    }

    void ScanObstacles()
    {
        Collider[] hits = Physics.OverlapSphere(radarOrigin.position, radarRange, obstacleLayer);

        foreach (var hit in hits)
        {
            Vector3 dir = hit.transform.position - radarOrigin.position;
            float distance = dir.magnitude;
            if (distance > radarRange) continue;

            // Получаем угол от центра радара до объекта в градусах (0–360)
            float angleToObject = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            if (angleToObject < 0f) angleToObject += 360f;

            // Разница между сканером и объектом
            float delta = Mathf.DeltaAngle(currentScanAngle, angleToObject);
            if (Mathf.Abs(delta) > scanWidth / 2f) continue;

            // Координаты объекта на радаре
            Vector2 radarPos = new Vector2(dir.x, dir.z) * pixelsPerUnit + center;

            echoBuffer.Add(new TrackedEcho
            {
                pixelPos = radarPos,
                timestamp = Time.time,
                color = Color.green
            });
        }
    }


    void DrawEchoes()
    {
        foreach (var echo in echoBuffer.ToList())
        {
            float age = Time.time - echo.timestamp;
            if (age > echoFadeDuration)
            {
                echoBuffer.Remove(echo);
                continue;
            }

            float intensity = Mathf.Lerp(1f, 0f, age / echoFadeDuration);
            DrawGlow((int)echo.pixelPos.x, (int)echo.pixelPos.y, echo.color * intensity, glowSize);
        }
    }

    void AddNoise()
    {
        for (int i = 0; i < noisePointsPerFrame; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Mathf.Sqrt(Random.Range(0f, 1f)) * radarRange;
            Vector2 noiseDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 noisePos = noiseDir * radius * pixelsPerUnit + center;

            DrawPixel((int)noisePos.x, (int)noisePos.y, new Color(0f, 0.4f, 0.4f, 0.2f));
        }
    }

    void AddScanLine()
    {
        float angleRad = currentScanAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        for (int r = 0; r < radarRange * pixelsPerUnit; r++)
        {
            Vector2 pos = direction * r + center;
            DrawPixel((int)pos.x, (int)pos.y, new Color(0.2f, 1f, 0.2f));
        }
    }
    void DrawPixel(int x, int y, Color color, float alpha = 0.6f)
    {
        if (x < 0 || x >= textureSize || y < 0 || y >= textureSize)
            return;

        int index = y * textureSize + x;
        pixelBuffer[index] = Color32.Lerp(pixelBuffer[index], color, alpha);
    }

    void DrawGlow(int cx, int cy, Color color, float radius)
    {
        int r = Mathf.CeilToInt(radius);
        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist > radius) continue;

                float alpha = 1f - (dist / radius);
                DrawPixel(cx + x, cy + y, color, alpha);
            }
        }
    }
}
