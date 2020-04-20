using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{

    [Space(5), Header("Base Stats")]
    public int baseHealth = 5;    
    public int baseMovementSpeed = 5;
    public int baseAttack = 5;
    public int baseAttackRange = 1;
    public int baseInitiation = 5;
    public bool canFly = false;
    public float attackSpeed = 5;
    
    
    public float dying = 10f;
    public Material skin;

    [Space(5), Header("Current Stats")]
    public float currentHealth;
    public Vector3Int currentTilePosition;

    [Space(5), Header("Control Variables")]    
    public bool active = false;
    public bool playerControlled = false;

    [Space(5), Header("Reference Variables")]
    public UnitManager unitManager;

    private void Start()
    {
        InitiliseUnit();
    }

    private void InitiliseUnit()
    {
        currentHealth = baseHealth;
    }

    private void Update()
    {
        DeathCheck();
        
        if (active)
        {
            // add glow to unit to indicate current units turn
        }
    }

    
    private void DeathCheck()
    {
        if (currentHealth <= 0)
        {
            //unitManager.gameUnits.Remove(this);

            dying -= Time.deltaTime;
            skin.SetFloat("Fade", dying / 10);
        }
        if (dying <= 0)
        {            
            Destroy(this.gameObject);            
        }
    }

    public void TakeDamage(float damageAmount)
    {
        // do hurt animation

        // calculate damages applied

        // apply damage
        currentHealth -= damageAmount;
    }
}
