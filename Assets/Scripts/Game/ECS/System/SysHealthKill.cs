using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// System taking care of killing of entities whose health ran out.
/// </summary>
public class SysHealthKill : SystemBase
{
    /// <summary>
    /// Entity Command Buffer system used for the destruction commands.
    /// </summary>
    EndSimulationEntityCommandBufferSystem mECBSystem;

    /// <summary>
    /// Initialization code for the system.
    /// </summary>
    protected override void OnCreate()
    {
        base.OnCreate();
        mECBSystem = World.DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        // Prepare the ECB for potentially destroyed entities.
        var ecb = mECBSystem.CreateCommandBuffer().AsParallelWriter();
        
        // Run the health-based kill code. Exclude players, since we manage their entities in the Player script.
        Entities
            .WithNone<TPlayer>()
            .ForEach((Entity entity, int entityInQueryIndex, ref CHealth health) => {
                if (health.current <= 0.0f)
                { ecb.DestroyEntity(entityInQueryIndex, entity); }
            }).ScheduleParallel();
        
        // Provide the ECB system with our destruction jobs.
        mECBSystem.AddJobHandleForProducer(this.Dependency);
    }
}
