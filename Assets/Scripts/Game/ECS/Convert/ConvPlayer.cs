using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Conversion behavior for the Player.
/// </summary>
[DisallowMultipleComponent]
public class ConvPlayer : MonoBehaviour, IConvertGameObjectToEntity
{
    /// <summary>
    /// Current health of the Player.
    /// </summary>
    public float? health;
    
    /// <summary>
    /// Maximum health of the Player.
    /// </summary>
    public float? maxHealth;

    /// <summary>
    /// Add all necessary components to the provided entity.
    /// </summary>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Get the health from the main Component if not provided.
        var current = health ?? GetComponent<Player>().health;
        var max = maxHealth ?? GetComponent<Player>().maxHealth;
        
        // Make the entity a Player.
        dstManager.AddComponent<TPlayer>(entity);
        dstManager.AddComponentData(entity, new CHealth { current = current, max = max });
    }
}
