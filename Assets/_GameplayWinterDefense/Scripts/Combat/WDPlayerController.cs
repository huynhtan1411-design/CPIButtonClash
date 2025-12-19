using UnityEngine;
using WD;
using System.Collections;

namespace CLHoma.Combat
{
    public class WDPlayerController : MonoSingleton<WDPlayerController>
    {
        [SerializeField] private PlayerHeroEntity playerHeroEntity;
        [SerializeField] private Vector3 spawnPoint;
        [SerializeField] private float respawnDelay = 2f;
        [SerializeField] private GameObject playerObject;
        [SerializeField] private GameObject horseObject;
        [SerializeField] private GameObject ghostObject;
        [Header("Safe Zone Damage")]
        [SerializeField] private float damageInterval = 1f; 
        [SerializeField] private float damageAmount = 10f; 
        [SerializeField] private float addHealthAmount = 5f; 
        private float damageTimer;
        private float addHealthTimer;

        public PlayerHeroEntity PlayerHeroEntity { get => playerHeroEntity; set => playerHeroEntity = value; }

        private void Start()
        {
            // Store initial position as spawn point if not set
            if (spawnPoint == Vector3.zero)
            {
                spawnPoint = transform.position;
            }

            // Get PlayerHeroEntity if not assigned
            if (playerHeroEntity == null)
            {
                playerHeroEntity = GetComponent<PlayerHeroEntity>();
            }
        }

        private void Update()
        {
            //CheckSafeZoneDamage();
        }

        private void CheckSafeZoneDamage()
        {
            if (playerHeroEntity.IsDead)
                return;
            if (!SafeZoneController.IsPointInSafeZone(transform.position))
            {
                damageTimer += Time.deltaTime;
                addHealthTimer = 0f;
                if (damageTimer >= damageInterval)
                {
                    damageTimer = 0f;
                    if (playerHeroEntity != null && playerHeroEntity.gameObject.activeSelf)
                    {
                        playerHeroEntity.TakeDamage(damageAmount);
                    }
                }
            }
            else
            {
                if (WD.GameManager.Instance.CurrentPhase == GamePhase.BuildPhase)
                {
                    damageTimer = 0f;
                    addHealthTimer += Time.deltaTime;
                    if (addHealthTimer >= damageInterval)
                    {
                        addHealthTimer = 0f;
                        if (playerHeroEntity != null && playerHeroEntity.gameObject.activeSelf)
                        {
                            playerHeroEntity.AddHealth(addHealthAmount);
                        }
                    }
                }
            }
        }

        public void ShowGhost()
        {
            playerObject.SetActive(false);
            horseObject.SetActive(false);
            ghostObject.SetActive(true);
            gameObject.tag = "Ghost";
        }

        public void RespawnPlayer(bool isDelay = false)
        {
            if (isDelay)
            {
                StartCoroutine(RespawnWithDelay());
            }
            else
            {
                RespawnImmediate();
            }
        }

        private IEnumerator RespawnWithDelay()
        {
            yield return new WaitForSeconds(respawnDelay);
            RespawnImmediate();
        }

        private void RespawnImmediate()
        {
            gameObject.SetActive(false);
            // Reset position
            transform.position = spawnPoint;
            gameObject.tag = "Player";

            // Reset health and reactivate player
            if (playerHeroEntity != null)
            {
                playerHeroEntity.gameObject.SetActive(true);
                playerHeroEntity.ResetHealth();
                // playerObject.SetActive(true);
                // horseObject.SetActive(true);
                // ghostObject.SetActive(false);
            }
            gameObject.SetActive(true);
        }

        public void SetSpawnPoint(Vector3 newSpawnPoint)
        {
            spawnPoint = newSpawnPoint;
        }
    }
}