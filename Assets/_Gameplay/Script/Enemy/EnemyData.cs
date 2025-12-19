using UnityEngine;

namespace CLHoma.Combat
{
    [CreateAssetMenu(fileName = "New Enemy Data", menuName = "CLHoma/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] private EnemyType enemyType;
        public EnemyType EnemyType => enemyType;

        [SerializeField] private EnemyTier enemyTier;
        public EnemyTier EnemyTier => enemyTier;

        [SerializeField] private ElementType elementType;
        public ElementType ElementType => elementType;

        [SerializeField] private GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] private EnemyStats stats;
        public EnemyStats Stats => stats;
    }
}