using UnityEngine;

public class Keycard : MonoBehaviour, IInteractable
{
    public string keyID;

    public string GetDescription() => "Press E to get " + keyID;

    public void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<PlayerInventory>().AddKey(keyID);
            Debug.Log("Get Key: " + keyID);
            Destroy(gameObject);
        }
    }
}
