using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    protected PlayingState _updateState;

    public Rigidbody _rb;
    public Animator animator;
    public Camera cam;

    private PlayerPickUp _playerPickUp;
    public PlayerInventory playerInventory;
    public GameObject[] highlightedHand;

    public PlayerAttack PlayerAttackComponent;

    [Header("Movement Settings")]
    private Vector2 _moveDirection;
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _acceleration = 10f;
    [SerializeField] float _deceleration = 10f;


    [Header("Dash Settings")]
    [SerializeField] float _dashDistance = 3f;
    [SerializeField] float _dashDuration = 0.2f;
    [SerializeField] float _dashCooldown = 0.5f;
    private bool _isDashing = false;
    private float _lastDash = 0f;
    [SerializeField] float dashAgainstWallOffset = 0.4f;


    private Vector3 _pickupRange = new Vector3(3, 3, 3);
    public Transform[] handPositions;
    private GameObject _currentWeapon;
    public int weaponsEquipped;

    #region Attack and Move Variables

    //[SerializeField] float _comboTimerSet;
    [SerializeField] float weaponThrowingPower;
    private float _throwCD = 0f;
    bool _canThrow = false;

    private Vector3 currentVel;

    #endregion

    [Header("Debug Settings")]
    public bool PrintDebugLogs = false;
    [SerializeField]
    private bool _GodMode = false;

    public void ToggleGodmode(bool value)
    {
        PlayerAttackComponent.ToggleGodmode(value);
        PlayerHealth health = GetComponentInChildren<PlayerHealth>();
        if (health != null)
        {
            health.ToggleGodmode(value);
        }
    }


    void Start()
    {
        _updateState = GameManager.Instance.GetState<PlayingState>();
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(ManagedUpdate);
            _updateState.StateFixedUpdate.AddListener(ManagedFixedUpdate);
        }
        else
        {
            Debug.Log("tried to add listener but myUpdateState == null");
        }

        _rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerInventory = GetComponent<PlayerInventory>();
        _playerPickUp = GetComponent<PlayerPickUp>();

        ToggleGodmode(_GodMode);
    }

    protected void ManagedUpdate()
    {
        _currentWeapon = _playerPickUp.closestWeapon;
        CheckWeaponAmount();

        if (Time.time - PlayerAttackComponent.lastAttackTime > PlayerAttackComponent.comboResetTimer && PlayerAttackComponent.isAttacking == false) 
        { 
            PlayerAttackComponent.comboStep = 0;
        }

        if (PlayerAttackComponent.comboStep >= weaponsEquipped) PlayerAttackComponent.comboStep = 0;
        PlayerAttackComponent.maxComboStep = weaponsEquipped;

        _throwCD += Time.deltaTime;
        if (_throwCD >= 0.5f) { _canThrow = true; }
    }

    public void OnEnable()
    {
        _isDashing = false;
        if (PlayerAttackComponent == null)
        {
            PlayerAttackComponent = GetComponentInChildren<PlayerAttack>();
            if (PlayerAttackComponent != null)
            {
                PlayerAttackComponent.isAttacking = false;
            }
            else
            {
                Debug.LogWarning("Player Controller could not find PlayerAttack in children on enable");
            }
        }
        
    }

    protected void ManagedFixedUpdate()
    {

        if(PlayerAttackComponent.isAttacking || _isDashing) { return; }
        Movement();


        if (_moveDirection.x != 0 || _moveDirection.y != 0)
        {
            animator.SetFloat("moveSpeed", 1);
        }
        else { animator.SetFloat("moveSpeed", 0); }
    }

    void CheckWeaponAmount()
    {
        weaponsEquipped = playerInventory.weapons.Count;
    }

    #region Attacks

    public void OnAttack()
    {
        if (PrintDebugLogs) Debug.Log("PLAYER RECIEVED ATTACK ACTION");
        PlayerAttackComponent.Attack();
    }

    public void OnThrow()
    {
        if (PrintDebugLogs) Debug.Log("PLAYER RECIEVED THROW ACTION");
        if (_canThrow) { PlayerAttackComponent.ThrowAttack(); _canThrow = false; _throwCD = 0f; }
                
    }
    #endregion

    #region MoveActions
    public void OnMove(InputValue value)
    {
        _moveDirection = value.Get<Vector2>();
    }

    private void Movement()
    {
        Vector3 targetVel = new Vector3(_moveDirection.x , 0f, _moveDirection.y) * _moveSpeed;
        currentVel.x = Mathf.MoveTowards(currentVel.x, targetVel.x, _acceleration * Time.fixedDeltaTime);
        currentVel.z = Mathf.MoveTowards(currentVel.z, targetVel.z, _acceleration * Time.fixedDeltaTime);

        if (_moveDirection == Vector2.zero)
        {
            currentVel.x = Mathf.MoveTowards(currentVel.x, 0, _deceleration * Time.fixedDeltaTime);
            currentVel.z = Mathf.MoveTowards(currentVel.z, 0, _deceleration * Time.fixedDeltaTime);
        }

        _rb.linearVelocity = currentVel;

        // Rotate player towards mouse if not using gamepad in player prefs
        if (PlayerPrefs.GetInt("isUsingGamepad", 0) == 0){
            Vector3 point = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f));
            float t = cam.transform.position.y / (cam.transform.position.y - point.y);
            Vector3 lastPoint = new Vector3(
                t * (point.x - cam.transform.position.x) + cam.transform.position.x,
                transform.position.y,
                t * (point.z - cam.transform.position.z) + cam.transform.position.z);

            Vector3 direction = lastPoint - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        //Below is code turner added
        // Rotate player towards movement direction if using gamepad, with clamp for deadzone
        else
        {
            Vector3 direction = new Vector3(_moveDirection.x, 0f, _moveDirection.y);

            // Deadzone threshold to filter out small unintended movements from joystick
            if (direction.sqrMagnitude < 0.2f * 0.2f) {
                return;
            }

            // Normalize direction to avoid rotation from small input variations
            direction = direction.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Only update rotation if the angle difference is significant
            if (Quaternion.Angle(transform.rotation, targetRotation) > 1f) {
                transform.rotation = targetRotation;
            }
        }
        //End code turner added

    }

    void OnDash()
    {
        if(_isDashing || Time.time < _lastDash + _dashCooldown || _moveDirection == Vector2.zero) { return; }
 
        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        _isDashing = true;
        _lastDash = Time.time;

        Vector3 dashDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.y);
        Vector3 startPos = _rb.position;
        Vector3 endPos = transform.position + dashDirection * _dashDistance;

        RaycastHit hit;
        if (Physics.Raycast(startPos, dashDirection, out hit, _dashDistance))
        {
            endPos = hit.point - dashDirection * dashAgainstWallOffset;
        }

        float timeGone = 0f;
        while (timeGone < _dashDuration)
        {
            timeGone += Time.deltaTime;
            float t = timeGone / _dashDuration;
            t = t * (2-t);

            _rb.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        _isDashing = false;
    }

    public void ChangeDash()
    {
        _isDashing = false;
    }

    #endregion

    #region Interact with weapons on ground

    public void OnPickup()
    {
        if (PrintDebugLogs) Debug.Log("PLAYER RECIEVED PICKUP ACTION");
        if (_currentWeapon != null && (_currentWeapon.transform.position.x - transform.position.x) <= _pickupRange.x && (_currentWeapon.transform.position.z - transform.position.z) <= _pickupRange.z)
        {
            if (_currentWeapon.GetComponent<Weapon>().isPickedUp == true || _currentWeapon.GetComponent<Weapon>().isThrown == true) { return; }

            _currentWeapon.GetComponent<Weapon>().isPickedUp = true;
            _currentWeapon.GetComponent<Weapon>().isThrown = false;
            PickUpItem();
            _playerPickUp.closestWeapon = null;
        }
    }

    private void PickUpItem()
    {

        if (playerInventory.AddWeapon(_currentWeapon))
        {
            for (int i = 0; i < handPositions.Length; i++)
            {
                if (handPositions[i].childCount == 0)
                {
                    EquipWeapon(_currentWeapon, i);
                    return;
                }
            }
        }
    }

    private void EquipWeapon(GameObject weapon,int handIndex)
    {
        weapon.transform.SetParent(handPositions[handIndex], false);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        GameObject weaponchild = weapon.transform.GetChild(0).gameObject;
        weaponchild.transform.localScale = Vector3.one;

        weapon.GetComponent<CapsuleCollider>().enabled = false;

        weapon.GetComponent<Rigidbody>().isKinematic = true;
        weapon.GetComponent<Rigidbody>().useGravity = false;
        weapon.GetComponent<BoxCollider>().enabled = false;
    }
    #endregion

    //public void OnMenu()
    //{
    //    if (PrintDebugLogs) Debug.Log("PLAYER RECIEVED OPEN MENU ACTION");
    //    GameManager.Instance.SwitchState<PauseState>();
    //}

    public void Initialize()
    {
        for (int i = 0;i < handPositions.Length; i++)
        {
            if(handPositions[i].childCount > 0)
            {
                Destroy(handPositions[i].GetChild(0).gameObject);
            }

        }
    }
}

