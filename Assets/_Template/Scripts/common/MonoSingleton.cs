using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static bool _applicationIsQuitting;
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
                return null;
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                //if (_instance == null)
                //{
                //    GameObject newGo = new GameObject();
                //    newGo.name = "TInstance";
                //    _instance = newGo.AddComponent<T>();
                //    DontDestroyOnLoad(newGo);
                //}
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}