using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    protected PlayingState _updateState;

    bool _canDealDamage;
    bool _hasDealtDamage;

    //weapon details?
    public float _weaponLength;
    public float _weaponDamage;

    void Start()
    {
        _canDealDamage = false;
        _hasDealtDamage = false;

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

    
    void ManagedUpdate()
    {
        if (_canDealDamage && !_hasDealtDamage)
        { 
            //Testing dealing damage, do not touch! Changes to be made when we have enemy animations
            RaycastHit hit;
            //int layerMask = 1 << 8;

            if (Physics.Raycast(transform.position, -transform.up, out hit, _weaponLength))
            {
                Debug.Log("Enemy has dealt damage");
                //eg Player.TakeDamage();
                _hasDealtDamage = true;
            }
        }
    }

    public void StartDealDamage()
    {
        _canDealDamage = true;
        _hasDealtDamage = false;
    }

    public void EndDealDamage()
    {
        _canDealDamage = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.right * _weaponLength);
    }
}
