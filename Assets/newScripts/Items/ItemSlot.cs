using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ItemSlot : MonoBehaviour {

    public InventoryItem _inventoryItem;
    public Color _baseColor;

    public void Initialize()
    {
        _baseColor = GetComponent<Image>().color;

        transform.Find("name").GetComponent<TextMeshProUGUI>().text = _inventoryItem._item.Nom;
        transform.Find("previewImage").GetComponent<Image>().sprite = _inventoryItem._item.ImagePreview;
        transform.Find("weight").GetComponent<TextMeshProUGUI>().text = _inventoryItem._item.Poids.ToString() + " kg";

        if (_inventoryItem._itemType=="consommable")
        {
            Consommable itemConsommable = ItemsDatabase.Instance.GetConsommableByName(_inventoryItem._itemId);
            transform.Find("value").GetComponent<TextMeshProUGUI>().text = "V: " + itemConsommable.Valeur.ToString();
        }
        else if(_inventoryItem._itemType=="weapon")
        {
            Weapon itemWeapon = ItemsDatabase.Instance.GetWeaponByName(_inventoryItem._itemId);
            transform.Find("value").GetComponent<TextMeshProUGUI>().text = "P: " + itemWeapon.Puissance.ToString();
        }
    }

    //Fonction qui sera appelée seulement par les slots consommables qui auront le bouton assigné à la fonction
    public void ConsumeItem()
    {
        //Checker si on a besoin d'un autre objet pour le consommer
        Consommable conso = ItemsDatabase.Instance.GetConsommableByName(_inventoryItem._itemId);
        //On va forcément vérifier si un objet de type Various est présent dans l'inventaire. En effet, les seuls objets required seront de type Various
        if (conso.required != "")
            if (!Inventory.Instance.CheckItemExist("", conso.required))
                return;

        Inventory.Instance.ConsumeItem(_inventoryItem);
        _inventoryItem.DeleteItem();
    }

    public void RefreshStack()
    {
        transform.Find("stackBack").Find("stack").GetComponent<TextMeshProUGUI>().text = _inventoryItem._stackNumber.ToString();
    }

    public void Throw()
    {
        _inventoryItem.ThrowItem();
    }

    public void SelectItem()
    {
        if (!_inventoryItem._selected)
            _inventoryItem.SelectItem();
        else
        {
            _inventoryItem.DeselectItem();
            Inventory.Instance.SetItemDescription(null);
            Model3DPreview.Instance.DeleteCurrentModel();
        }
    }

}
