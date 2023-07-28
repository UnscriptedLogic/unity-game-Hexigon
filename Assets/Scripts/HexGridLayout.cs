using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
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
        if (ringIndex <= 0) return;

        int hexagonsInRing = 6 * ringIndex;
        int nodesBeforeTurn = ringIndex % hexagonsInRing;

        float width = Mathf.Sqrt(3) * (hexData.outerRadius * ringIndex);
        float height = 2 * (hexData.outerRadius * ringIndex);

        Vector3 referenceAnchor = transform.position + new Vector3(width * ringIndex, 0f, 0f);
        GameObject rootHex = CreateHex(referenceAnchor, "Root Hex");

        int turnIndex = 0;
        int sideIndex = 0;
        Vector3 offset = GetOffset(turnIndex, width, height);
        for (int i = 1; i < hexagonsInRing; i++)
        {
            GameObject hex = CreateHex(referenceAnchor + offset, $"Hex {i}");

            sideIndex++;
            if (sideIndex == nodesBeforeTurn)
            {
                turnIndex++;
                sideIndex = 0;
                offset = GetOffset(turnIndex, width, height);
                referenceAnchor = hex.transform.position;
            }
        }
    }

    private Vector3 GetOffset(int turn, float width, float height)
    {
        Vector3 offset = Vector3.zero;

        switch (turn)
        {
            case 0:
                offset.x -= width * 0.5f;
                offset.z += height * 0.75f;
                break;

            case 1:
                offset.x -= width;
                break;

            default:
                offset = Vector3.zero;
                break;
        }

        return offset;
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
