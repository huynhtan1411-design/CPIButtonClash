using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SphereCollider))]
public class ResourceItem : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float collectRadius = 3f;
    [SerializeField] public float flyDuration = 0.5f;
    [SerializeField] private float rotationSpeed = 180f; // Tốc độ xoay (độ/giây)
    [SerializeField] private float jumpPower = 2f; // Độ cao nhảy
    [SerializeField] private int numJumps = 1; // Số lần nhảy
    
    private Transform target;
    private bool isCollectable = false;
    private bool isMovingToTarget = false;
    private int amount = 1;
    private SphereCollider sphereCollider;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        timeOffset = Random.Range(0f, 100f);
        sphereCollider.radius = collectRadius;
    }

    public void Initialize(int resourceAmount)
    {
        amount = resourceAmount;
        isCollectable = true;
        isMovingToTarget = false;
        sphereCollider.enabled = true;
    }
    public void UpdateCollectRadius(int newRadius)
    {
        if (sphereCollider != null)
        {
            sphereCollider.radius = newRadius;
            collectRadius = newRadius;
        }
    }
public float hoverHeight = 0.005f;
public float hoverSpeed = 0.02f;

private Vector3 startPos;
private float timeOffset;
    private void OnEnable()
    {
        if (sphereCollider != null)
        {
            sphereCollider.enabled = false;
        }
        startPos = transform.position;
    }

    private void Update()
    {
        if (!isMovingToTarget)
        {
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
            // float yOffset = Mathf.Sin((Time.time + timeOffset) * hoverSpeed) * hoverHeight;
            // transform.position = startPos + Vector3.up * yOffset;
        }
    }

    public void MoveToTarget()
    {
        if (isMovingToTarget) return;
        
        isMovingToTarget = true;
        sphereCollider.enabled = false;
        
        transform.DOJump(target.position, jumpPower, numJumps, flyDuration)
            .SetEase(Ease.InQuad).Join(transform.DOScale(0.3f, flyDuration))
            .OnComplete(() => {
                ResourceManager.Instance.CollectResource(amount);
                ResourceManager.Instance.ReturnToPool(this);
            });

        Audio_Manager.instance.play("Fly");
    }

    public void SetTarget(Transform newTarget)
    {
        if (target == null)
        {
            target = newTarget;
            Debug.Log($"Resource target set to: {newTarget.name}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
} 