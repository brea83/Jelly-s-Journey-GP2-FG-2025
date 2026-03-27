using NGAME;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, NGAME.ISpawnable
{
    protected PlayingState _updateState;
    [Header("Spawning Info")]
    [SerializeField]
    protected SO_SpawnTypeTag _spawnTypeTag;
    [Header("Enemy Stats")]
    [SerializeField]
    public int _moveSpeed;
    [SerializeField]
    public int _currentHealth;
    [SerializeField]
    public int _damage;

    [SerializeField]
    public float _lineOfSight;
    [SerializeField]
    public float _attackDistance;
    [SerializeField]
    public float _attackCoolDown;
    public float _rotationSpeed;

    [Header("Ranged Enemy Avoid Player Distance")]
    [SerializeField]
    public float _avoidPlayer;


    [HideInInspector]
    public Animator _animator;
    private DropRateManager _dropRateManager;
    public bool _isStunned = false;
    private int _maxHealth = 0;
    float _stunnedTimer = 0.0f;
    [HideInInspector]
    public NavMeshAgent _agent;

    //public EnemeySoundEffects _soundEffects;

    private FloatingDamage _floatingDamage;

    [Header("Debug Settings")]
    public bool PrintDebugLogs = false;

    public SO_SpawnTypeTag SpawnTypeTag => _spawnTypeTag;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _dropRateManager = GetComponent<DropRateManager>();
        _animator = GetComponent<Animator>();
        _floatingDamage = GetComponent<FloatingDamage>();
        //_soundEffects = GetComponent<EnemeySoundEffects>();

        _updateState = GameManager.Instance.GetState<PlayingState>();
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(UpdateEnemy);
        }
        else
        {
            Debug.Log("tried to add listener but my UpdateEnemy3 == null");
        }

        Initialize();
    }

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        _updateState = GameManager.Instance.GetState<PlayingState>();
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(UpdateEnemy);
        }
        else
        {
            Debug.Log("tried to add listener but my UpdateEnemy3 == null");
        }

        Initialize();
    }

    public virtual void Initialize()
    {
        if (_maxHealth == 0)
        { 
            _maxHealth = _currentHealth;
        }
        if (!IsAlive())
        {
            _currentHealth = _maxHealth;
        }
        
    }

    public virtual void UpdateEnemy()
    {
        if (_stunnedTimer > 0)
        {
            _stunnedTimer -= Time.deltaTime;
        }
        else if (_isStunned)
        {
            _isStunned = false;
            _animator.SetBool("Stunned", false);
        }
    }

    public virtual void LookAtPlayer(Transform target)
    {
        //Vector3 here = new Vector3(target.position.x, transform.position.y, target.position.z);
        //transform.LookAt(here);

        Vector3 targetDir = (target.position - transform.position).normalized;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDir, _rotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    public virtual void MoveToTarget(Transform target)
    {
        //First time enemy spots player, double the _lineOfSight variable. 
        //Simplifies version of "tagging" the player or making the enemy more "alert", so the enemy continues chasing the player.
        _lineOfSight = 20;

        _animator.SetBool("MoveEnemy", true);
        _animator.SetBool("Attacking", false);

        _agent.speed = _moveSpeed;
        _agent.isStopped = false;
        _agent.destination = target.position;
        //if (PrintDebugLogs && this.GetComponent<RangedEnemy>() != null) Debug.Log($"------------ {this} is trying to move to a target with the navmesh");
    }

    public virtual void AvoidPlayer(Transform target)
    {
        
    }

    public virtual void Attack(GameObject target)
    {
        
        _agent.isStopped = true;
        _animator.SetBool("MoveEnemy", false);
        if (_damage == 0) { return; }
    }

    public virtual void Stunned()
    {
        _isStunned = true;
        _stunnedTimer = 1.0f;
        _agent.isStopped = true;
        
        _animator.SetBool("MoveEnemy", false);
        _animator.SetBool("Attacking", false);
        _animator.SetBool("Stunned", true);
    }

    public virtual void TakeDamage(int damage)
    {
        //_animator.SetTrigger("TakeDamage"); ??

        _currentHealth -= damage;
        string damage_text = damage.ToString();
        _floatingDamage.DamageFloat(damage_text);

        if (_currentHealth <= 0)
        {
            Dead();
        }
    }

    public void Dead()
    {
        _agent.isStopped = true;
        _animator.SetBool("MoveEnemy", false);
        _animator.SetTrigger("DeadEnemy");
        //Animation triggers FinishedDying event, see below
    }

    public void FinishedDying()
    {
        gameObject.SetActive(false);
        GetComponent<DropRateManager>().InstantiateLoot(transform.position);
        GetComponent<PooledObject>().ReturnToPool();
    }

    public bool IsAlive()
    {
        if (_currentHealth > 0)
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _lineOfSight);
        Gizmos.color= Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _avoidPlayer);
    }
}
