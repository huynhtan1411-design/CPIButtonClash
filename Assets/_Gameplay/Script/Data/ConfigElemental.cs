using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLHoma
{
    [CreateAssetMenu(fileName = "ConfigElemental", menuName = "CLHoma/ConfigElemental")]
    public class ConfigElemental : ScriptableObject
    {
        [Serializable]
        public class ElementData
        {
            public ElementType elementType;
            public Sprite elementSprite;
            public Color elementColor = Color.white;
            [TextArea(2, 5)]
            public string description;
        }

        [Header("Element Configuration")]
        [SerializeField] private List<ElementData> elementData = new List<ElementData>();
        
        private Dictionary<ElementType, ElementData> elementDataMap;

        private void InitializeDictionary()
        {
            elementDataMap = new Dictionary<ElementType, ElementData>();
            
            foreach (var data in elementData)
            {
                if (!elementDataMap.ContainsKey(data.elementType))
                {
                    elementDataMap.Add(data.elementType, data);
                }
                else
                {
                    Debug.LogWarning($"Duplicate element type found: {data.elementType}");
                }
            }
        }

        public Sprite GetElementSprite(ElementType elementType)
        {
            if (elementDataMap == null)
            {
                InitializeDictionary();
            }

            if (elementDataMap.TryGetValue(elementType, out ElementData data))
            {
                return data.elementSprite;
            }

            Debug.LogWarning($"No sprite found for element type: {elementType}");
            return null;
        }

        public Color GetElementColor(ElementType elementType)
        {
            if (elementDataMap == null)
            {
                InitializeDictionary();
            }

            if (elementDataMap.TryGetValue(elementType, out ElementData data))
            {
                return data.elementColor;
            }

            Debug.LogWarning($"No color found for element type: {elementType}");
            return Color.white;
        }

        public string GetElementDescription(ElementType elementType)
        {
            if (elementDataMap == null)
            {
                InitializeDictionary();
            }

            if (elementDataMap.TryGetValue(elementType, out ElementData data))
            {
                return data.description;
            }

            return string.Empty;
        }

        public List<ElementData> GetAllElementData()
        {
            return new List<ElementData>(elementData);
        }
    }
}
