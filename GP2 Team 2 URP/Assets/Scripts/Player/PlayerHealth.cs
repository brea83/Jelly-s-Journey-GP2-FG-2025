using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IGameStateMachineListener
{
    protected PlayingState _updateState;

    PlayerController _controller;
    PlayerInventory _inventory;
    PlayerWeaponUI _weaponUI;
    PlayerAttack _pAttack;
    //[SerializeField] private UIHealthSlider healthBarSlider;

    private bool _IsDamaged;
    private float _damagedTimer;
    public float invulnerableTimer;

    [SerializeField] private int _maxHealth;
    [SerializeField] Animator _animator;

    [Header("Debug Settings")]
    public bool PrintDebugLogs = false;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<PlayerController>();
        _inventory = GetComponent<PlayerInventory>();
        _weaponUI = GetComponent<PlayerWeaponUI>();
        _pAttack = GetComponent<PlayerAttack>();
        // moved to the interface IGameStateMAchineListener func OnGameStateMachineInitialized which is called from the GameManager's events
        //_updateState = GameManager.Instance.GetState<PlayingState>();

        _maxHealth = 1;
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
        DamagedTimer();

    }

    public void TakeDamage()
    {
        _animator.Play("rig_001|gethit", 0);
        _pAttack.EndAttack();

        if (!_IsDamaged)
        {
            _IsDamaged = true;

            for (int i = _controller.handPositions.Length - 1; i >= 0; i--)
            {
                if (_controller.handPositions[i].childCount > 0)
                {
                    GameObject weaponInHand = _controller.handPositions[i].GetChild(0).gameObject;

                    int weaponDurability = weaponInHand.GetComponent<Weapon>().durability -= 1;

                    if (weaponDurability == 2)
                    {
                        weaponDurability--;
                        return;
                    }

                    if (weaponDurability == 1)
                    {
                        weaponDurability--;
                        return;
                    }

                    if (weaponDurability <= 0)
                    {
                        _inventory.weapons.Remove(weaponInHand);
                        Destroy(weaponInHand);
                        return;
                    }

                }
                if (_controller.handPositions[0].childCount <= 0)
                {
                    _maxHealth--;
                    //_weaponUI.healthDisplay.SetActive(false);

                    if (_maxHealth <= 0)
                    {
                        GameManager.Instance.switchState<EndState>();
                    }
                }
            }
        }
    }

    private void DamagedTimer()
    {
        if (_IsDamaged)
        {
            _damagedTimer += Time.deltaTime;

            if (_damagedTimer >= invulnerableTimer)
            {
                _damagedTimer = 0;
                _IsDamaged = false;
            }
        }
    }

    public void Initialize()
    {
        _maxHealth = 1;
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
            if (PrintDebugLogs) Debug.Log("tried to add PlayerPickUp's listener but myUpdateState == null");
        }
    }
}

