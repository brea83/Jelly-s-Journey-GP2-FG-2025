using UnityEngine;

public class MeleeEnemy : Enemy
{
    AIDamageSensor sensor;
    float timer;

    public override void Initialize()
    {
        base.Initialize();
        timer = 0;
    }

    public override void UpdateEnemy()
    {
        base.UpdateEnemy();
        if (timer > 0)
        { 
            timer -= Time.deltaTime;
        }
    }

    public override void Attack(GameObject target)
    {
        base.Attack(target);
        
        if (timer <= 0)
        {
            _animator.SetTrigger("AttackPlayer");
            //_animator.SetBool("Attacking", true);
            timer = _attackCoolDown;
        }  
    }

    public void EndDealDamage()
    {
        if (!_isStunned)
        {
            sensor = GetComponentInChildren<AIDamageSensor>();
            sensor.DamageChecker();
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
    }
}
