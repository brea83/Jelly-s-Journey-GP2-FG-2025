using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyScriptableObject", menuName = "Scriptable Objects/EnemyScriptableObject")]
public class EnemyScriptableObject : ScriptableObject
{
    //Bases stats for enemies
    [SerializeField]
    public int moveSpeed;
    public int MoveSpeed { get => moveSpeed; private set => moveSpeed = value; }

    [SerializeField]
    public int maxHealth;
    public int MaxHealth { get => maxHealth; private set => maxHealth = value; }

    [SerializeField]
    public int damage;
    public int Damage { get => damage; private set => damage = value; }

    [SerializeField]
    public int projectileSpeed; //I wanted to connect enemy scriptabl eobject to it's projectile (if it has one), but couldn't figure out how to pass it on to projectile
    public int ProjectileSpeed { get => projectileSpeed; private set => projectileSpeed = value; }
}
