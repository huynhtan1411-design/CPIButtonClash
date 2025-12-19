#if UNITY_EDITOR
using UnityEngine;

public class CSVLoaderExample : MonoBehaviour
{
    [SerializeField] private TextAsset csvFile; 
    [SerializeField] private ScriptableObject heroDatabase; 

    void Start()
    {
        CSVLoaderUtility.LoadCSVToScriptableObject(csvFile, heroDatabase);
    }
}
#endif