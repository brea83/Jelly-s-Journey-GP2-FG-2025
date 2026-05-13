using UnityEngine;
using UnityEngine.Events;

public class LockableBarrier : MonoBehaviour
{

    public UnityEvent OnUnlocked;
    public UnityEvent OnLocked;

    public bool LocksDurringCombat = true;
    public bool UseOldBarrierAnimation = false;
    public bool Locked { get => _bIsLocked; }

    protected GameState _updateState;
    protected bool _bIsLocked;

    protected GameObject _barrier;

    [Header("Debug tools")]
    public Material UnlockedMaterial;
    public Material LockedMaterial;
    public Vector3 UnlockedOffset;
    private Vector3 _barrierLockedPosition;

    public float TempUnlockSpeed = 1f;
    private float _timer = 0f;
    private bool _isUnlocking;
    private bool _hasDisabled = false;

    private void OnEnable()
    {

        if (_hasDisabled)
        {
            SubscribeToPlayState();
            if (LocksDurringCombat)
            {

                NewEncounterManager encounterManager = GameManager.Instance.EncounterManager;
                if (encounterManager != null)
                {
                    encounterManager.OnEncounterStart.AddListener(OnEncounterStart);
                    encounterManager.OnEncounterEnd.AddListener(OnEncounterEnd);
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform childTransform = this.gameObject.FindComponentInChildWithTag<Transform>("Barrier");//this.transform.Find("Barrier");

        if (childTransform != null)
        {
            _barrier = childTransform.gameObject;
            _barrierLockedPosition = _barrier.transform.localPosition;
            //Debug.Log($"found {_barrier.name} in {this.name}");
        }
        if (!Locked)
        {
            Unlock();
        }
        if (LocksDurringCombat == true)
        {
            NewEncounterManager encounterManager = GameManager.Instance.EncounterManager;
            if (encounterManager != null)
            {
                encounterManager.OnEncounterStart.AddListener(OnEncounterStart);
                encounterManager.OnEncounterEnd.AddListener(OnEncounterEnd);
            }
        }

        SubscribeToPlayState();
    }

    private void ManagedUpdate()
    {

    }
    private void ManagedFixedUpdate()
    {
        if (_isUnlocking)
        {
            _barrier.transform.localPosition = Vector3.Lerp(_barrier.transform.localPosition, UnlockedOffset, _timer);
            if (_timer >= TempUnlockSpeed)
            {
                _barrier.transform.localPosition = UnlockedOffset;
                _timer = 0f;
                _isUnlocking = false;
            }
            _timer += Time.fixedDeltaTime;
        }
    }
    private void SubscribeToPlayState()
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
    }
    private void UnsubscribeToPlayState()
    {
        if (_updateState != null)
        {
            _updateState.StateUpdate.RemoveListener(ManagedUpdate);
            _updateState.StateFixedUpdate.RemoveListener(ManagedFixedUpdate);
        }
        else
        {
            Debug.Log("tried to remove listeners but myUpdateState == null");
        }
    }

    public void OnEncounterStart()
    {
        if (LocksDurringCombat)
        {
            Lock();
        }
    }

    public void OnEncounterEnd()
    {
        if (LocksDurringCombat)
        {
            Unlock();
        }
    }

    public void Lock()
    {
        _bIsLocked = true;
        if (_barrier == null) { return; }
        if (UseOldBarrierAnimation)
        {
            _isUnlocking = false;
            _barrier.GetComponent<BoxCollider>().enabled = true;
            //temp animation, remove this when animation is calling this function
            _barrier.transform.localPosition = _barrierLockedPosition;
            MeshRenderer barrierMesh = _barrier.GetComponent<MeshRenderer>();
            if (barrierMesh != null)
            {
                barrierMesh.material = LockedMaterial;
            }
        }
        else
        {
            _barrier.SetActive(true);
        }
        OnLocked?.Invoke();
    }

    public void Unlock()
    {
        _bIsLocked = false;
        if (_barrier == null) { return; }
        if (UseOldBarrierAnimation)
        {
            _barrier.GetComponent<BoxCollider>().enabled = false;
            //temp "animation", remove this when animation is calling this function
            _isUnlocking = true;
            _timer = 0f;
            _barrier.transform.localPosition = UnlockedOffset;
            MeshRenderer barrierMesh = _barrier.GetComponent<MeshRenderer>();
            if (barrierMesh != null)
            {
                barrierMesh.material = UnlockedMaterial;
            }
        }
        else
        {
            _barrier.SetActive(false);
        }
        OnUnlocked?.Invoke();

    }
}
