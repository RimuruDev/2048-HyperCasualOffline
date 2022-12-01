using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameDataContainer : MonoBehaviour
{
    [Header("Popups")]
    public Popup WinPopup;
    public Popup DefeatPopup;

    [Header("Prefabs")]
    public GameObject EmptyTile;
    public GameObject[] tilePrefabs;

    [Header("Texts")]
    public ScoreText ScoreText;
}
