using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 _moveDirection;
    float horizontalInput;
    float verticalInput;
    float inRange = 17f;
    protected PlayingState _updateState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        _updateState = GameManager.Instance.GetState<PlayingState>();
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(ManagedUpdate);
        }
        else
        {
            Debug.Log("tried to add listener but my UpdateState == null");
        }
    }

    // Update is called once per frame
    void ManagedUpdate()
    {
        MyInput();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = (new Vector3(horizontalInput, 0f, verticalInput) * 100);
    }

    public void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown("space"))
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider[] context = Physics.OverlapSphere(transform.position, inRange);
        
        foreach (Collider c in context)
        {
            if(c.gameObject.CompareTag("Enemy"))
            {
                //Enemy enemy = c.gameObject.GetComponent<Enemy>();
                Enemy enemy = c.gameObject.GetComponentInParent<Enemy>();
                //enemy.Dead();
                enemy.TakeDamage(15);
                Debug.Log("attempting stun");
            }
        }
    }
}
