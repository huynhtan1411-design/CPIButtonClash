using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class CameraToggle
    {
        public GameObject cameraObject;
        public float toggleTime;
    }

    public List<CameraToggle> cameraToggles = new List<CameraToggle>();

    private void Start()
    {
        StartCoroutine(SwitchCamerasRoutine());
    }

    private IEnumerator SwitchCamerasRoutine()
    {
        foreach (var cameraToggle in cameraToggles)
        {
            yield return new WaitForSeconds(cameraToggle.toggleTime);
            if (cameraToggle.cameraObject != null)
            {
                cameraToggle.cameraObject.SetActive(true);
            }


        }
    }
}