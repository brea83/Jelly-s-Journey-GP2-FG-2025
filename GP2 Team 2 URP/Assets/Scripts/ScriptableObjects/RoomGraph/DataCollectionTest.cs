using UnityEngine;


public class DataCollectionTest : MonoBehaviour
{
    //public stuff
    public string GetName() { return _Name; }
    public int GetNumber() { return _TestNumber; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // private and protected stuff-----------------------------
    [SerializeField]
    protected string _Name = "Default Name";

    [SerializeField]
    protected int _TestNumber = 0;
}
