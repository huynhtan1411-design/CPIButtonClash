using UnityEngine;

namespace WD
{
    public class CompanionFollower : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followDistance = 1.5f;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotateSpeed = 8f;
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private bool autoFindTargetByTag = true;
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParam = "Speed";

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Update()
        {
            if (autoFindTargetByTag && (target == null || !target.gameObject.activeInHierarchy))
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go != null) target = go.transform;
            }

            if (target == null) return;

            Vector3 toTarget = target.position - transform.position;
            float dist = toTarget.magnitude;
            float desiredSpeed = 0f;

            if (dist > followDistance)
            {
                Vector3 dir = toTarget.normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                desiredSpeed = moveSpeed;
            }

            if (toTarget.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(new Vector3(toTarget.x, 0, toTarget.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);
            }

            if (animator != null && !string.IsNullOrEmpty(speedParam))
            {
                animator.SetFloat(speedParam, desiredSpeed);
            }
        }
    }
}
