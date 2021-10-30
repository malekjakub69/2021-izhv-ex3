using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System moving entities forward.
/// </summary>
public class SysMoveForward : SystemBase
{
    /// <summary>
    /// Main system update code.
    /// </summary>
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;

        Entities
            .WithAll<TMoveForward>()
            .ForEach((ref Translation translation, in Rotation rotation, in CMove move) => {
            translation.Value += math.mul(rotation.Value, new float3(0.0f, 0.0f, move.speed)) * deltaTime;
        }).ScheduleParallel();
    }
}
