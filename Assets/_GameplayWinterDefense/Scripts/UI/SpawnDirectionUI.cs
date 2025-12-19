using UnityEngine;
using TMPro;
using System.Collections.Generic;
using WD;

public class SpawnDirectionUI : MonoBehaviour
{
    [System.Serializable]
    public class DirectionUIConfig
    {
        public SpawnDirection direction;
        public Vector2 screenOffset;    // Offset from screen edge
        public Vector2 anchorPosition;  // Anchor position (0-1)
    }

    [Header("UI Prefab")]
    [SerializeField] private GameObject spawnInfoPrefab;// Prefab containing TextMeshPro
    [Header("UI Prefab")]
    [SerializeField] private Transform spawnInfoParent;

    [Header("Direction Configurations")]
    private List<DirectionUIConfig> directionConfigs = new List<DirectionUIConfig>
    {
        new DirectionUIConfig { direction = SpawnDirection.Top, screenOffset = new Vector2(0, -100), anchorPosition = new Vector2(0.5f, 1) },
        new DirectionUIConfig { direction = SpawnDirection.Down, screenOffset = new Vector2(0, 100), anchorPosition = new Vector2(0.5f, 0) },
        new DirectionUIConfig { direction = SpawnDirection.Left, screenOffset = new Vector2(50, 0), anchorPosition = new Vector2(0, 0.5f) },
        new DirectionUIConfig { direction = SpawnDirection.Right, screenOffset = new Vector2(-50, 0), anchorPosition = new Vector2(1, 0.5f) },
        new DirectionUIConfig { direction = SpawnDirection.TopLeft, screenOffset = new Vector2(100, -100), anchorPosition = new Vector2(0, 1) },
        new DirectionUIConfig { direction = SpawnDirection.TopRight, screenOffset = new Vector2(-100, -100), anchorPosition = new Vector2(1, 1) },
        new DirectionUIConfig { direction = SpawnDirection.DownLeft, screenOffset = new Vector2(100, 100), anchorPosition = new Vector2(0, 0) },
        new DirectionUIConfig { direction = SpawnDirection.DownRight, screenOffset = new Vector2(-100, 100), anchorPosition = new Vector2(1, 0) }
    };

    private Dictionary<SpawnDirection, GameObject> activeUIElements = new Dictionary<SpawnDirection, GameObject>();

    public void ShowSpawnInfo(List<EnemyGroup> enemyGroups)
    {
        // Clear existing UI elements
        ClearSpawnInfo();

        // Group enemies by direction
        Dictionary<SpawnDirection, int> enemiesByDirection = new Dictionary<SpawnDirection, int>();
        foreach (var group in enemyGroups)
        {
            SpawnDirection direction = group.spawnDirection;
            
            // Handle Random direction
            if (direction == SpawnDirection.Random)
            {
                // Exclude Random from possible directions
                SpawnDirection[] possibleDirections = new SpawnDirection[] 
                {
                    SpawnDirection.Top, SpawnDirection.Down, SpawnDirection.Left, SpawnDirection.Right,
                    SpawnDirection.TopLeft, SpawnDirection.TopRight, SpawnDirection.DownLeft, SpawnDirection.DownRight
                };
                direction = possibleDirections[Random.Range(0, possibleDirections.Length)];
            }

            if (!enemiesByDirection.ContainsKey(direction))
            {
                enemiesByDirection[direction] = 0;
            }
            enemiesByDirection[direction] += group.count;
        }
        // Create UI for each direction
        foreach (var dirConfig in directionConfigs)
        {
            if (enemiesByDirection.ContainsKey(dirConfig.direction) && enemiesByDirection[dirConfig.direction] > 0)
            {
                CreateDirectionUI(dirConfig, enemiesByDirection[dirConfig.direction]);
            }
        }
    }

    private void CreateDirectionUI(DirectionUIConfig config, int enemyCount)
    {
        GameObject uiElement = Instantiate(spawnInfoPrefab, spawnInfoParent);
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
        uiElement.SetActive(true);
        // Set anchor and position
        rectTransform.anchorMin = config.anchorPosition;
        rectTransform.anchorMax = config.anchorPosition;
        rectTransform.anchoredPosition = config.screenOffset;

        // Set text
        TextMeshProUGUI tmpText = uiElement.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"{enemyCount}";
        }

        // Store reference
        activeUIElements[config.direction] = uiElement;
    }

    public void ClearSpawnInfo()
    {
        foreach (var uiElement in activeUIElements.Values)
        {
            if (uiElement != null)
            {
                Destroy(uiElement);
            }
        }
        activeUIElements.Clear();
    }
} 