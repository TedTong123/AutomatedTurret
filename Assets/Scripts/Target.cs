using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour, IDamage
{
    [SerializeField] private float Health;

    public void Damage(int Damage)
    {
        Debug.Log("Ow That Hurt");
        Health -= Damage;
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
