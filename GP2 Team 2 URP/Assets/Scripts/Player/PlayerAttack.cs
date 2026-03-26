using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour, IGameStateMachineListener
{
    [SerializeField] private PlayerController _PC;

    [Header("Attack Settings")]
    public float comboResetTimer;
    public int comboStep;
    public int maxComboStep;
    public bool isAttacking = false;
    public float lastAttackTime = 0f;
    private GameObject _currentWeaponAttack;
    public int unarmedDamage;
    public float attackMovementForce;

    [Header("Slot Multipliers")]
    public float[] slotMultiplier;

    [SerializeField] GameObject[] _attackCollider;

    int weaponThrowingPower = 15;
    private int damageAmount;
    //private float hiddenAttackTimer = 1f;

    protected PlayingState _updateState;

    //float timer = 1.5f;
    [Header("Debug Settings")]
    public bool PrintDebugLogs = false;

    private void Start()
    {
        // moved to the interface IGameStateMAchineListener func OnGameStateMachineInitialized which is called from the GameManager's events
        //_updateState = GameManager.Instance.GetState<PlayingState>();
    }

    protected void OnDisable()
    {
        if (_updateState != null)
        {
            _updateState.StateUpdate.RemoveListener(ManagedUpdate);
        }
    }

    protected void OnEnable()
    {
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(ManagedUpdate);
        }
        else
        {
            if (PrintDebugLogs) Debug.Log("tried to add listener but myUpdateState == null");
        }
    }
    protected void ManagedUpdate()
    {
        //RemoveCollisionTimer();
        if(PrintDebugLogs) Debug.Log("Player Attack script Reached Managed Update");
    }


    private void Awake()
    {
        comboStep = 0;
        isAttacking = false;
    }

    public void Attack()
    {
        if (isAttacking) { return; }
        isAttacking = true;

        _PC._rb.AddForce(transform.forward * attackMovementForce, ForceMode.Impulse);

        _PC.animator.SetTrigger("Attack" + comboStep);
    }

    public void EndAttack()
    {
        comboStep++;
        isAttacking = false;

        if (comboStep >= maxComboStep || comboStep > 3) comboStep = 0;

        lastAttackTime = Time.time;
    }

    public void ThrowAttack()
    {
        _PC.animator.SetTrigger("Throw");
        for (int i = 0; i < _PC.handPositions.Length; i++)
        {
            if (_PC.handPositions[i].childCount > 0)
            {
                GameObject weapon = _PC.handPositions[i].GetChild(0).gameObject;
                Weapon weaponcomp = weapon.GetComponent<Weapon>();
                _PC.playerInventory.ThrowItem(weapon);
                weapon.transform.SetParent(null);

                weaponcomp.isThrown = true;
                weaponcomp.isPickedUp = false;

                weapon.transform.position = _PC.handPositions[i].position;
                weapon.transform.localScale = Vector3.one;
                GameObject weaponchild = weapon.transform.GetChild(0).gameObject;
                weaponchild.transform.localScale = new Vector3(100f, 100f, 100f);

                Rigidbody weaponRb = weapon.GetComponent<Rigidbody>();
                if (weaponRb != null)
                {
                    weaponRb.isKinematic = false;
                    weaponRb.linearVelocity = Vector3.zero;

                    Vector3 throwDirection = _PC.transform.forward;
                    weaponRb.AddForce(throwDirection * weaponThrowingPower, ForceMode.Impulse);
                }
                Collider weaponCollider = weapon.GetComponent<BoxCollider>();
                if (weaponCollider != null)
                {
                    weaponCollider.enabled = true;
                }

                for (int j = i + 1; j < _PC.handPositions.Length; j++)
                {
                    if (_PC.handPositions[j].childCount > 0)
                    {
                        Transform weapontomove = _PC.handPositions[j].GetChild(0);
                        weapontomove.transform.SetParent(_PC.handPositions[j - 1]);
                        weapontomove.transform.localPosition = Vector3.zero;
                        weapontomove.transform.localRotation = Quaternion.identity;
                    }
                }
                return;
            }
        }
    }

    public void TurnOnCollision()
    {
        _attackCollider[comboStep].SetActive(true);
    }

    public void RemoveCollision()
    {
        _attackCollider[comboStep].SetActive(false);
        for (int i = 0; i <= _attackCollider.Length - 1; i++)
        {
            _attackCollider[i].gameObject.SetActive(false);
        }
    }    
    //private void RemoveCollisionTimer()
    //{
    //    //if (timer <= 0)
    //    //{
    //    //    _attackCollider[0].SetActive(false);
    //    //    _attackCollider[1].SetActive(false);
    //    //    _attackCollider[2].SetActive(false);
    //    //    _attackCollider[3].SetActive(false);
    //    //    timer = 1f;
    //    //}

    //    for (int i = 0; i <= _attackCollider.Length - 1; i++)
    //    {
    //            _attackCollider[i].gameObject.SetActive(false);
    //    }
    //}

    public void IncreaseComboStep()
    {

        if(PrintDebugLogs) Debug.Log("Increased combostep to: " + comboStep);
    }

    public int CalculateDamage()
    {
        if (_PC.handPositions[0].childCount <= 0)
            return unarmedDamage;

        _currentWeaponAttack = _PC.handPositions[comboStep].GetChild(0).gameObject;
        Weapon weapon = _currentWeaponAttack.GetComponent<Weapon>();
        return (int)(weapon.damage * slotMultiplier[comboStep]);
    }

    public void OnGameStateMachineInitialized(GameManager manager)
    {
        _updateState = manager.GetState<PlayingState>();
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(ManagedUpdate);
        }
        else
        {
            Debug.Log("tried to add PlayerPickUp's listener but myUpdateState == null");
        }
    }
}

