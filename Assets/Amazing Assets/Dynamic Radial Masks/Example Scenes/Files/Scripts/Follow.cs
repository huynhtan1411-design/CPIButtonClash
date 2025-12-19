// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;


namespace AmazingAssets.DynamicRadialMasks.Examples
{
    public class Follow : MonoBehaviour
    {
        public DRMController drmController;

        public float scale = 1;

        void Update()
        {
            Vector3 position = transform.position;
            position.y = 0;


            float maskValue = drmController.GetMaskValue(position);


            position.y = maskValue * scale;

            transform.position = position;
        }
    }
}
