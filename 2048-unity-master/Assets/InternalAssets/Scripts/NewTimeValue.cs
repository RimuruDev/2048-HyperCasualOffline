using UnityEngine;
using System;

[Serializable]
public struct NewTimeValue
{
    [Range(0, 100), SerializeField] public int Lowest;  // = 2;
    [Range(0, 100), SerializeField] public int Highes;  // = 4;
}
