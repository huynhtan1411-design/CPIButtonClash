using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLHoma.Combat;

public class WallUnit : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 10f;
    private Dictionary<BaseEnemyBehavior, float> enemyLastDamageTime = new Dictionary<BaseEnemyBehavior, float>();

    private void OnTriggerStay(Collider other)
    {
        if (damagePerSecond == 0) return;
        if (other.CompareTag("Enemy"))
        {
            BaseEnemyBehavior enemy = other.GetComponent<BaseEnemyBehavior>();
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerSecond, transform.position, Vector3.zero, HitType.Hit);
                enemyLastDamageTime[enemy] = Time.time;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (damagePerSecond == 0) return;
        if (other.CompareTag("Enemy"))
        {
            BaseEnemyBehavior enemy = other.GetComponent<BaseEnemyBehavior>();
            if (enemy != null && enemyLastDamageTime.ContainsKey(enemy))
            {
                enemyLastDamageTime.Remove(enemy);
            }
        }
    }
}
