using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class HexGridLayout : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rings = 1;

    [Header("Tile Settings")]
    [SerializeField] private HexData hexData;

    private void LayoutGrid()
    {
        //Center
        CreateHex(transform.position);
        
        for (int i = 0; i < rings; i++)
        {
            GenerateRing(i);
        }
    }

    private GameObject CreateHex(Vector3 position, string name = "Hex")
    {
        GameObject tile = new GameObject(name, typeof(HexRenderer));
        tile.transform.position = position;

        HexRenderer hexRenderer = tile.GetComponent<HexRenderer>();
        hexRenderer.InitializeMesh(hexData);

        tile.transform.SetParent(transform, true);
        return tile;
    }

    private void GenerateRing(int ringIndex)
    {
        int hexagonsInRing = 6 * ringIndex;
        for (int i = 0; i < hexagonsInRing; i++)
        {
            Vector3 position = transform.position;
            position.x += hexData.outerRadius * ringIndex * Mathf.Sqrt(3);

            GameObject hex = CreateHex(position, $"Hex {i}");
            Debug.Log(ringIndex % hexagonsInRing, hex);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            for (int i = transform.childCount - 1; i > -1; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            LayoutGrid();
        }
    }
}
