using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemRealType { Consommable, Weapon, Various };

public class ItemReal : MonoBehaviour {

    public ItemRealType _itemType;
    public string _itemId;

    public void GetItem()
    {
        if(Inventory.Instance.CheckSpaceAvailable(GetItemType(), _itemId))
        {
            Inventory.Instance.AddItem(GetItemType(), _itemId);
            Destroy(gameObject);
        }

    }

    string GetItemType()
    {
        if (_itemType == ItemRealType.Consommable)
            return "consommable";
        else if (_itemType == ItemRealType.Weapon)
            return "weapon";
        else if (_itemType == ItemRealType.Various)
            return "various";

        return null;
    }

}
