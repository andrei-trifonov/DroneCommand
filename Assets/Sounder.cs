using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Sounder : MonoBehaviour
{
    public int textureWidth = 512;            // ширина экрана эхолота
    public int textureHeight = 256;            // высота экрана эхолота
    public float maxDepth = 100f;              // максимальная глубина
    public float pingInterval = 0.2f;          // частота пингов (сек)
    public float soundSpeed = 1500f;           // скорость звука в воде м/с
    public LayerMask groundLayer;              // слой дна
    public RawImage sonarScreen;   
    public RawImage sonarScreenBG;   // UI картинка для отображения сонар-данных
    public TextMeshProUGUI depthText;           // UI текст глубины
   
    private Texture2D sonarTexture;
    private Texture2D bgTexture;
    private Color[] clearColumn;
    private int currentX = 0;
    private float timeSinceLastPing = 0f;
    private Coroutine pingRoutine;
    private float lastMeasuredDepth = 0f;

    void Start()
    {
        sonarTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        sonarTexture.filterMode = FilterMode.Point;
        sonarTexture.wrapMode = TextureWrapMode.Clamp;

       bgTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        bgTexture.filterMode = FilterMode.Point;
        bgTexture.wrapMode = TextureWrapMode.Clamp;
        clearColumn = new Color[textureHeight];
        for (int i = 0; i < textureHeight; i++)
            clearColumn[i] = Color.clear; // Прозрачная очистка для эхо-перезаписи

        sonarScreen.texture = sonarTexture;
        sonarScreenBG.texture = bgTexture;
DrawBackgroundAndGrid();
      
    }
    
    void DrawBackgroundAndGrid()
    {
        Color backgroundColorTop = new Color(0.05f, 0.1f, 0.2f); // чуть светлее вверху
        Color backgroundColorBottom = new Color(0f, 0f, 0.05f);  // темнее внизу

        for (int y = 0; y < textureHeight; y++)
        {
            float t = y / (float)textureHeight;
            Color bgColor = Color.Lerp(backgroundColorBottom, backgroundColorTop, t);

            for (int x = 0; x < textureWidth; x++)
            {
                bgTexture.SetPixel(x, y, bgColor);
            }
        }

        // Нарисовать линии сетки
        int gridSpacingMeters = 10; // шаг сетки по глубине
        for (int depth = gridSpacingMeters; depth < maxDepth; depth += gridSpacingMeters)
        {
            int y = Mathf.RoundToInt((depth / maxDepth) * textureHeight);
            y = textureHeight - y; // инвертируем

            for (int x = 0; x < textureWidth; x++)
            {
                Color gridColor = new Color(0.3f, 0.3f, 0.3f); // сероватые линии
                bgTexture.SetPixel(x, y, gridColor);
            }
        }

        bgTexture.Apply();
    }
    void Update()
    {
        timeSinceLastPing += Time.deltaTime;

        if (timeSinceLastPing >= pingInterval)
        {
            if (pingRoutine != null)
                StopCoroutine(pingRoutine);

            pingRoutine = StartCoroutine(Ping());
            timeSinceLastPing = 0f;
        }
    }

    IEnumerator Ping()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDepth, groundLayer))
        {
            float distance = hit.distance;
            float travelTime = (distance * 2) / soundSpeed;

            yield return new WaitForSeconds(travelTime);

            lastMeasuredDepth = distance;

            if (depthText != null)
                depthText.text = $"Depth: {lastMeasuredDepth:F1} m";

            DrawEcho(currentX, distance);
        }
        else
        {
            lastMeasuredDepth = maxDepth;
            if (depthText != null)
                depthText.text = "Depth: --";

            DrawEcho(currentX, maxDepth);
        }

        currentX++;
        if (currentX >= textureWidth)
        {
            currentX = 0;
        
        }
    }

    void DrawEcho(int x, float distance)
    {
        // Очистить колонку
        sonarTexture.SetPixels(x, 0, 1, textureHeight, clearColumn);

        // Основной удар по дну
        int mainEchoY = Mathf.Clamp(Mathf.RoundToInt((distance / maxDepth) * textureHeight), 0, textureHeight - 1);

        // Тип грунта (случайный, но можно потом заменить на реальный материал)
        GroundType groundType = GetGroundType(distance);

        int echoThickness = GetEchoThickness(groundType); // сколько пикселей вниз эхо будет размазано

        for (int offset = 0; offset < echoThickness; offset++)
        {
            int y = mainEchoY + offset;
            if (y < textureHeight)
            {
                float fade = 1f - (offset / (float)echoThickness); // постепенное затухание вниз
                Color echoColor = DepthColor(distance, true) * fade;
                sonarTexture.SetPixel(x, textureHeight - y, echoColor);
            }
        }

        // Дополнительные слабые отражения
        for (int i = 1; i <= 3; i++)
        {
            float fakeDepth = distance + Random.Range(2f, 10f) * i;
            if (fakeDepth < maxDepth)
            {
                int echoY = Mathf.Clamp(Mathf.RoundToInt((fakeDepth / maxDepth) * textureHeight), 0, textureHeight - 1);
                sonarTexture.SetPixel(x, textureHeight - echoY, DepthColor(fakeDepth, false) * 0.5f);
            }
        }

        // Шум фона
        for (int i = 0; i < 5; i++)
        {
            int noiseY = Random.Range(0, textureHeight);
            Color noiseColor = Color.Lerp(Color.black, Color.gray, Random.Range(0.1f, 0.3f));
            sonarTexture.SetPixel(x, noiseY, noiseColor);
        }

        sonarTexture.Apply();
    }
    
    
    enum GroundType { HardRock, Sand, Mud }

    GroundType GetGroundType(float depth)
    {
        // Можно сделать более умную модель по координатам или другим данным
        float rand = Random.value;
        if (rand < 0.3f) return GroundType.HardRock;
        else if (rand < 0.6f) return GroundType.Sand;
        else return GroundType.Mud;
    }

    int GetEchoThickness(GroundType type)
    {
        switch (type)
        {
            case GroundType.HardRock:
                return Random.Range(1, 2); // почти нет размытия
            case GroundType.Sand:
                return Random.Range(2, 5); // среднее размытие
            case GroundType.Mud:
                return Random.Range(5, 8); // большое размытие
            default:
                return 3;
        }
    }
    Color DepthColor(float depth, bool isMainEcho)
    {
        float normalized = depth / maxDepth;
        float intensity = Mathf.Lerp(1f, 0.2f, normalized); // затухание с глубиной

        Color color;

        
        
        if (normalized < 0.3f) // мелководье (песок)
            color = Color.Lerp(Color.yellow, Color.red, Random.Range(0.0f, 0.2f));
        else if (normalized < 0.7f) // средние глубины (ил)
            color = Color.Lerp(Color.cyan, Color.blue, Random.Range(0.2f, 0.5f));
        else // большая глубина (скалы)
            color = Color.Lerp(Color.blue, Color.black, Random.Range(0.4f, 0.8f));

        if (!isMainEcho)
            intensity *= 0.5f; // для слабых отражений делаем сигнал слабее

        return new Color( color.r, color.g,color.b,   intensity);
    }
}
