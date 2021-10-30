using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Conversion behavior for the bullet.
/// </summary>
[DisallowMultipleComponent]
public class ConvBullet : MonoBehaviour, IConvertGameObjectToEntity
{
    /// <summary>
    /// The speed of the bullet.
    /// </summary>
    public float speed;
    
    /// <summary>
    /// The lifetime of the bullet.
    /// </summary>
    public float lifeTime;

    /// <summary>
    /// Add all necessary components to the provided entity.
    /// </summary>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<TBullet>(entity);
        dstManager.AddComponent<TMoveForward>(entity);
        dstManager.AddComponentData(entity, new CMove{ speed = speed });
        dstManager.AddComponentData(entity, new CTimed{ lifeTime = lifeTime });
    }
}
