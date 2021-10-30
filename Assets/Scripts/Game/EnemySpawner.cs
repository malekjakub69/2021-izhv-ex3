using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Main behavior script for the enemy spawners.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    /// <summary>
    /// Prefab used for the enemies.
    /// </summary>
    [ Header("Global") ]
    public GameObject enemyPrefab;
    
    /// <summary>
    /// Delay between spawning enemies.
    /// </summary>
    [ Header("Gameplay") ]
    public float spawnDelay = 1.0f;
    
    /// <summary>
    /// Is the spawner operating?
    /// </summary>
    public bool spawnEnabled = true;
    
    /// <summary>
    /// Time accumulator representing time before next shot in seconds.
    /// </summary>
    private float mCoolDown = 0.0f;
    
    /// <summary>
    /// Are we using the ECS to spawn the bullets?
    /// </summary>
    private bool mUseECS = false;

    /// <summary>
    /// Entity manager used for the spawning.
    /// </summary>
    private EntityManager mEntityManager;

    /// <summary>
    /// Storage for our blobs.
    /// </summary>
    private BlobAssetStore mBlobAssetStore;
    
    /// <summary>
    /// Entity prefab used as a base for each entity.
    /// </summary>
    private Entity mEnemyEntityPrefab;

    /// <summary>
    /// Called when the script instance is first loaded.
    /// </summary>
    private void Awake()
    { }

    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        mUseECS = Settings.Instance.useECS;
        if (mUseECS)
        { // Use the complete conversion workflow.
            // Get the entity manager for the main world.
            mEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Convert the Enemy GameObject Prefab to Entity.
            mBlobAssetStore = new BlobAssetStore();
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(
                World.DefaultGameObjectInjectionWorld, mBlobAssetStore
            );
            mEnemyEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                enemyPrefab, settings
            );
#if UNITY_EDITOR
            if (Application.isEditor)
            { mEntityManager.SetName(mEnemyEntityPrefab, "Enemy"); }
#endif // UNITY_EDITOR
        }
    }
    
    /// <summary>
    /// Cleanup when this script is destroyed.
    /// </summary>
    private void OnDestroy()
    { mBlobAssetStore?.Dispose(); }

    /// <summary>
    /// Update called at fixed time delta.
    /// </summary>
    void FixedUpdate()
    {
        if (spawnEnabled)
        {
            // Cool down the spawner by the elapsed time.
            mCoolDown -= Time.deltaTime;

            while (mCoolDown <= 0.0f)
            { // Spawn corresponding number of bullets.
                SpawnEnemy(transform.position);
                
                // "Heat up" the gun.
                mCoolDown += spawnDelay;
            }
        }
    }
    
    /// <summary>
    /// Spawn a single enemy at the provided position.
    /// </summary>
    /// <param name="position">Position to spawn the enemy at.</param>
    public void SpawnEnemy(Vector3 position)
    {
        if (mUseECS)
        { // Using ECS -> Spawn new entity.
            var enemy = mEntityManager.Instantiate(mEnemyEntityPrefab);
            mEntityManager.SetComponentData(enemy, new Translation{ Value = position });
        }
        else
        { // Using default -> Spawn new GameObject.
            // Instantiate the bullet and set it up.
            var enemy = Instantiate(enemyPrefab);
            enemy.transform.position = position;
        }
    }
}
