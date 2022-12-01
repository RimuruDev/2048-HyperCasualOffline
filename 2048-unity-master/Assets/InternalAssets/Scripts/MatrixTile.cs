using UnityEngine;

[System.Serializable]
public struct MatrixTile
{
    [Range(0, 100), SerializeField] public int Rows;
    [Range(0, 100), SerializeField] public int Cols;
}
