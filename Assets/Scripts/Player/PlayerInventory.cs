using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private List<string> keyList = new List<string>();

    public void AddKey(string keyID)
    {
        if (!keyList.Contains(keyID))
        {
            keyList.Add(keyID);
        }
    }

    public bool HasKey(string keyID)
    {
        return keyList.Contains(keyID);
    }
}
