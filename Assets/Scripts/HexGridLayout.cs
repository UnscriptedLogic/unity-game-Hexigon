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
        public List<List<Hex>> rings = new List<List<Hex>>();

        public void AddRing(List<Hex> positions)
        {
            rings.Add(positions);
        }
    }

    public class Hex 
    {
        public Vector3Int coordinate;
        public Vector3 position;
        public GameObject hexObject;
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

    [Header("Debug")]
    [SerializeField] private bool showCoords;
    [SerializeField] private float coordinateHeightOffset = 0.1f;
    [SerializeField] private GameObject coordinateDebugGO;

    private Coroutine spawnAnimation;

    private HexGridData LayoutGrid()
    {
        HexGridData grid = new HexGridData();

        //Center
        grid.AddRing(new List<Hex>() 
        { 
            new Hex() 
            { 
                position = transform.position
            } 
        });
        
        for (int i = 0; i < rings; i++)
        {
            grid.AddRing(GenerateRing(i));
        }

        return grid;
    }

    private GameObject CreateHex(Vector3 position, Vector3Int coords, string name = "Hex")
    {
        GameObject tile = new GameObject(name, typeof(HexRenderer));
        tile.transform.position = position;

        HexRenderer hexRenderer = tile.GetComponent<HexRenderer>();
        hexRenderer.InitializeMesh(hexData);

        if (showCoords)
        {
            GameObject coordinateUIobject = Instantiate(coordinateDebugGO, tile.transform);
            coordinateUIobject.transform.position = position + (Vector3.up * (coordinateHeightOffset + hexData.height * 0.5f));

            CoordinateTextDebugger coordinateTextDebugger = coordinateUIobject.GetComponent<CoordinateTextDebugger>();
            coordinateTextDebugger.Initialize($"{coords.x}, {coords.y}, {coords.z}");
        }

        tile.transform.SetParent(transform, true);
        return tile;
    }

    private List<Hex> GenerateRing(int ringIndex)
    {
        List<Hex> hexes = new List<Hex>();

        if (ringIndex <= 0) return hexes;

        int hexagonsInRing = 6 * ringIndex;
        int nodesBeforeTurn = ringIndex % hexagonsInRing;

        float width = Mathf.Sqrt(3) * hexData.outerRadius;
        float height = 2 * hexData.outerRadius;

        Vector3 referenceAnchor = transform.position + new Vector3((width + gap) * ringIndex, 0f, 0f);
        Vector3Int referenceCoord = new Vector3Int(-1, 1, 0) * ringIndex;
        Hex referenceHex = new Hex()
        {
            position = referenceAnchor,
            coordinate = referenceCoord
        };

        Hex rootHex = referenceHex;
        hexes.Add(rootHex);

        Vector3Int coordinateOffset;
        int turnIndex = 0;
        int sideIndex = 0;
        Vector3 offset = GetOffset(turnIndex, width, height, out coordinateOffset);
        for (int i = 1; i < hexagonsInRing; i++)
        {
            hexes.Add(new Hex()
            {
                position = referenceAnchor + offset,
                coordinate = referenceCoord + coordinateOffset
            });

            referenceAnchor += offset;
            referenceCoord += coordinateOffset;

            sideIndex++;
            if (sideIndex == nodesBeforeTurn)
            {
                turnIndex++;
                sideIndex = 0;
                offset = GetOffset(turnIndex, width, height, out coordinateOffset);
            }
        }

        return hexes;
    }

    private Vector3 GetOffset(int turn, float width, float height, out Vector3Int coordinateOffset)
    {
        Vector3Int coordOffset = Vector3Int.zero;
        Vector3 offset = Vector3.zero;
        float halfWidth = width * 0.5f;
        float threeQuarterHeight = height * 0.75f;

        switch (turn)
        {
            case 0:
                offset.x -= halfWidth + (gap * 0.5f);
                offset.z += threeQuarterHeight + gap;
                coordOffset.z--;
                coordOffset.x++;
                break;

            case 1:
                offset.x -= width + gap;
                coordOffset.x++;
                coordOffset.y--;
                break;

            case 2:
                offset.x -= halfWidth + (gap * 0.5f);
                offset.z -= threeQuarterHeight + gap;
                coordOffset.y--;
                coordOffset.z++;
                break;

            case 3:
                offset.x += halfWidth + (gap * 0.5f);
                offset.z -= threeQuarterHeight + gap;
                coordOffset.x--;
                coordOffset.z++;
                break;

            case 4:
                offset.x += width + gap;
                coordOffset.x--;
                coordOffset.y++;
                break;

            case 5:
                offset.x += halfWidth + (gap * 0.5f);
                offset.z += threeQuarterHeight + gap;
                coordOffset.y++;
                coordOffset.z--;
                break;

            default:
                offset = Vector3.zero;
                break;
        }

        coordinateOffset = coordOffset;
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
                Hex hex = hexGridData.rings[i][j];
                Vector3 position = hexGridData.rings[i][j].position;
                hex.hexObject = CreateHex(position, hex.coordinate);

                hex.hexObject.transform.position += Vector3.up * heightOffset;
                hex.hexObject.transform.localScale = Vector3.zero;
                hex.hexObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                hex.hexObject.transform.DOScale(Vector3.one, scaleSpeed).SetEase(scaleEase);
                hex.hexObject.transform.DOMoveY(transform.position.y, speed).SetEase(ease);
                hex.hexObject.transform.DORotate(Vector3.zero, .5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuint);
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
