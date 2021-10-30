using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Util
{

public static class Common
{
    /// <summary>
    /// Get child of a GameObject by name.
    /// </summary>
    /// <param name="go">Target GameObject.</param>
    /// <param name="name">Target child name.</param>
    /// <returns>Returns the child or null if no child with such name exists.</returns>
    [CanBeNull]
    public static GameObject GetChildByName(GameObject go, string name) 
    {
        var childTransform = go.transform.Find(name);
        return childTransform == null ? null : childTransform.gameObject;
    }
    
    /// <summary>
    /// Get child of a GameObject by its associated script.
    /// </summary>
    /// <param name="go">Target GameObject.</param>
    /// <typeparam name="TScript">Target child script.</typeparam>
    /// <returns>Returns the first child which has requested script type or null if none exists.</returns>
    [CanBeNull]
    public static GameObject GetChildByScript<TScript>(GameObject go) 
    {
        foreach (Transform child in go.transform)
        {
            if (child.gameObject.GetComponent<TScript>() != null)
            { return child.gameObject; }
        }

        return null;
    }
    
    /// <summary>
    /// Get default layer mask for a target layer, as it is defined in the
    /// Layer Collision Matrix.
    /// </summary>
    /// <param name="targetLayer">Layer to check against.</param>
    /// <returns>Returns the layer mask.</returns>
    public static LayerMask GetLayerMaskFromMatrix(int targetLayer) 
    {
        var layerMask = 0;
        
        for (var iii = 0; iii < 32; ++iii)
        {
            if (!Physics.GetIgnoreLayerCollision(targetLayer, iii))
            { layerMask = layerMask | (1 << iii); }
        }
        
        return layerMask;
    }
}

} // namespace Common
