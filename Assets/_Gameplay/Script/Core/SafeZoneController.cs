using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLHoma;
using AmazingAssets.DynamicRadialMasks;
using System;
using Cinemachine;
public class SafeZoneController : MonoSingleton<SafeZoneController>
{
    public Action OnZoneLevelChanged;
    public Action OnRadiusAnimationComplete;
    [SerializeField] private SafeZoneConfig safeZoneConfig;
    [SerializeField] private int currentZoneLevel = 1;
    [SerializeField] private DRMGameObject safeZoneVisual;
    [SerializeField] private GameObject lightInNight;
    [SerializeField] private float animationDuration = 1f; // Duration of the radius animation
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private Coroutine radiusAnimationCoroutine;




    public int CurrentZoneLevel => currentZoneLevel;
    void Start()
    {
        if (safeZoneConfig == null)
        {
            return;
        }
        UpdateSafeZone();

        WD.GameManager.Instance.OnCombatPhaseStart.AddListener(delegate { ToggleLightInNight(true); });
        WD.GameManager.Instance.OnBuildPhaseStart.AddListener(delegate { ToggleLightInNight(false); });
    }

    public void UpdateSafeZone()
    {
        float newRadius = safeZoneConfig.GetSafeZoneRadius(currentZoneLevel);
        UpdateZoneVisual(15);
    }

    private void UpdateZoneVisual(float targetRadius)
    {
        if (safeZoneVisual != null)
        {
            if (radiusAnimationCoroutine != null)
            {
                StopCoroutine(radiusAnimationCoroutine);
            }
            if (virtualCamera != null)
                virtualCamera.gameObject.SetActive(true);
            radiusAnimationCoroutine = StartCoroutine(AnimateRadius(safeZoneVisual.radius, targetRadius, () => {
                OnRadiusAnimationComplete?.Invoke();
            }));
            OnZoneLevelChanged?.Invoke();
        }
    }

    private IEnumerator AnimateRadius(float startRadius, float endRadius, Action onComplete = null)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            float currentRadius = Mathf.Lerp(startRadius, endRadius, t);
            safeZoneVisual.radius = currentRadius;
            
            yield return null;
        }
        
        safeZoneVisual.radius = endRadius;
        radiusAnimationCoroutine = null;
        onComplete?.Invoke();
        yield return new WaitForSeconds(0.3f); // Small delay to ensure the visual update is complete

        if (virtualCamera != null)
            virtualCamera.gameObject.SetActive(false);
    }

    public int GetCurrentZoneLevel()
    {
        return currentZoneLevel;
    }

    public void UpgradeZoneLevel()
    {
        currentZoneLevel++;
        UpdateSafeZone();
        Audio_Manager.instance.play("sfx_land_expanding");
    }

    public static bool IsPointInSafeZone(Vector3 point)
    {
        if (Instance.safeZoneVisual == null) return false;
        
        Vector3 safeZoneCenter = Instance.safeZoneVisual.transform.position;
        
        float distance = Vector2.Distance(
            new Vector2(point.x, point.z),
            new Vector2(safeZoneCenter.x, safeZoneCenter.z)
        );
        
        return distance <= (Instance.safeZoneVisual.radius  + 0.6f);
    }

    public void ResetSafeZone()
    {
        UpdateSafeZone();
    }

    private void ToggleLightInNight(bool value)
    {
        DG.Tweening.DOVirtual.DelayedCall(.4f, delegate
        {
            lightInNight.SetActive(value);
        });
    }
}
