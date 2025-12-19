using UnityEngine;

public class Global : MonoBehaviour
{
    private static Global instance;
    private static bool isInitialized = false;
    [SerializeField] private Audio_Manager audio_Manager;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            //audio_Manager.Init();
        }
    }
}