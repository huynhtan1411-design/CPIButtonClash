using UISystems;
using UnityEngine;
using WD;
using TemplateSystems.Controllers.Player;
using CLHoma.Combat;

public class CharacterTriggerHandler : MonoBehaviour
{
    const string SlotBuilder = "SlotBuilder";
    const string Tower = "Tower";
    const string Tree = "Tree";
    const string Resource = "Resource";

    [SerializeField] PlayerAttackController playerAttackController;
    [SerializeField] BaseDamageableEntity characterEntity;
    [SerializeField] private float resourceCollectionRadius = 3f;
    
    private SphereCollider resourceCollector;

    private void Reset()
    {
        playerAttackController = GetComponent<PlayerAttackController>();
        characterEntity = GetComponent<BaseDamageableEntity>();
    }

    private void Awake()
    {
        // Add a trigger collider for resource collection
        resourceCollector = gameObject.AddComponent<SphereCollider>();
        resourceCollector.isTrigger = true;
        resourceCollector.radius = resourceCollectionRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (characterEntity.IsDead) return;
        
        if (other.CompareTag(Resource))
        {
            ResourceItem resource = other.GetComponent<ResourceItem>();
            if (resource != null)
            {
                resource.SetTarget(transform);
                resource.MoveToTarget();
            }
            return;
        }
        
        if (other.CompareTag(SlotBuilder))
        {
            // if (GameManager.Instance.CurrentPhase != GamePhase.BuildPhase) return;
            HandleSlotBuilder(other.gameObject);
        }
        if (other.CompareTag(Tower))
        {
            //HandleTower(other.gameObject, true);
        }
        if (other.CompareTag("dasdasd"))
        {
            TreeInteractiveBehaviour tree = other.GetComponent<TreeInteractiveBehaviour>();
            if (tree != null && !tree.IsDestroyed)
            {
                tree.ChopTree(delegate
                {
                    playerAttackController.UseAxe();
                });
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the resource collection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, resourceCollectionRadius);
    }

    private void OnTriggerExit(Collider other)
    {
        if (characterEntity.IsDead) return;
        if (other.CompareTag(SlotBuilder))
        {
            //var ctrl = UIManager.instance.UIGameplayCtr;
            //if (ctrl != null)
            //{
            //    ctrl.ToggleButtonCrafting(false);
            //}
        }
        //if (other.CompareTag(Tower))
        //{
        //    HandleTower(other.gameObject, false);
        //}

        if (other.CompareTag(Tree))
        {
            TreeInteractiveBehaviour tree = other.GetComponent<TreeInteractiveBehaviour>();
            if (tree != null && !tree.IsDestroyed)
            {
                tree.CancelChop();
            }
        }
    }

    private void HandleSlotBuilder(GameObject item)
    {
        SlotBuildingBehaviour builder = item.GetComponentInParent<SlotBuildingBehaviour>();
        if (builder != null)
        {
            //BuildingManager.Instance.CurrentActiceSlotInteractive = builder;
            builder.BuildTower();
        }
        else
        {
            Debug.LogError("Slot Builder is not empty or does not exist.");
        }
    }

    private void HandleTower(GameObject item, bool IsEnter)
    {
        TowerBehavior towerBehavior = item.GetComponentInParent<TowerBehavior>();
        if (towerBehavior != null)
        {
            towerBehavior.ToggleAimRing(IsEnter);
        }
    }
} 