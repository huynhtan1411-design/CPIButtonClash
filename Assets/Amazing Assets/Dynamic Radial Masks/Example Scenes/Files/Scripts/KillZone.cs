// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;


namespace AmazingAssets.DynamicRadialMasks.Examples
{
    public class KillZone : MonoBehaviour
    {
        public float groundLevel = 0;

        void FixedUpdate()
        {
            if (transform.position.y < groundLevel)
                GameObject.Destroy(this.gameObject);
        }
    }
}
