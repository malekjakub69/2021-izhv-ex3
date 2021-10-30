using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

/// <summary>
/// Conversion behavior for the Enemy.
/// </summary>
[DisallowMultipleComponent]
public class ConvEnemy : MonoBehaviour, IConvertGameObjectToEntity
{
    /// <summary>
    /// Current health of the Enemy.
    /// </summary>
    public float health = 10.0f;

    /// <summary>
    /// Current speed of the Enemy.
    /// </summary>
    public float speed = 1.0f;

    /// <summary>
    /// Add all necessary components to the provided entity.
    /// </summary>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<TEnemy>(entity);
        dstManager.AddComponent<TMoveForward>(entity);
        dstManager.AddComponentData(entity, new CHealth { current = health, max = health });
        dstManager.AddComponentData(entity, new CMove { speed = speed });
    }
}
