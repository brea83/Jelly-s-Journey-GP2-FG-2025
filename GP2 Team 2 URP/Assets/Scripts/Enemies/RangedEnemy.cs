using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : Enemy
{
    [Header("Ranged Attack Details")]
    public int _projectileVelocity;
    public int _projectilesPerShot;
    public GameObject projectile;
    public Transform projectileSpawnPoint;
    public float _projectileLifeSpan;
    public bool _trajection;
    bool _isCoroutine;
    GameObject _target;
    public float timer;
    
    

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
        
        if(_isCoroutine && _isStunned)
        {
            StopCoroutine(Controller());
            _isCoroutine = false;
        }
    }

    public override void AvoidPlayer(Transform target)
    {
        base.AvoidPlayer(target);
        if (PrintDebugLogs) Debug.Log($"**************** {this} is trying to avoid the player");
        Vector3 away = (target.position - transform.position).normalized; 

        _animator.SetBool("MoveEnemy", false);
        _agent.speed = _moveSpeed / 2;
        _agent.isStopped = false;
        _agent.destination = (transform.position - away);
    }

    public override void Attack(GameObject target)
    {
        base.Attack(target);
        _target = target;
        if (PrintDebugLogs) Debug.Log($"********* {this} has been told to start attacking timer == {timer} and the frame is {Time.frameCount}");
        if (timer <= 0)
        {
            _animator.SetTrigger("AttackPlayer");
            timer = _attackCoolDown;
            if (PrintDebugLogs) Debug.Log($"********* {this} has started their attack animation.");
        }
    }

    //Animation Event
    public void StartShooting()
    {
        if (PrintDebugLogs) Debug.Log($"******** {this} is trying to start shooting");
        StartCoroutine(Controller());
        _isCoroutine = true;
    }

    //Animation Event
    public void StopShooting()
    {
        timer = _attackCoolDown;
        StopCoroutine(Controller());
        _isCoroutine = false;
        if (PrintDebugLogs) Debug.Log($"******************{this} has stopped its shoot coroutine");
    }

    IEnumerator Controller()
    {
        projectile.GetComponent<RangedProjectile>()._target = _target;
        projectile.GetComponent<RangedProjectile>()._velocity = _projectileVelocity;
        projectile.GetComponent<RangedProjectile>()._damage = _damage;
        projectile.GetComponent<RangedProjectile>()._lifeSpan = _projectileLifeSpan;
        projectile.GetComponent<RangedProjectile>()._trajectory = _trajection;

        for (int i = 0; i < _projectilesPerShot; i++)
        {
            if (!_isStunned)
            {
                Instantiate(projectile, projectileSpawnPoint.transform.position, Quaternion.identity);
                if (PrintDebugLogs) Debug.Log($"********** {this} has instantiated a shot");
                yield return new WaitForSeconds(1.08f / _projectilesPerShot);
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
    }
}
