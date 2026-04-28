using UnityEngine;

public class ItemInteractable : MonoBehaviour, IInteractable
{
    public string itemName;
    public string GetDescription() => "Press E to get : " + itemName;

    public void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<PlayerInventory>().AddItem(itemName);
            Debug.Log("Got :" + itemName);
            Destroy(gameObject);
        }
    }
}
