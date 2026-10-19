using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] int Damage;
    [SerializeField] int Ammo;
    [SerializeField] int MaxAmmo;
    [SerializeField] float FireRate;
    [SerializeField] float ReloadTime;
    [SerializeField] float TurnSpeed;

    private Collider[] HitObjects;
    private List<GameObject> Targets = new List<GameObject>();
    private Vector3 TargetRotation;
    private float ClosestDistance = Mathf.Infinity;
    private GameObject ClosestTarget = null;
    private GameObject CurrentTarget = null;
    private bool CanShoot = true;
    private bool IsReloading;
    private IDamage DamageInterface;

    void Start()
    {
        // Check Objects Around Turret And Filter The Non Target Objects Out And Add The Target Objects To The Targets List
        HitObjects = Physics.OverlapSphere(transform.position, 150f);
        foreach (Collider collider in HitObjects)
        {
            if (collider.gameObject.CompareTag("Target"))
            {
                Targets.Add(collider.gameObject);
            }
        }
    }

    void Update()
    {
        DesiredRotaion();
        if (CurrentTarget == null || DamageInterface == null)
        {
            CurrentTarget = FindClosestTarget();
            if (CurrentTarget != null)
            {
                DamageInterface = CurrentTarget.GetComponent<IDamage>();
                if (DamageInterface != null)
                {
                    FindLookAtRotation();
                }
            }
            else
            {
                DamageInterface = null;
            }
        }
        if (CurrentTarget != null && DamageInterface != null)
        {
            TryShooting();
        }

    }

    //Logic To Smoothly Rotate The Turret To The Target Rotation
    void DesiredRotaion()
    {
        Quaternion TargetQuaternion = Quaternion.Euler(TargetRotation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, TargetQuaternion, TurnSpeed * Time.deltaTime);
    }
    
    //Logic To Find Which Target Is The Closest
    GameObject FindClosestTarget()
    {
        ClosestDistance = Mathf.Infinity;
        ClosestTarget = null;
        for (int i = Targets.Count - 1; i >= 0; i--)
        {
            GameObject target = Targets[i];
            if (target == null)
            {
                Targets.RemoveAt(i);
                continue;
            }
            IDamage damage = target.GetComponent<IDamage>();
            if (damage == null)
            {
                Targets.RemoveAt(i);
                continue;
            }


            Vector3 TargetDistance = target.transform.position - transform.position;
            if (TargetDistance.magnitude < ClosestDistance)
            {
                ClosestDistance = TargetDistance.magnitude;
                ClosestTarget = target;
            }
        }
        return ClosestTarget;
    }

    //Logic To Calculate The Look At Rotation For The Current Target
    void FindLookAtRotation()
    {
        Vector3 TargetDirection = CurrentTarget.transform.position - transform.position;
        float TargetY = Mathf.Atan2(TargetDirection.x, TargetDirection.z) * Mathf.Rad2Deg;
        TargetY += -90f;
        TargetRotation = new Vector3(0, TargetY, 0);
    }

    //Logic For Try Shooting
    void TryShooting()
    {
        if (CurrentTarget == null || DamageInterface == null)
            return;

        if (CanShoot && Ammo > 0)
        {
            CanShoot = false;
            Ammo--;
            DamageInterface.Damage(Damage);
            StartCoroutine(Shoot());
        }
        else if (Ammo == 0 && !IsReloading)
        {
            IsReloading = true;
            StartCoroutine(Reload());
        }
    }
    IEnumerator Shoot()
    {
        yield return new WaitForSeconds(FireRate);
        CanShoot = true;
    }
    IEnumerator Reload()
    {
        yield return new WaitForSeconds(ReloadTime);
        IsReloading = false;
        Ammo = MaxAmmo;
    }
}