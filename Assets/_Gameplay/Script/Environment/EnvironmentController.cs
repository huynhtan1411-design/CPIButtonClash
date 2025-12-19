using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
namespace WD
{
    public class EnvironmentController : MonoSingleton<EnvironmentController>
    {
        [SerializeField] private List<GameObject> gameObjects = new List<GameObject>();
        [SerializeField] private List<TreeInteractiveBehaviour> trees = new List<TreeInteractiveBehaviour>();

        [SerializeField] private Renderer[] grounds;
        [SerializeField] Color darkColor;
        [SerializeField] Color lightColor;
        private MaterialPropertyBlock propBlock;

        private void Awake()
        {
            propBlock = new MaterialPropertyBlock();
        }

        public void ToggleEnvironmentByLevelIndex(int levelIndex)
        {
            ResetAllTree();
            if (gameObjects.Count == 0)
            {
                return;
            }

            int actualIndex = ((levelIndex % gameObjects.Count) + gameObjects.Count) % gameObjects.Count;

            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (gameObjects[i] != null)
                {
                    gameObjects[i].SetActive(false);
                }
            }

            if (gameObjects[actualIndex] != null)
            {
                gameObjects[actualIndex].SetActive(true);
            }
        }

        public void ResetAllTree()
        {
            foreach (var tree in trees)
            {
                tree.Init();
            }
        }

        public void ChangeColorMaterialGroundToDark()
        {
            foreach (var ground in grounds)
            {
                if (ground != null)
                {
                    DOTween.To(() => ground.GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor"),
                        color => {
                            propBlock.SetColor("_BaseColor", color);
                            ground.SetPropertyBlock(propBlock);
                        },
                        darkColor,
                        0.75f);
                }
            }
        }
        public void ChangeColorMaterialGroundToLight()
        {
            foreach (var ground in grounds)
            {
                if (ground != null)
                {
                    DOTween.To(() => ground.GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor"),
                        color => {
                            propBlock.SetColor("_BaseColor", color);
                            ground.SetPropertyBlock(propBlock);
                        },
                        lightColor,
                        0.75f);
                }
            }
        }
    }
}