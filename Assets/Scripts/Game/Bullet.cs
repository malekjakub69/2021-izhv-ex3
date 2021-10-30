using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main behavior script for the bullets.
/// </summary>
public class Bullet : MonoBehaviour
{
    /// <summary>
    /// Speed of the projectile.
    /// </summary>
    [ Header("Gameplay") ]
    public float speed = 10.0f;

    /// <summary>
    /// How long should the bullet exist (seconds).
    /// </summary>
    public float maxLifeTime = 10.0f;
    
    /// <summary>
    /// Rigid body of the projectile
    /// </summary>
    private Rigidbody mRigidBody;

    /// <summary>
    /// How long the bullet currently exists (seconds).
    /// </summary>
    private float mLifeTime = 0.0f;
    
    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        mRigidBody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Update called at fixed time delta.
    /// </summary>
    void FixedUpdate()
    {
        // Propel the projectile forward.
        mRigidBody.MovePosition(
            transform.position + 
            transform.forward * speed * Time.deltaTime
        );
        
        // Update its life-time.
        mLifeTime += Time.deltaTime;
        // Destroy it if we reach the limit.
        if (mLifeTime >= maxLifeTime)
        { DestroyBullet(); }
    }

    /// <summary>
    /// Triggered when a collision is detected.
    /// </summary>
    /// <param name="other">The other collidable.</param>
    private void OnTriggerEnter(Collider other)
    { DestroyBullet(); }

    /// <summary>
    /// Destroy this bullet.
    /// </summary>
    void DestroyBullet()
    { Destroy(gameObject); }
}
