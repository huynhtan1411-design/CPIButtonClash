using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UILoading : MonoBehaviour
{
    [SerializeField] private Transform _left;
    [SerializeField] private Transform _right;
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private float _waitDuration = 1f;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Ease _easeType = Ease.OutBack;
    [SerializeField] private float _leftOffset = -500f;
    [SerializeField] private float _rightOffset = 500f;
    
    private Vector3 _leftInitialPosition;
    private Vector3 _rightInitialPosition;
    private Sequence _currentSequence;
    
    private void Awake()
    {
        if (_left != null)
        {
            _left.localPosition = new Vector3(_leftOffset, _left.localPosition.y, _left.localPosition.z);
            _leftInitialPosition = _left.localPosition;
        }
            
        if (_right != null)
        {
            _right.localPosition = new Vector3(_rightOffset, _right.localPosition.y, _right.localPosition.z);
            _rightInitialPosition = _right.localPosition;
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShowLoading();
        }
    }
    
    public void ShowLoading()
    {
        if (_currentSequence != null && _currentSequence.IsActive())
        {
            _currentSequence.Kill();
        }
        
        StartCoroutine(ShowLoadingRoutine());
    }
    
    private IEnumerator ShowLoadingRoutine()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        Vector3 leftTargetPosition = new Vector3(0, _leftInitialPosition.y, _leftInitialPosition.z);
        Vector3 rightTargetPosition = new Vector3(0, _rightInitialPosition.y, _rightInitialPosition.z);
        
        _currentSequence = DOTween.Sequence();
        
        if (_left != null)
        {
            _currentSequence.Join(_left.DOLocalMove(leftTargetPosition, _animationDuration).SetEase(_easeType));
        }
        
        if (_right != null)
        {
            _currentSequence.Join(_right.DOLocalMove(rightTargetPosition, _animationDuration).SetEase(_easeType));
        }
        
        _currentSequence.AppendInterval(_waitDuration);
        
        if (_left != null)
        {
            _currentSequence.Append(_left.DOLocalMove(_leftInitialPosition, _animationDuration).SetEase(_easeType));
        }
        
        if (_right != null)
        {
            _currentSequence.Join(_right.DOLocalMove(_rightInitialPosition, _animationDuration).SetEase(_easeType));
        }
        
        if (_canvasGroup != null)
        {
            _currentSequence.Join(_canvasGroup.DOFade(0, _animationDuration));
        }
        
        yield return _currentSequence.WaitForCompletion();
    }
}
