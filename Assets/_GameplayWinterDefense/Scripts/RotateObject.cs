using UnityEngine;

namespace WD
{
    public class RotateObject : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);
        [SerializeField] private Space space = Space.Self;

        private void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, space);
        }
    }
}
