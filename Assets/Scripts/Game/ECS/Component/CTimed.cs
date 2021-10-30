using System;
using Unity.Entities;

/// <summary>
/// Component for timed entities.
/// </summary>
[Serializable]
public struct CTimed : IComponentData
{
    /// <summary>
    /// Currently remaining life-time.
    /// </summary>
    public float lifeTime;
}
