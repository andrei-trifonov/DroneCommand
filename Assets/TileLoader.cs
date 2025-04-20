using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TileLoader : MonoBehaviour
{
    public GameObject tilePrefab;
    public int zoom = 10;
    public int tileRangeX = 2;
    public int tileRangeY = 1;// радиус тайлов вокруг центра
    public double centerLat = 45.0;
    public double centerLon = 30.0;
    private Vector3 dragOrigin;
    private bool isDragging = false;
    private LRUCache<string, Texture2D> tileCache = new LRUCache<string, Texture2D>(200); // максимум 200 тайлов

    void Start()
    {
        LoadTiles();
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            int newZoom = zoom + (scroll > 0 ? 1 : -1);
            newZoom = Mathf.Clamp(newZoom, 3, 18); // пределы zoom

            if (newZoom != zoom)
            {
                zoom = newZoom;
                ClearTiles();
                LoadTiles();
                    //  MapGrid_.RedrawGrid();
            }
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - dragOrigin;
            dragOrigin = Input.mousePosition;

            // Преобразование смещения в изменение широты/долготы (примерно)
            double scale = 360.0 / (Mathf.Pow(2, zoom) * 256); // градусы/пиксель

            centerLon -= delta.x * scale;
            centerLat -= delta.y * scale;

            ClearTiles();
            LoadTiles();
          // MapGrid_.RedrawGrid();
        }
    }

    void ClearTiles()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }



    void LoadTiles()
    {
        int centerX = LonToTileX(centerLon, zoom);
        int centerY = LatToTileY(centerLat, zoom);
        

        for (int dx = -tileRangeX; dx <= tileRangeX; dx++)
        {
            for (int dy = -tileRangeY; dy <= tileRangeY; dy++)
            {
                int tileX = centerX + dx;
                int tileY = centerY + dy;

                Vector3 position = new Vector3(dx * 1.0f, 0, -dy * 1.0f); // перевёрнуто по Y
                GameObject tileOSeaM = Instantiate(tilePrefab, position, Quaternion.identity);
                GameObject tileOStreetM = Instantiate(tilePrefab, position, Quaternion.identity);
                tileOSeaM.transform.eulerAngles = new Vector3(90, 0, 0);
                tileOStreetM.transform.parent = this.transform;
                tileOStreetM.transform.eulerAngles = new Vector3(90, 0, 0);
                var position1 = tileOStreetM.transform.position;
                position1 = new Vector3(position1.x, -0.1f, position1.z);
                tileOStreetM.transform.position = position1;
                tileOSeaM.transform.parent = this.transform;
                StartCoroutine(LoadTileImage($"https://tiles.openseamap.org/seamark/{zoom}/{tileX}/{ tileY}.png", tileOSeaM, tileX, tileY, zoom));
                StartCoroutine(LoadTileImage($"https://tile.openstreetmap.org/{zoom}/{tileX}/{ tileY}.png", tileOStreetM, tileX, tileY, zoom));
            }
        }
        MapGrid grid = GetComponent<MapGrid>();
        if (grid != null)
        {
            grid.DrawGrid(zoom, centerLat, centerLon, tileRangeX, tileRangeY);
        }
    }

    IEnumerator LoadTileImage(string url, GameObject tile, int x, int y, int zoom)
    {
            if (tile == null) yield break;
            
            
            string key = $"{zoom}_{x}_{y}_{url.GetHashCode()}";

            if (tileCache.TryGet(key, out Texture2D cachedTexture))
            {
                tile.GetComponentInChildren<Renderer>().material.mainTexture = cachedTexture;
            }
            else
            {
            
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
                yield return www.SendWebRequest();
               
                if (www.result == UnityWebRequest.Result.Success)
                {
                    if (tile == null) yield break;
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    tile.GetComponent<Renderer>().material.mainTexture = texture;
                    tileCache.Add(key, texture);
                }
                else
                {
                    Debug.LogError("Tile load failed: " + www.error);

                }
            }
          


    }

    public int LonToTileX(double lon, int zoom)
    {
        return (int)((lon + 180.0) / 360.0 * (1 << zoom));
    }
    public float LonToX(double lon, int zoom)
    {
        return (float)((lon + 180.0) / 360.0 * (1 << zoom));
    }
    public float LatToY(double lat, int zoom)
    {
        double latRad = lat * Mathf.Deg2Rad;
        return (float)((1.0 - Mathf.Log(Mathf.Tan((float)latRad) + 1.0f / Mathf.Cos((float)latRad)) / Mathf.PI) / 2.0f * (1 << zoom));
    }

    public int LatToTileY(double lat, int zoom)
    {
        double latRad = lat * Mathf.Deg2Rad;
        return (int)((1.0 - Mathf.Log(Mathf.Tan((float)latRad) + 1.0f / Mathf.Cos((float)latRad)) / Mathf.PI) / 2.0f * (1 << zoom));
    }
    public Vector3 LatLonToWorld(double lat, double lon)
    {
        int centerX = LonToTileX(centerLon, zoom);
        int centerY = LatToTileY(centerLat, zoom);

        int x = LonToTileX(lon, zoom);
        int y = LatToTileY(lat, zoom);

        float dx = x - centerX;
        float dz = centerY - y; // перевёрнут по Z (Y на карте)

        return new Vector3(dx, 0, dz);
    }
    public Vector2 WorldToLatLon(Vector3 world)
    {
        int centerX = LonToTileX(centerLon, zoom);
        int centerY = LatToTileY(centerLat, zoom);

        int tileX = centerX + Mathf.RoundToInt(world.x);
        int tileY = centerY - Mathf.RoundToInt(world.z); // т.к. ты переворачивал по Z

        double lon = tileX / (double)(1 << zoom) * 360.0 - 180.0;

        double n = Math.PI - 2.0 * Math.PI * tileY / (double)(1 << zoom);
        double lat = Math.Atan(Math.Sinh(n)) * Mathf.Rad2Deg;

        return new Vector2((float)lat, (float)lon);
    }
}
