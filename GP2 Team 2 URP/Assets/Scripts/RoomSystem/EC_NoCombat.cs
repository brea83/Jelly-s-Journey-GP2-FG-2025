using NGAME;
using System;
using UnityEngine;

[Serializable]
public class EC_NoCombat : EntranceCondition
{
    public EC_NoCombat()
    {
        Name = "No Combat";
        Description = "Entrance requires there to be no ongoing combat to be traversable";
    }

    public override bool Evaluate()
    {
        NewEncounterManager encounters = GameManager.Instance.EncounterManager;
        bool result = !encounters.IsEncounterActive();
        Debug.Log($"!!!!==== Entry condition No Combat evaluates to {result}");
        return result;
    }
}
