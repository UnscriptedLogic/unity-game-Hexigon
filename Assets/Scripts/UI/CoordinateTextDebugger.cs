using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoordinateTextDebugger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coordinateTMP;

    public void Initialize(string coordinateText)
    {
        coordinateTMP.text = coordinateText;
    }
}
