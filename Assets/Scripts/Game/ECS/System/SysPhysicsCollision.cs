using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System used for the "real" physics-based collision detection.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class SysPhysicsCollision : JobComponentSystem
{
    /// <summary>
    /// World where the physical entities are created.
    /// </summary>
    private BuildPhysicsWorld mBuildPhysicsWorld;

    /// <summary>
    /// World where the physical entities are simulated.
    /// </summary>
    private StepPhysicsWorld mStepPhysicsWorld;
    
    /// <summary>
    /// Our main collision detection and resolution job.
    /// </summary>
    [BurstCompile]
    struct SysPhysicsCollisionJob : ICollisionEventsJob
    {
        /// <summary>
        /// Group of all entities we consider bullets.
        /// </summary>
        [ReadOnly] public ComponentDataFromEntity<TBullet> bulletColliderGroup;
        /// <summary>
        /// Group of all entities we consider enemies.
        /// </summary>
        [ReadOnly] public ComponentDataFromEntity<TEnemy> enemyColliderGroup;
        /// <summary>
        /// Group of all entities we consider players.
        /// </summary>
        [ReadOnly] public ComponentDataFromEntity<TPlayer> playerColliderGroup;

        /// <summary>
        /// Group of entities with health, which we are going to modify.
        /// </summary>
        public ComponentDataFromEntity<CHealth> healthGroup;

        /// <summary>
        /// Group of entities with timed life, which we are going to modify.
        /// </summary>
        public ComponentDataFromEntity<CTimed> timedGroup;

        /// <summary>
        /// The main code for resolution of a collision.
        /// </summary>
        public void Execute(CollisionEvent collisionEvent)
        {
            // Collision event gives us the two entities that collided as EntityA and EntityB.
            
            // Since we will may get two collisions (A <-> B), we will use the EntityA as the active
            // instigator and EntityB as the receiver.
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;
            // Initialize our relationship.
            var instigator = entityA;
            var receiver = entityB;

            // Detect type collision types.
            var isBulletA = bulletColliderGroup.HasComponent(entityA);
            var isBulletB = bulletColliderGroup.HasComponent(entityB);
            var isEnemyA = enemyColliderGroup.HasComponent(entityA);
            var isEnemyB = enemyColliderGroup.HasComponent(entityB);
            var isPlayerA = playerColliderGroup.HasComponent(entityA);
            var isPlayerB = playerColliderGroup.HasComponent(entityB);
            
            // Reorder entities to match instigator and receiver - Just for easier understanding, do
            // not do this in production code since it may waste performance.
            if (isEnemyA && isBulletB)
            { instigator = entityA; receiver = entityB; }
            if (isPlayerA && isEnemyB)
            { instigator = entityB; receiver = entityA; }

            // Instigators may be either a bullet (hitting an enemy) or an enemy (hitting a player).
            var bulletImpact = bulletColliderGroup.HasComponent(instigator);
            var enemyImpact = enemyColliderGroup.HasComponent(instigator);
            
            // Receiver may be either an enemy (hit by a bullet) or a player (hit by an enemy).
            var hitEnemy = enemyColliderGroup.HasComponent(receiver);
            var hitPlayer = playerColliderGroup.HasComponent(receiver);
            
            // Note: This is a simplified implementation of collision -> damage.
            // In a more ECS fashion, we would not modify the health directly, bud add some component 
            // (perhaps called CDamage). Then, after all of the collisions were finished, we would apply 
            // the damage in some further system.

            if (bulletImpact)
            { // Bullet hit something -> Destroy it.
                // Copy & write component for performance.
                var modTime = timedGroup[instigator];
                modTime.lifeTime = 0.0f;
                timedGroup[instigator] = modTime;
            }

            if (bulletImpact && hitEnemy)
            { // Bullet hit an enemy -> Damage the enemy.
                // Copy & write component for performance.
                var modHealth = healthGroup[receiver];
                modHealth.current = 0.0f;
                healthGroup[receiver] = modHealth;
            }

            if (enemyImpact && hitPlayer)
            { // Enemy hit a player -> Damage the player.
                // Copy & write component for performance.
                var modHealth = healthGroup[receiver];
                modHealth.current -= 1.0f;
                healthGroup[receiver] = modHealth;
            }
        }
    }

    /// <summary>
    /// Initialization code for the system.
    /// </summary>
    protected override void OnCreate()
    {
        base.OnCreate();
        mBuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        mStepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }
    
    /// <summary>
    /// Main system update code.
    /// </summary>
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        // Schedule our collision job.
        var jobHandle = new SysPhysicsCollisionJob()
        {
            // Our read-only groups.
            bulletColliderGroup = GetComponentDataFromEntity<TBullet>(true), 
            enemyColliderGroup = GetComponentDataFromEntity<TEnemy>(true), 
            playerColliderGroup = GetComponentDataFromEntity<TPlayer>(true), 
            
            // Our read/write groups.
            healthGroup = GetComponentDataFromEntity<CHealth>(false), 
            timedGroup = GetComponentDataFromEntity<CTimed>(false)
        }.Schedule(
            mStepPhysicsWorld.Simulation, 
            ref mBuildPhysicsWorld.PhysicsWorld,
            inputDependencies
        );
        
        // Enqueue the job for execution.
        jobHandle.Complete();

        // Return our queue.
        return jobHandle;
    }
}
