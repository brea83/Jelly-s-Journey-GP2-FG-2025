using TMPro;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    public GameObject textPrefab;

    public void DamageFloat(string DamageTaken)
    {
        float randomNumber = Random.Range(-1.5f, 1.5f);
        Vector3 pos = new Vector3(transform.position.x + randomNumber, transform.position.y + 1, transform.position.z);
        GameObject speechInstance = Instantiate(textPrefab, pos, Quaternion.identity);
        
        speechInstance.GetComponent<TMP_Text>().text = DamageTaken;

        Destroy(speechInstance, 1f);
    }
}
