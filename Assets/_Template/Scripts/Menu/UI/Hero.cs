using UnityEngine;
public class Hero : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    public void Attack(Enemy enemy)
    {
        enemy.Die();
    }
}