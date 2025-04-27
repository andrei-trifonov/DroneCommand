using System;
using UnityEngine;

public class ShipMarker : MonoBehaviour
{
    [Header("Ship Coordinates")]
    public double latitude = 45.0;
    public double longitude = 30.0;
    public Transform shipModel;
    [Header("Tile Map")]
    public TileLoader tileLoader; // ссылка на карту

    
    
    private Vector3 startPosition;
    private Vector3 Rotation;
    private Vector3 pos;
    private float startLat;
    private float startLon;
    
    private void Start()
    {
       pos = tileLoader.LatLonToWorld( latitude, longitude);
        transform.position = pos;
        startPosition = shipModel.transform.position;
    }

    void Update()
    {
        if (tileLoader != null)
        {


            pos = tileLoader.LatLonToWorld(latitude , longitude);
            transform.position = pos;
            transform.position += (shipModel.transform.position - startPosition)/1000;
            transform.eulerAngles = new Vector3(90, 0, -shipModel.transform.eulerAngles.y);
            transform.position = new Vector3(transform.position.x, 0.02f, transform.position.z);

        }
    }
}