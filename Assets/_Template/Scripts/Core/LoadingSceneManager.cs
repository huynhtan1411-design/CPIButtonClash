using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    public string targetScene;
    [SerializeField] Image backgroundImage;
    [SerializeField] List<Image> iconImages;
    [SerializeField] TextMeshProUGUI textLoading;
    [SerializeField] GameObject loadingObject;
    [SerializeField] CanvasScaler canvasScaler;
    public Slider progressBar;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        LoadScene();
    }

    public void LoadScene()
    {
        StartCoroutine(LoadSceneAsync(targetScene));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Target scene name is empty or null!");
            yield break;
        }

        float minLoadingTime = 2f;
        float startTime = Time.time;

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            while (operation.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f) * 0.5f;
                progressBar.value = progress;
                yield return null;
            }
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minLoadingTime)
            {
                while (progressBar.value < 0.5f)
                {
                    progressBar.value += 0.005f;
                    yield return new WaitForSeconds(0.01f);
                }
                yield return new WaitForSeconds(0.5f);
                while (progressBar.value < 0.95f)
                {
                    progressBar.value += 0.005f;
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }

        yield return new WaitForSeconds(0.3f);

        progressBar.value = 1f;
        operation.allowSceneActivation = true;

        loadingObject.SetActive(false);
        backgroundImage.DOFade(0.0f, 1f);
        textLoading.DOFade(0.0f, 1f);
        foreach (var image in iconImages)
        {
            image.DOFade(0.0f, 1f);
        }
    }
}