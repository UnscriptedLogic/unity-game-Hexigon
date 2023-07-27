using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HexGridLayout : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rings = 1;

    [Header("Tile Settings")]
    [SerializeField] private HexData hexData;

    private void LayoutGrid()
    {
        for (int i = 0; i < rings; i++)
        {
            GameObject tile = new GameObject($"Hex", typeof(HexRenderer));

            tile.transform.position = GenerateRing(i);

            HexRenderer hexRenderer = tile.GetComponent<HexRenderer>();
            hexRenderer.InitializeMesh(hexData);

            tile.transform.SetParent(transform, true);
        }
    }

    private Vector3 GenerateRing(int ringIndex)
    {
        Vector3 position = transform.position;

        position.x += hexData.outerRadius * ringIndex * Mathf.Sqrt(3);

        return position;
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
