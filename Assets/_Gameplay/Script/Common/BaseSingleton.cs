using UnityEngine;

public abstract class Singleton<T> where T : new()
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }
}

public class ManualSingletonMono<T> : MonoBehaviour where T : MonoBehaviour
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
                Debug.LogWarning("Cannot find Object with type " + typeof(T));
            }

            return _instance;
        }
    }

    public static bool IsInstanceValid()
    {
        return (_instance != null);
    }

    //MUST OVERRIDE AWAKE AT CHILD CLASS
    public virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("Already has intsance of " + typeof(T));
            GameObject.Destroy(this);
            return;
        }

        if (_instance == null)
            _instance = (T)(MonoBehaviour)this;

        if (_instance == null)
        {
            Debug.LogError("Awake xong van NULL " + typeof(T));
        }
        //Debug.LogError("Awake of " + typeof(T));
    }

    protected virtual void OnDestroy()
    {
        //self destroy?
        if (_instance == this)
        {
            _instance = null;
            //Debug.LogError ("OnDestroy " + typeof(T));
        }
    }


    private void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}

/// <summary>
/// Singleton for mono behavior object
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T singleton;

    public static bool IsInstanceValid() { return singleton != null; }

    void Clear()
    {
        gameObject.name = typeof(T).Name;
    }

    public static T Instance
    {
        get
        {
            if (SingletonMono<T>.singleton == null)
            {
                SingletonMono<T>.singleton = (T)FindObjectOfType(typeof(T));
                if (SingletonMono<T>.singleton == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "[@" + typeof(T).Name + "]";
                    SingletonMono<T>.singleton = obj.AddComponent<T>();
                }
            }

            return SingletonMono<T>.singleton;
        }
    }


}