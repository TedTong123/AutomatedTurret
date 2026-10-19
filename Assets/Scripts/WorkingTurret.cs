using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WorkingTurret : MonoBehaviour
{
    // Settings For The Turret
    [Header("Turret Settings")]
    [SerializeField] int TurretDamage;
    [SerializeField] int CurrentAmmo;
    [SerializeField] int MaxAmmo;
    [SerializeField] float FireRate;
    [SerializeField] float ReloadSpeed;
    [SerializeField] float TurnSpeed;

    //Boolean Variables
    private bool CanShoot = true;
    private bool IsReloading;

    //Game Object Variables
    private Collider[] HitObjects;
    private List<GameObject> Targets = new List<GameObject>();
    private GameObject CurrentTarget = null;

    //Interface Variables
    private IDamage DamageInterface = null;

    //Vector Variables
    private Vector3 DesiredRotation;

    //Quaternion Variables
    Quaternion DesiredQuaternion;

    void Start()
    {
        //Gets All Objects Around Turret And Filters Out The Game Objects That Do Not Have The Target Tag
        HitObjects = Physics.OverlapSphere(transform.position, 150f);
        foreach (Collider collider in HitObjects)
        {
            if (collider.gameObject.CompareTag("Target"))
            {
                Targets.Add(collider.gameObject);
                Debug.Log(collider);
            }
        }
    }

    void Update()
    {
        RotateToDesiredRotation();
        //Checks If Variable CurrentTarget Is Null If So Then It Activates The FindClosestTarget Variable And Sets CurrentTarget To The New Closest Target Then Sets Variable DamageInterface To The Current Interface Then Activates The FindDesiredRotation Function
        if (CurrentTarget == null)
        {
            Debug.Log("Current Target Not Found");
            CurrentTarget = FindClosestTarget();
            if(CurrentTarget != null)
            {
                DamageInterface = CurrentTarget.GetComponent<IDamage>();
                FindDesiredRotation();
                Debug.Log(CurrentTarget);
                Debug.Log(DamageInterface == null);
            }
        }
        //Checks If Variable DamageInterface Is Not Null If So Then 
        else if (DamageInterface != null)
        {
            TryShooting();
        }
    }

    void RotateToDesiredRotation()
    {
        //Converts The Vector To A Quaternion And Smoothly Rotates The Turret To The DesiredQuaternion
        DesiredQuaternion = Quaternion.Euler(DesiredRotation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, DesiredQuaternion, TurnSpeed * Time.deltaTime);
    }
    GameObject FindClosestTarget()
    {
        //Clears The ClosestDistance And ClosestTarget Variables And Then Removes All Null GameObjects From The Target List Then Goes Through The Target List And Checks Each Ones Distance From The Turret To Find The Closest Target Then Once It's Gone Through Every Game Object In The Target List It Outputs The ClosestTarget
        Debug.Log("Finding Closest Target");
        float ClosestDistance = Mathf.Infinity;
        GameObject ClosestTarget = null;
        Targets.RemoveAll(GameObject => GameObject == null);
        foreach(GameObject Target in Targets)
        {
            Vector3 DistanceFromTurret = Target.transform.position - transform.position;
            if (DistanceFromTurret.magnitude < ClosestDistance)
            {
                ClosestDistance = DistanceFromTurret.magnitude;
                ClosestTarget = Target;
            }
        }
        return ClosestTarget;
    }
    void FindDesiredRotation()
    {
        //Does Math To Find The Look At Rotation So The Turret Is Looking At The Current Target
        Vector3 DirectionOfTarget = CurrentTarget.transform.position - transform.position;
        float YRotation = Mathf.Atan2(DirectionOfTarget.x, DirectionOfTarget.z) * Mathf.Rad2Deg;
        DesiredRotation = new Vector3(0, YRotation += -90f, 0);
    }

    void TryShooting()
    {
        //Checks If Can Shoot Is True And That The Turret Has Ammo And That The Rotation Of The Turret Equals The DesiredRotation And IsReloading Is False If So Then It Sets CanShoot False And Subtracts 1 From CurrentAmmo And Then Using The Damage Interface Damages The Target Then Activates A Coroutine Called Shoot
        if (CanShoot && CurrentAmmo > 0 && transform.rotation == DesiredQuaternion && !IsReloading)
        {
            Debug.Log("Shooting");
            CanShoot = false;
            CurrentAmmo--;
            DamageInterface.Damage(TurretDamage);
            StartCoroutine(Shoot());
        }
        //Checks If The Turrets Ammo Is Zero If So Then It Sets IsReloading To True Then Sets CurrentAmmo To MaxAmmo Then Activates A Coroutine Called Reload
        else if (CurrentAmmo == 0)
        {
            IsReloading = true;
            CurrentAmmo = MaxAmmo;
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
        yield return new WaitForSeconds(ReloadSpeed);
        IsReloading = false;
    }
}
