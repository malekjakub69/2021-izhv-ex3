using System;
using Unity.Entities;

/// <summary>
/// Data for entities with the ability to move.
/// </summary>
[Serializable]
public struct CMove: IComponentData
{
	/// <summary>
	/// Current movement speed.
	/// </summary>
	public float speed;
}

