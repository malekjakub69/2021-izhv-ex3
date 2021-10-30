using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

/// <summary>
/// The main Player script.
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// Camera following the Player character.
    /// </summary>
    [ Header("Global") ]
    public Camera mainCamera;
    
    /// <summary>
    /// Is this the primary Player for this game?
    /// </summary>
    public bool primaryPlayer = false;

    /// <summary>
    /// Current health of the Player.
    /// </summary>
    [ Header("Gameplay") ]
    public float health = 10.0f;
    
    /// <summary>
    /// Maximum health of the Player.
    /// </summary>
    public float maxHealth = 10.0f;
    
    /// <summary>
    /// Is the player invincible?
    /// </summary>
    public bool invincible = false;

    /// <summary>
    /// Delay between two damaging events in seconds.
    /// </summary>
    public float damageDelay = 0.7f;

    /// <summary>
    /// Current speed of the Player.
    /// </summary>
    public float speed = 1.0f;
    
    /// <summary>
    /// Main RigidBody of the player model.
    /// </summary>
    private Rigidbody mRigidBody;
    
    /// <summary>
    /// Currently held gun.
    /// </summary>
    private GameObject mGun;

    /// <summary>
    /// Is the Player currently dead?
    /// </summary>
    private bool mIsDead = false;

    /// <summary>
    /// Current cooldown before the next damage may be applied.
    /// </summary>
    private float mDamageCooldown = 0.0f;

    /// <summary>
    /// Input controller for this player.
    /// </summary>
    private PlayerInput mPlayerInput;
    
    /// <summary>
    /// Requested look position.
    /// </summary>
    private Vector2 mLookInput = Vector2.zero;
    
    /// <summary>
    /// Requested movement input.
    /// </summary>
    private Vector2 mMoveInput = Vector2.zero;
    
    /// <summary>
    /// Requested zoom input.
    /// </summary>
    private float mZoomInput = 0.0f;
    
    /// <summary>
    /// Requested bullet count input.
    /// </summary>
    private float mBulletCountInput = 0.0f;
    
    /// <summary>
    /// Requested shotgun spread input.
    /// </summary>
    private float mShotgunSpreadInput = 0.0f;
    
    /// <summary>
    /// Are we using the ECS to spawn the bullets?
    /// </summary>
    private bool mUseECS = false;

    /// <summary>
    /// Get index of this player within the all players list.
    /// </summary>
    public int playerIndex { get => mPlayerIndex; }

    /// <summary>
    /// Entity manager used for the spawning.
    /// </summary>
    private EntityManager mEntityManager;

    /// <summary>
    /// Storage for our blobs.
    /// </summary>
    private BlobAssetStore mBlobAssetStore;
    
    /// <summary>
    /// Entity corresponding to this Player.
    /// </summary>
    private Entity mPlayerEntity;

    /// <summary>
    /// Index of this player within the player list.
    /// </summary>
    private int mPlayerIndex;

    /// <summary>
    /// Called when the script instance is first loaded.
    /// </summary>
    private void Awake()
    {
        // Fetch the main camera if we did not receive any other.
        if (mainCamera == null)
        { mainCamera = Settings.Instance.mainCamera; }
        
        mRigidBody = GetComponent<Rigidbody>();
        mPlayerInput = GetComponent<PlayerInput>();
        
        // Find the currently held gun.
        mGun = Common.GetChildByScript<Gun>(gameObject);
    }

    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        // Register this player or get its index.
        mPlayerIndex = Settings.Instance.AddPlayer(gameObject);

        // Initialize the health display for this player.
        GameManager.Instance.DisplayHealth(mPlayerIndex, health, 0.0f, maxHealth);
        
        mUseECS = Settings.Instance.useECS;
        if (mUseECS)
        { // Use the complete conversion workflow.
            // Get the entity manager for the main world.
            mEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Convert this Player GameObject to Entity.
            mBlobAssetStore = new BlobAssetStore();
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(
                World.DefaultGameObjectInjectionWorld, mBlobAssetStore
            );
            mPlayerEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                gameObject, settings
            );
#if UNITY_EDITOR
            if (Application.isEditor)
            { mEntityManager.SetName(mPlayerEntity, "Player"); }
