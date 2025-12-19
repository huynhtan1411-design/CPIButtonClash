using UnityEngine;
using DG.Tweening;

public class ConveyorBeltAnimation : MonoBehaviour
{
    [Header("Conveyor Belt Settings")]
    [SerializeField] private RectTransform image1; // Image đầu tiên
    [SerializeField] private RectTransform image2; // Image thứ hai
    [SerializeField] private float moveSpeed = 180; // Tốc độ di chuyển
    [SerializeField] private float loopOffset = 670f; // Khoảng cách lặp lại (kích thước chiều ngang của image)
    [SerializeField] private MergeCardHeros mergeCardHeros; // Tham chiếu đến MergeCardHeros để kiểm tra trạng thái thẻ
    private float conveyorMoveSpeed;
    private bool isPaused = false;

    void Start()
    {
        float maxDuration = 4f;
        conveyorMoveSpeed = Vector2.Distance(mergeCardHeros.pointInit.anchoredPosition, mergeCardHeros.pointEnd.anchoredPosition) / maxDuration;
        StartConveyorBelt();
    }

    void Update()
    {
        if (mergeCardHeros != null && mergeCardHeros.Cards.Count > 0)
        {
            bool allCardsStopped = mergeCardHeros.Cards.TrueForAll(card =>
            {
                RectTransform cardRect = card.GetComponent<RectTransform>();
                return DOTween.IsTweening(cardRect) == false;
            });

            //if (allCardsStopped && !isPaused)
            //{
            //    PauseConveyorBelt();
            //}
            //else if (!allCardsStopped && isPaused)
            //{
            //    ResumeConveyorBelt();
            //}
        }
    }

    private void StartConveyorBelt()
    {
        image1.DOAnchorPosX(-loopOffset, loopOffset / conveyorMoveSpeed)
              .SetEase(Ease.Linear)
              .SetLoops(-1, LoopType.Restart);

        image2.DOAnchorPosX(-loopOffset, loopOffset / conveyorMoveSpeed)
              .SetEase(Ease.Linear)
              .SetLoops(-1, LoopType.Restart);
    }

    private void PauseConveyorBelt()
    {
        isPaused = true;
        DOTween.Pause(image1);
        DOTween.Pause(image2);
    }

    private void ResumeConveyorBelt()
    {
        isPaused = false;
        DOTween.Play(image1);
        DOTween.Play(image2);
    }
}