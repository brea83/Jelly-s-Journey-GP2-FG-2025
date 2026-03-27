using UnityEngine;

public class RangedProjectile : MonoBehaviour
{
    protected PlayingState _updateState;

    public GameObject _target;
    Rigidbody _rb;
    Vector3 _direction;
    public int _damage;
    public bool _trajectory;
    Vector3 startPos;
    Vector3 endPos;

    public float _velocity;
    public float _lifeSpan;
    private float timer = 0;

    void Start()
    {
        _updateState = GameManager.Instance.GetState<PlayingState>();
        if(_updateState != null)
        {
            _updateState.StateUpdate.AddListener(ManagedUpdate);
        }
        else
        {
            Debug.Log("tried to add listener but my UpdateProjectile == null");
        }

        _rb = GetComponent<Rigidbody>();
        
        startPos = transform.position;
        endPos = _target.transform.position;

        if(!_trajectory)
        {
            Shot();
        }
        
    }

    void Shot()
    {
        _direction = new Vector3(endPos.x - startPos.x, 0.0f, endPos.z - startPos.z).normalized * _velocity;
        _rb.AddForce(_direction, ForceMode.VelocityChange);
    }

    void ManagedUpdate()
    {
        timer += Time.deltaTime;

        if (timer > _lifeSpan)
        {
            Destroy(gameObject);
        }

        if (_trajectory)
        {
            transform.position = Trajectory(timer);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            int damage = _damage;
            if (damage > 0)
            {
                PlayerHealth player = other.gameObject.GetComponent<PlayerHealth>();
                player.TakeDamage();
            }
           
            Destroy(gameObject);
        }
    }

    Vector3 Trajectory(float t)
    {
        Vector3 midPoint = new Vector3((startPos.x + endPos.x)/2, (startPos.y + endPos.y)/2 + 4, (startPos.z + endPos.z)/2);

        Vector3 ac = Vector3.Lerp(startPos, midPoint, t);
        Vector3 cb = Vector3.Lerp(midPoint, endPos, t);
        return Vector3.Lerp(ac, cb, t);
    }

}
