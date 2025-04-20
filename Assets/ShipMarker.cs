using UnityEngine;

public class ShipMarker : MonoBehaviour
{
    [Header("Ship Coordinates")]
    public double latitude = 45.0;
    public double longitude = 30.0;

    [Header("Tile Map")]
    public TileLoader tileLoader; // ссылка на карту

    void Update()
    {
        if (tileLoader != null)
        {
            Vector3 pos = tileLoader.LatLonToWorld(latitude, longitude);
            transform.position = pos + new Vector3(0, 0.05f, 0); // немного выше тайлов
        }
    }
}