using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class Depth : MonoBehaviour
{
    public string url = "https://ows.emodnet-bathymetry.eu/wms?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap&BBOX=-10,45,-9,46&CRS=EPSG:4326&WIDTH=256&HEIGHT=256&LAYERS=emodnet:mean_atlas_land&STYLES=&FORMAT=image/png"; // URL для запроса
    public float meshScale = 10f;  // Масштаб глубины
    public int mapResolution = 256;  // Разрешение карты (256x256 пикселей)
    public float depthMultiplier = 0.5f;  // Множитель для глубины (чтобы подкорректировать масштаб)
    public ShipMarker SM;
    private MeshFilter meshFilter;
    private Renderer meshRenderer;
    public RawImage rawImage; // RawImage компонент для отображения карты глубин

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<Renderer>();

        // Получаем GPS-позицию
        float lat = transform.position.x; // Пример: используем X как широту
        float lon = transform.position.z; // Пример: используем Z как долготу
        
        // Строим BBOX для запроса
        double delta = 0.001; // 100 метров в градусах
        string bbox = "-0.001792499, 0.001792501, 0.001792499, 0.001792501";
        // Строим URL с актуальными координатами
        string requestUrl = url.Replace("{bbox}", bbox);

        // Начинаем загрузку карты
        StartCoroutine(DownloadAndProcessDepthMap( "https://ows.emodnet-bathymetry.eu/wms?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetMap&BBOX=" + (SM.longitude-1).ToString() + "," + (SM.latitude -1).ToString() + "," + (SM.longitude+1).ToString() + "," + (SM.latitude +1).ToString() +"&CRS=EPSG:4326&WIDTH=256&HEIGHT=256&LAYERS=emodnet:mean_atlas_land&STYLES=&FORMAT=image/png"));
    }

    IEnumerator DownloadAndProcessDepthMap(string requestUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(requestUrl);
        yield return www.SendWebRequest();

        Debug.Log(www.result);
        if (www.result == UnityWebRequest.Result.Success)
        {
            // Проверяем тип контента
            string contentType = www.GetResponseHeader("Content-Type");
            Debug.Log($"Content-Type: {contentType}");

            // Если это изображение, продолжаем обработку
            if (contentType.Contains("image"))
            {
                Texture2D depthTexture = DownloadHandlerTexture.GetContent(www);
                Debug.Log($"Texture Width: {depthTexture.width}, Height: {depthTexture.height}");
                rawImage.texture = depthTexture;

                // Дополнительно можно настроить отображение
                rawImage.rectTransform.sizeDelta = new Vector2(depthTexture.width, depthTexture.height);

                ProcessDepthMap(depthTexture);
            }
            else
            {
                Debug.LogError($"Expected an image, but got: {contentType}");
                Debug.LogError($"Response Body: {www.downloadHandler.text}");
            }
        }
        else
        {
            Debug.LogError($"Failed to load depth map. Error: {www.error}");
            Debug.LogError($"Response Code: {www.responseCode}");
            Debug.LogError($"URL: {requestUrl}");
        }
    }
    void ProcessDepthMap(Texture2D depthTexture)
    {
        // Получаем пиксели карты глубин
        Color[] pixels = depthTexture.GetPixels();

        // Создаем массив вершин и треугольников для меша
        Vector3[] vertices = new Vector3[mapResolution * mapResolution];
        int[] triangles = new int[(mapResolution - 1) * (mapResolution - 1) * 6];

        // Генерация вершин
        for (int y = 0; y < mapResolution; y++)
        {
            for (int x = 0; x < mapResolution; x++)
            {
                // Индекс пикселя
                int index = y * mapResolution + x;

                // Получаем цвет пикселя (глубина зависит от яркости пикселя)
                Color pixelColor = pixels[index];
                float depth = pixelColor.grayscale * depthMultiplier; // Глубина (черный = 0, белый = максимальная глубина)

                // Располагаем вершины в сетке (высота = depth)
                float xPos = x * meshScale;
                float zPos = y * meshScale;
                float yPos = -depth; // Отрицательная высота для глубины

                vertices[index] = new Vector3(xPos, yPos, zPos);
            }
        }

        // Генерация треугольников для меша (каждый квадрат состоит из 2 треугольников)
        int triangleIndex = 0;
        for (int y = 0; y < mapResolution - 1; y++)
        {
            for (int x = 0; x < mapResolution - 1; x++)
            {
                int topLeft = y * mapResolution + x;
                int topRight = y * mapResolution + (x + 1);
                int bottomLeft = (y + 1) * mapResolution + x;
                int bottomRight = (y + 1) * mapResolution + (x + 1);

                // Первый треугольник
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topRight;

                // Второй треугольник
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = bottomRight;
            }
        }

        // Создаем меш
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Применяем нормали
        mesh.RecalculateNormals();

        // Применяем меш к объекту
        meshFilter.mesh = mesh;
    }
}
