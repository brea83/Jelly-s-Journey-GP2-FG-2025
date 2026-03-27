using UnityEngine;

public class UIBillboarding : MonoBehaviour
{
    private Camera _camera;
    protected PlayingState _updateState;

    void Start()
    {
        _updateState = GameManager.Instance.GetState<PlayingState>();
        if (_updateState != null)
        {
            _updateState.StateUpdate.AddListener(UpdateView);
        }
        else
        {
            Debug.Log("tried to add listener but my UpdateView == null");
        }
    }

    private void Awake()
    {
        _camera = Camera.main;
    }

    // Update is called once per frame
    void UpdateView()
    {
        transform.forward = _camera.transform.forward;
    }
}
