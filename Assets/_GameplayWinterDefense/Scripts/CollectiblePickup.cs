using UnityEngine;
using CLHoma.Combat;

namespace WD
{
    [RequireComponent(typeof(Collider))]
    public class CollectiblePickup : MonoBehaviour
    {
        [Header("Pickup Settings")]
        [SerializeField] private float collectRadius = 1.5f;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotateSpeed = 180f;
        [SerializeField] private float bobAmplitude = 0.1f;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private ParticleSystem collectVfx;
        [SerializeField] private AudioClip collectSfx;
        [SerializeField] private bool autoDisable = true;

        [Header("Companion Spawn")]
        [SerializeField] private bool spawnCompanion = false;
        [SerializeField] private GameObject companionPrefab;
        [SerializeField] private Vector3 companionSpawnOffset = new Vector3(0, 0, -0.5f);

        private Transform player;
        private Vector3 startPos;
        private bool isCollecting;

        private void Awake()
        {
            if (visualRoot == null) visualRoot = transform;
            startPos = visualRoot.localPosition;

            // Collider as trigger
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Start()
        {
            if (WDPlayerController.Instance != null) player = WDPlayerController.Instance.transform;

            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
            }
        }


        private void Update()
        {
            if (visualRoot != null && !isCollecting)
            {
                float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
                visualRoot.localPosition = startPos + new Vector3(0, bob, 0);
                visualRoot.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            }

            if (!isCollecting && player != null)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= collectRadius)
                {
                    StartCollect();
                }
            }

            if (isCollecting && player != null)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                if (Vector3.Distance(transform.position, player.position) < 0.3f)
                {
                    CompleteCollect();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollecting) return;
            if (player == null) return;

            if (other.transform == player || other.CompareTag("Player"))
            {
                StartCollect();
            }
        }

        private void StartCollect()
        {
            isCollecting = true;
            // Stop bobbing
            if (visualRoot != null)
                startPos = visualRoot.localPosition;
        }

        private void CompleteCollect()
        {
            if (collectVfx != null)
                Instantiate(collectVfx, transform.position, Quaternion.identity);

            if (collectSfx != null && Audio_Manager.instance != null)
                Audio_Manager.instance.play(collectSfx.name);

            if (spawnCompanion)
                SpawnCompanion();

            if (autoDisable)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }

        private void SpawnCompanion()
        {
            if (companionPrefab == null || player == null)
                return;

            GameObject companion = Instantiate(companionPrefab, player.position + companionSpawnOffset, Quaternion.identity);
            CompanionFollower follower = companion.GetComponent<CompanionFollower>();
            if (follower != null)
            {
                follower.SetTarget(player);
            }
        }
    }
}
