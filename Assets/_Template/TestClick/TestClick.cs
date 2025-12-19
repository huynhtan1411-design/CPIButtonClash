using UnityEngine;
#if TTP_CORE
using Tabtale.TTPlugins;
#endif
public class TestClick : MonoBehaviour
{
    private void Awake()
    {
#if TTP_CORE
       TTPCore.Setup();      
#endif
    }
}

