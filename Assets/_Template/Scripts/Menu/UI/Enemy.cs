using UnityEngine;
public class Enemy : MonoBehaviour
{
    public float energyOnKill = 10f; 

    public void Die()
    {
        Destroy(gameObject);
    }
}