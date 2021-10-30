using System;
using Unity.Entities;

/// <summary>
/// Data for entities with health.
/// </summary>
[Serializable]
public struct CHealth : IComponentData
{
    /// <summary>
    /// Current amount of health.
    /// </summary>
    public float current;
    
    /// <summary>
    /// Maximum amount of health.
    /// </summary>
    public float max;
}
