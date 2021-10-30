using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// System taking care of killing of entities whose lifetime ran out.
/// </summary>
public class SysTimedKill : SystemBase
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

    /// <summary>
    /// Main system update code.
    /// </summary>
    protected override void OnUpdate()
    {
        // Prepare the ECB for potentially destroyed entities.
        var ecb = mECBSystem.CreateCommandBuffer().AsParallelWriter();
        
        float deltaTime = Time.DeltaTime;
        
        // Run the timed kill code.
        Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref CTimed timed) => {
                timed.lifeTime -= deltaTime;
                if (timed.lifeTime <= 0.0f)
                { ecb.DestroyEntity(entityInQueryIndex, entity); }
            }).ScheduleParallel();
        
        // Provide the ECB system with our destruction jobs.
        mECBSystem.AddJobHandleForProducer(this.Dependency);
    }
}
