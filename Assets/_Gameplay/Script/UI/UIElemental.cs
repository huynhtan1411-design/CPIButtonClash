using CLHoma;
using UnityEngine;
using UnityEngine.UI;

public class UIElemental : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image elementIcon;
    [SerializeField] private ConfigElemental configElemental;

    public void Setup(ElementType elementType)
    {
        if (configElemental == null || elementIcon == null || rectTransform == null) return;

        if (elementType == ElementType.None)
        {
            elementIcon.enabled = false;
            rectTransform.gameObject.SetActive(false);
            return;
        }

        Sprite elementSprite = configElemental.GetElementSprite(elementType);
        if (elementSprite != null)
        {
            elementIcon.sprite = elementSprite;
            elementIcon.enabled = true;
            rectTransform.gameObject.SetActive(true);
        }
        else
        {
            elementIcon.enabled = false;
            rectTransform.gameObject.SetActive(false);
        }
    }
}