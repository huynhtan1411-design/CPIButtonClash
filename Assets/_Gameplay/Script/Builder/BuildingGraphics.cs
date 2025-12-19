using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using CLHoma;
namespace WD
{
    public class BuildingGraphics : MonoBehaviour
    {
        [Header("Tower Graphics")]
        [SerializeField] protected Transform projectileSpawnPoint;
        [SerializeField] protected GameObject projectilePrefab;
        public Transform ProjectileSpawnPoint => projectileSpawnPoint;
        public GameObject ProjectilePrefab => projectilePrefab;

        [Header("Wall Graphics")]
        [SerializeField] protected List<GameObject> wallObjects = new List<GameObject>();

        [Header("Rotating Parts")]
        [SerializeField] protected List<Transform> rotatingParts = new List<Transform>();

        [Header("Rotation Settings")]
        private float minRotationAngle = 30f;
        private float maxRotationAngle = 90f;
        private float minRotationDuration = 1f;
        private float maxRotationDuration = 3f;
        private float attackRotationDuration = 0.1f;
        private Ease easeType = Ease.Linear;
        private float resumeDelay = 1f;

        [Header("Drop Parts")]
        [SerializeField] protected List<Transform> dropParts = new List<Transform>();
        private float dropDistance = 0.5f;
        private float dropDuration = 0.25f;
        private float dropDelay = 0.01f;
        private Ease dropEaseType = Ease.InOutSine;


        private List<Tween> activeTweens = new List<Tween>();
        private List<Coroutine> activeCoroutines = new List<Coroutine>();
        private bool isRotating = false;
        private bool isTrackingTarget = false;


        private void Start()
        {
            StartRotatingParts();
        }

        public void AnimationDropOfWall()
        {
            StartCoroutine(StartDropAnimation());
        }

        private  IEnumerator StartDropAnimation()
        {
            dropParts.ForEach(item => item.gameObject.SetActive(false));
            for (int i = 0; i < dropParts.Count; i++)
            {
                var part = dropParts[i];
                if (part != null)
                {
                    Vector3 originalPos = part.position;
                    part.position += Vector3.up * dropDistance;

                    part.gameObject.SetActive(true);
                    part.DOMove(originalPos, dropDuration)
                        .SetEase(dropEaseType);
                }
                yield return new WaitForSeconds(dropDelay);
            }
        }

        public void StartTrackingTarget(Vector3 direction, Action onComplete = null)
        {
            if (!isTrackingTarget)
            {
                StopAllRotations();
                isTrackingTarget = true;
            }

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            if (projectileSpawnPoint != null)
            {
                Vector3 currentRotation = projectileSpawnPoint.eulerAngles;
                projectileSpawnPoint.rotation = Quaternion.Euler(currentRotation.x, targetAngle, currentRotation.z);
            }

            foreach (var part in rotatingParts)
            {
                if (part != null)
                {
                    DOTween.Kill(part);

                    Vector3 currentRotation = part.eulerAngles;
                    part.DORotate(new Vector3(currentRotation.x, targetAngle, currentRotation.z), attackRotationDuration)
                        .SetEase(Ease.OutBack).OnComplete(delegate
                        {
                            onComplete?.Invoke();
                        });
                }
            }
        }

        public void LookAtTarget(Transform target, Action onComplete = null)
        {
            foreach (var part in rotatingParts)
            {
                if (part != null)
                {
                    //DOTween.Kill(part);

                    Vector3 currentRotation = part.eulerAngles;
                    part.LookAt(target);
                }
            }
        }

        public void StopAllRotations()
        {
            // Stop all coroutines
            foreach (var coroutine in activeCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            activeCoroutines.Clear();

            // Kill all tweens
            foreach (var tween in activeTweens)
            {
                if (tween != null && tween.IsPlaying())
                {
                    tween.Kill();
                }
            }
            activeTweens.Clear();

            // Kill any remaining tweens on rotating parts
            foreach (var part in rotatingParts)
            {
                if (part != null)
                {
                    DOTween.Kill(part);
                }
            }

            isRotating = false;
        }

        public void StopTrackingTarget()
        {
            isTrackingTarget = false;
            ResumeRotationWithDelay();
        }

        public void StartRotatingParts()
        {
            if (isRotating || isTrackingTarget) return;
            isRotating = true;

            foreach (var part in rotatingParts)
            {
                if (part != null)
                {
                    var coroutine = StartCoroutine(RotatePart(part));
                    activeCoroutines.Add(coroutine);
                }
            }
        }

        public void PauseRotation()
        {
            foreach (var tween in activeTweens)
            {
                if (tween != null && tween.IsPlaying())
                {
                    tween.Kill();
                }
            }
            activeTweens.Clear();
            isRotating = false;
        }

        public void ResumeRotationWithDelay()
        {
            if (isRotating || isTrackingTarget) return;
            StartCoroutine(ResumeAfterDelay());
        }

        private IEnumerator ResumeAfterDelay()
        {
            yield return new WaitForSeconds(resumeDelay);

            if (!isTrackingTarget)
            {
                StartRotatingParts();
            }
        }

        private IEnumerator RotatePart(Transform part)
        {
            while (true && !isTrackingTarget)
            {
                if (part == null)
                {
                    yield break;
                }

                float angle = UnityEngine.Random.Range(minRotationAngle, maxRotationAngle);
                if (UnityEngine.Random.value > 0.5f)
                {
                    angle = -angle;
                }
                float duration = UnityEngine.Random.Range(minRotationDuration, maxRotationDuration);
                Vector3 currentRotation = part.localEulerAngles;
                Vector3 targetRotation = currentRotation + new Vector3(0, angle, 0);

                Tween rotateTween = part.DOLocalRotate(targetRotation, duration, RotateMode.FastBeyond360)
                                       .SetEase(easeType);
                activeTweens.Add(rotateTween);

                yield return rotateTween.WaitForCompletion();

                activeTweens.Remove(rotateTween);

                if (isTrackingTarget)
                {
                    yield break;
                }
            }
        }

        private void OnDestroy()
        {
            StopAllRotations();
        }

        public void SetModel(int level)
        {
            int index = level - 1;
            if (index < 0 || index >= wallObjects.Count)
            {
                return;
            }
            for (int i = 0; i < wallObjects.Count; i++)
            {
                wallObjects[i].SetActive(i == index);
            }
        }
    }
}