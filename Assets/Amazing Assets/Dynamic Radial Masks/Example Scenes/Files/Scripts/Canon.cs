// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;


namespace AmazingAssets.DynamicRadialMasks.Examples
{
    public class Canon : MonoBehaviour
    {
        public GameObject prefab;
        public float impulseStrength = 1;

        void Update()
        {
            if (DRMInput.GetLeftMouseButtonDown())
            {
                Ray ray = Camera.main.ScreenPointToRay(DRMInput.GetMousePosition());


                GameObject go = GameObject.Instantiate(prefab);
                go.SetActive(true);
                go.hideFlags = HideFlags.HideInHierarchy;
                go.transform.position = ray.origin - Vector3.up;
                go.GetComponent<Rigidbody>().AddForce(ray.direction.normalized * impulseStrength, ForceMode.Impulse);
            }
        }
    }
}
