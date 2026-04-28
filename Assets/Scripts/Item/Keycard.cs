using UnityEngine;

public class Keycard : MonoBehaviour, IInteractable
{
    public string keyID;

    public string GetDescription() => "กด E เพื่อเก็บ " + keyID;

    public void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<PlayerInventory>().AddKey(keyID);
            Debug.Log("เก็บกุญแจเรียบร้อย: " + keyID);
            Destroy(gameObject);
        }
    }
}
