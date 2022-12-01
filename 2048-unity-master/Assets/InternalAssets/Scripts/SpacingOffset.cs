using System;
using UnityEngine;

[Serializable]
public struct SpacingOffset
{
    [Range(-100, 100), SerializeField] public float Horizontal; // = -1.65f;
    [Range(-100, 100), SerializeField] public float Vertical; // = 1.65f;
}
