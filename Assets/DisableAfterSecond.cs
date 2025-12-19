using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterSecond : MonoBehaviour
{
    public float disableAfterSeconds = 1f;
    private void OnEnable()
    {
        DG.Tweening.DOVirtual.DelayedCall(disableAfterSeconds, () =>
        {
            gameObject.SetActive(false);
        });
    }
}
