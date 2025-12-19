using System.Collections.Generic;
using UnityEngine;

namespace CLHoma
{
    public class EnvironmentTextureController : MonoSingleton<EnvironmentTextureController>
    {
        [System.Serializable]
        public class TextureData
        {
            public Texture2D texture;
            public int chapterIndex;
        }

        [Header("Renderer Settings")]
        [SerializeField] private Renderer targetRenderer;

        [Header("Texture Settings")]
        [SerializeField] private List<TextureData> textures = new List<TextureData>();

        private MaterialPropertyBlock propertyBlock;
        private TextureData currentTexture;

        public void Initialize()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }

            propertyBlock = new MaterialPropertyBlock();

            Material currentMaterial = targetRenderer.sharedMaterial;
            if (currentMaterial != null)
            {
                Texture2D currentTex = currentMaterial.GetTexture("_BaseMap") as Texture2D;
                if (currentTex == null)
                {
                    currentTex = currentMaterial.GetTexture("_MainTex") as Texture2D;
                }

                currentTexture = textures.Find(x => x.texture == currentTex);
            }
        }

        public void ChangeTextureByChapter(int chapterIndex)
        {
            TextureData newTexture = textures.Find(x => x.chapterIndex == chapterIndex);

            if (newTexture == null)
            {
                int i = chapterIndex % 3;
                newTexture = textures.Find(x => x.chapterIndex == i);
            }
            if (newTexture == null)
                newTexture = Utils.GetRandomFromList(textures.ToArray());
            if (newTexture == currentTexture) return;

            currentTexture = newTexture;
            ApplyTexture(currentTexture);
        }

        private void ApplyTexture(TextureData textureData)
        {
            if (textureData == null) return;

            targetRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetTexture("_BaseMap", textureData.texture);
            propertyBlock.SetTexture("_MainTex", textureData.texture);

            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        private void OnValidate()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }
        }
    }
}