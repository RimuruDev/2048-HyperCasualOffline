using System;
using UnityEngine;

[Serializable]
public struct Score
{
    [Min(0), SerializeField] public int MaxScore;
    [Min(0), SerializeField] public int MinScore;
}
