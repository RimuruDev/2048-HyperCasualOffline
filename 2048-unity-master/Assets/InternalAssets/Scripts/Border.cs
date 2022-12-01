using System;
using UnityEngine;

[Serializable]
public struct Border
{
    [Range(0, 100), SerializeField] public float Offset;    // = 0.05f;
    [Range(0, 100), SerializeField] public float Spacing;   // = 0.1f;
}