#endif // UNITY_EDITOR
            
            // Remove child Entities, which are not necessary for our use: 
            // (There is a simpler way by using the StopConvertToEntity Component, but this is more educative...)
            // First get all of the entities within the group -> List of parent and its children.
            var entityGroup = mEntityManager.GetBuffer<LinkedEntityGroup>(mPlayerEntity);
            // We will record the destroy commands in order to not invalidate our list.
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            // Destroy all entities which are not the parent.
            foreach (var entity in entityGroup)
            { if (entity.Value != mPlayerEntity) { ecb.DestroyEntity(entity.Value); } }
            // Execute the destroy commands.
            ecb.Playback(mEntityManager);
            // Dispose of the ECB, since it is not managed by the Garbage Collector.
            ecb.Dispose();
        }
    }
    
    // Input System Callbacks: 
    public void OnLook(InputAction.CallbackContext ctx)
    { mLookInput = ctx.ReadValue<Vector2>(); }
    public void OnMove(InputAction.CallbackContext ctx)
    { mMoveInput = ctx.ReadValue<Vector2>(); }
    public void OnZoom(InputAction.CallbackContext ctx)
    { if (ctx.started) { mZoomInput = ctx.ReadValue<float>() / -120.0f; } }
    public void OnToggleDevUI(InputAction.CallbackContext ctx)
    { GameManager.Instance.ToggleDevUI(); }
    public void OnBulletCount(InputAction.CallbackContext ctx)
    { if (ctx.started) { mBulletCountInput = ctx.ReadValue<float>() / 120.0f; } }
    public void OnShotgunSpread(InputAction.CallbackContext ctx)
    { mShotgunSpreadInput = ctx.ReadValue<float>() / 120.0f; }
    public void OnToggleShotgun(InputAction.CallbackContext ctx)
    { if (ctx.started && mGun != null) { mGun.GetComponent<Gun>().shotgun ^= true; } }
    public void OnToggleInvincible(InputAction.CallbackContext ctx)
    { if (ctx.started) { invincible ^= true; } }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    {
        // When the health reaches zero, we die...
        if (health <= 0.0f)
        { KillPlayer(); }
    }

    /// <summary>
    /// Update called at fixed time delta.
    /// </summary>
    void FixedUpdate()
    {
        if (mIsDead)
        { return; }

        // Alternative approaches: 
        // Input.GetButtonDown("A"), Input.GetButtonDown("D")
        // Input.GetAxis("Horizontal")
        // Keyboard.current.aKey.isPressed
        
        // Move the Player in the requested direction.
        MovePlayer(new Vector3{ 
            x = mMoveInput.x, y = 0.0f, z = mMoveInput.y
        });
        
        // Make the Player look in the target direction.
        var lookTarget = CalculateLookTarget();
        if (lookTarget != null)
        {
            // Rotate facing of the Player.
            RotatePlayer(lookTarget.Value);
            // Notify shooting script that we changed direction to face the target.
            GetComponent<PlayerShoot>().ChangeTarget(lookTarget.Value);
        }
        
        // Zoom the camera.
        if (primaryPlayer && mZoomInput != 0.0f)
        {
            // Convert the raw zoom to smoothed version.
            var smoothZoom = CalculateSmoothZoom(mZoomInput);
            // Zoom the view.
            mainCamera.GetComponent<CameraHelper>().ZoomView(smoothZoom); 
            // Update the zoom.
            mZoomInput = smoothZoom;
        }

        // Update the gun characteristics.
        UpdateGunFromInput();
        
        // Update the "invincibility frames"
        mDamageCooldown = Math.Max(mDamageCooldown - Time.deltaTime, 0.0f);
        
        // Update state with the corresponding Player entity.
        SynchronizeEntityState();
    }
    
    /// <summary>
    /// Triggered when a collision is detected.
    /// </summary>
    /// <param name="other">The other collidable.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        { DamagePlayer(1.0f); }
        if (other.CompareTag("Bullet"))
        { DamagePlayer(2.0f); }
    }
    
    /// <summary>
    /// Cleanup when this script is destroyed.
    /// </summary>
    private void OnDestroy()
    { mBlobAssetStore?.Dispose(); }

    /// <summary>
    /// Calculate the look target point from the current input.
    /// </summary>
    /// <returns>Returns a look target or null if no valid direction is detected.</returns>
    Vector3? CalculateLookTarget()
    {
        if (mPlayerInput.currentControlScheme == "M&K")
        { // Mouse and Keyboard -> Use the mouse pointer screen coordinates.
            var cameraRay = mainCamera.ScreenPointToRay(mLookInput);
            if (Physics.Raycast(cameraRay, out var hit, Settings.Instance.groundLayer))
            { return hit.point; }
            else
            { return null; }
        }
        else
        { // Controller -> Use rotation of the control stick.
            if (mLookInput.magnitude > 0.1f)
            {
                return transform.position + new Vector3{
                    x = mLookInput.x,
                    y = 0.0f,
                    z = mLookInput.y
                }.normalized * 5.0f;
            }
            else
            { return null; }
        }
    }

    /// <summary>
    /// Move the player in given direction, using the curent speed.
    /// </summary>
    /// <param name="direction">Movement direction.</param>
    public void MovePlayer(Vector3 direction)
    {
        var moveDelta = direction.normalized * speed * Time.deltaTime;
        
        RaycastHit hit;
        var hitCollider = Physics.Raycast(
            new Ray(
                transform.position,
                moveDelta.normalized),
            out hit,
            moveDelta.magnitude, 
            Common.GetLayerMaskFromMatrix(gameObject.layer)
        );
        
        if (hitCollider)
        { mRigidBody.MovePosition(hit.point); }
        else
        { mRigidBody.MovePosition(transform.position + moveDelta); }
    }
    
    /// <summary>
    /// Rotate the player to face the given position.
    /// </summary>
    /// <param name="target">Position to rotate towards.</param>
    public void RotatePlayer(Vector3 target)
    {
        // Get the direction vector.
        var playerToTarget = target - transform.position;
        
        // Allow only rotation in the XZ plane (play area).
        playerToTarget.y = 0.0f;
        playerToTarget.Normalize();
        
        // Rotate towards it.
        mRigidBody.MoveRotation(Quaternion.LookRotation(playerToTarget));
    }

    /// <summary>
    /// Damage the Player's health by given amount.
    /// </summary>
    /// <param name="damage">Damage received.</param>
    public void DamagePlayer(float damage)
    {
        // Check if we were recently damaged or invincible and quit early.
        if (mDamageCooldown > 0.0f || invincible)
        { return; }
        
        // Damage the player.
        health -= damage; 
        // Apply "invincibility frames".
        mDamageCooldown += damageDelay;
        // Update the UI.
        GameManager.Instance.DisplayHealth(mPlayerIndex, health, 0.0f, maxHealth);
    }

    /// <summary>
    /// Health the Player's health by given amount.
    /// </summary>
    /// <param name="heal">Damage received.</param>
    public void HealPlayer(float heal)
    {
        // Heal the player.
        health += heal;
        // Update the UI.
        GameManager.Instance.DisplayHealth(mPlayerIndex, health, 0.0f, maxHealth);
    }

    /// <summary>
    /// Kill the Player character.
    /// </summary>
    public void KillPlayer()
    {
        mIsDead = true;
        Settings.Instance.RemovePlayer(gameObject);
        if (mGun != null)
        { mGun.GetComponent<Gun>().fireEnabled = false; }
    }

    /// <summary>
    /// Does this Player live?
    /// </summary>
    /// <returns>Returns true if the Player is currently alive.</returns>
    public bool IsAlive()
    { return !mIsDead && health >= 0.0f; }

    /// <summary>
    /// Calculate smoothed version of the given zoom input.
    /// </summary>
    /// <param name="rawZoom">Raw zoom input <-1.0f, 1.0f></param>
    /// <returns>Returns smoothed version.</returns>
    private float CalculateSmoothZoom(float rawZoom)
    {
        var smoothZoom = rawZoom * Settings.Instance.zoomAttune;
        return Math.Abs(smoothZoom) < Settings.Instance.zoomCutoff ? 0.0f : smoothZoom;
    }

    /// <summary>
    /// Use the player input to update the properties of the currently held weapon.
    /// </summary>
    private void UpdateGunFromInput()
    {
        if (mGun == null)
        { mBulletCountInput = 0.0f; mShotgunSpreadInput = 0.0f; return; }
        
        // Update the bullet count.
        if (mBulletCountInput != 0.0f)
        {
            // Convert the raw zoom to smoothed version.
            var smoothBulletCount = CalculateSmoothZoom(mBulletCountInput);
            // Adjust the fire rate.
            mGun.GetComponent<Gun>().AdjustFireRate(smoothBulletCount);
            // Update the zoom.
            mBulletCountInput = smoothBulletCount;
        }
        
        // Update the shotgun spread.
        if (mShotgunSpreadInput != 0.0f)
        {
            // Adjust the spread size.
            mGun.GetComponent<Gun>().AdjustSpreadSize((int)Math.Floor(mShotgunSpreadInput));
            // Reset the input.
            mShotgunSpreadInput = 0.0f;
        }
    }

    /// <summary>
    /// Synchronize data between the Player entity and the Player GameObject.
    /// </summary>
    private void SynchronizeEntityState()
    {
        if (!mUseECS)
        { return; }
        
        // Set the current position and rotation.
        mEntityManager.SetComponentData(mPlayerEntity, 
            new Translation
            {
                Value = new float3(transform.position) 
            });
        mEntityManager.SetComponentData(mPlayerEntity, 
            new Rotation
            {
                Value = transform.rotation
            });
        
        // Check if we were damaged and apply the damage.
        var cHealth = mEntityManager.GetComponentData<CHealth>(mPlayerEntity);
        if (cHealth.current < health)
        { DamagePlayer(health - cHealth.current); }
        
        // Synchronize current health value.
        mEntityManager.SetComponentData(mPlayerEntity, new CHealth{ current = health, max = maxHealth });
    }
}
