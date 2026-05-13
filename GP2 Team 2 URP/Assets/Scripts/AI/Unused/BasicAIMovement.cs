using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class BasicAIMovement : MonoBehaviour
{
    public Transform Target;
    public float AttackDistance;
    public float LineOfSight;

    private NavMeshAgent _agent;
    private float _distanceFromPlayer;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    
    void Update()
    {
        _distanceFromPlayer = Vector3.Distance(_agent.transform.position, Target.position);

        if(_distanceFromPlayer <= LineOfSight)
        {
            if (_distanceFromPlayer < AttackDistance)
            {
                _agent.isStopped = true;
                Debug.Log("Attacking");
            }
            else
            {
                _agent.isStopped = false;
                _agent.destination = Target.position;
            }
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, LineOfSight);
    }
}
