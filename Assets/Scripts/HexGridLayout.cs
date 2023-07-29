using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridLayout : MonoBehaviour
{
    [System.Serializable]
    public class HexGridData
    {
        public HexData hexData;
        public List<List<Vector3>> rings = new List<List<Vector3>>();

        public void AddRing(List<Vector3> positions)
        {
            rings.Add(positions);
        }
    }

    [Header("Grid Settings")]
    [SerializeField] private bool refresh;
    [SerializeField] private int rings = 1;
    [SerializeField] private float gap = 0.1f;
    [SerializeField][Min(0)] private float spawnDelay = 0.25f;

    [Header("Placement Animation")]
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private Ease ease;

    [Header("Spawn Animation")]
    [SerializeField] private float scaleSpeed = 1f;
    [SerializeField] private Ease scaleEase;

    [Header("Tile Settings")]
    [SerializeField] private HexData hexData;

    private Coroutine spawnAnimation;

    private HexGridData LayoutGrid()
    {
        HexGridData grid = new HexGridData();

        //Center
        grid.AddRing(new List<Vector3>() { transform.position });
        
        for (int i = 0; i < rings; i++)
        {
            grid.AddRing(GenerateRing(i));
        }

        return grid;
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

    private List<Vector3> GenerateRing(int ringIndex)
    {
        List<Vector3> nodePositions = new List<Vector3>();

        if (ringIndex <= 0) return nodePositions;

        int hexagonsInRing = 6 * ringIndex;
        int nodesBeforeTurn = ringIndex % hexagonsInRing;

        float width = Mathf.Sqrt(3) * hexData.outerRadius;
        float height = 2 * hexData.outerRadius;

        Vector3 referenceAnchor = transform.position + new Vector3((width + gap) * ringIndex, 0f, 0f);
        nodePositions.Add(referenceAnchor);

        int turnIndex = 0;
        int sideIndex = 0;
        Vector3 offset = GetOffset(turnIndex, width, height);
        for (int i = 1; i < hexagonsInRing; i++)
        {
            nodePositions.Add(referenceAnchor + offset);
            referenceAnchor += offset;

            sideIndex++;
            if (sideIndex == nodesBeforeTurn)
            {
                turnIndex++;
                sideIndex = 0;
                offset = GetOffset(turnIndex, width, height);
            }   
        }

        return nodePositions;
    }

    private Vector3 GetOffset(int turn, float width, float height)
    {
        Vector3 offset = Vector3.zero;
        float halfWidth = width * 0.5f;
        float threeQuarterHeight = height * 0.75f;

        switch (turn)
        {
            case 0:
                offset.x -= halfWidth + (gap * 0.5f);
                offset.z += threeQuarterHeight + gap;
                break;

            case 1:
                offset.x -= width + gap;
                break;

            case 2:
                offset.x -= halfWidth + (gap * 0.5f);
                offset.z -= threeQuarterHeight + gap;
                break;

            case 3:
                offset.x += halfWidth + (gap * 0.5f);
                offset.z -= threeQuarterHeight + gap;
                break;

            case 4:
                offset.x += width + gap;
                break;

            case 5:
                offset.x += halfWidth + (gap * 0.5f);
                offset.z += threeQuarterHeight + gap;
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
            
            if (spawnAnimation != null)
                StopCoroutine(spawnAnimation);
            
            spawnAnimation = StartCoroutine(AnimateSpawn());
        }
    }

    private IEnumerator AnimateSpawn()
    {
        HexGridData hexGridData = LayoutGrid();
        for (int i = 0; i < hexGridData.rings.Count; i++)
        {
            for (int j = 0; j < hexGridData.rings[i].Count; j++)
            {
                Vector3 position = hexGridData.rings[i][j];
                GameObject hex = CreateHex(position);
                hex.transform.position += Vector3.up * heightOffset;
                hex.transform.localScale = Vector3.zero;
                hex.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                hex.transform.DOScale(Vector3.one, scaleSpeed).SetEase(scaleEase);
                hex.transform.DOMoveY(transform.position.y, speed).SetEase(ease);
                hex.transform.DORotate(Vector3.zero, .5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuint);
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
