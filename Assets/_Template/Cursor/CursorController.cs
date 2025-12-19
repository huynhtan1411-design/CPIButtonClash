using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class CursorController : MonoBehaviour
{
    [Header("Cursor Settings")]
    public Texture2D cursorTexture;

    [Header("Scale Settings")]
    public float normalScale = 1.0f;
    public float scaleDuration = 0.1f;
    public float offsetX = 45f;

    private RectTransform cursorRect;
    private bool isClicked = false;
    private Vector2 _size = Vector2.one;

    private CanvasGroup _canvasGroup;
    void Start()
    {
        GameObject cursorUI = new GameObject("CustomCursor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        cursorUI.transform.SetParent(transform.parent.GetComponent<Canvas>().transform);

        cursorRect = cursorUI.GetComponent<RectTransform>();
        Image cursorImage = cursorUI.GetComponent<Image>();
        _canvasGroup = cursorUI.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        cursorImage.raycastTarget = false;

        cursorImage.sprite = Sprite.Create(cursorTexture, new Rect(0, 0, cursorTexture.width, cursorTexture.height), new Vector2(0.5f, 0.5f));
        cursorRect.sizeDelta = new Vector2(cursorTexture.width, cursorTexture.height) * normalScale;
        _size = cursorRect.sizeDelta;
    }

    void Update()
    {
        cursorRect.position = Input.mousePosition + new Vector3(_size.x / 2 - offsetX * normalScale * 10, -_size.y / 2);

        if (Input.GetMouseButtonDown(0) && !isClicked)
        {
            isClicked = true;
            ClickEffect();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            _canvasGroup.alpha = _canvasGroup.alpha > 0 ? 0 : 1;
        }
    }

    private void ClickEffect()
    {
        cursorRect.transform.DOPunchScale(Vector3.one * -.2f, .2f, 2).OnComplete(() =>
        {
            isClicked = false;
        });
    }
}
