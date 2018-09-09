using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem
{
    public string _itemType;
    public string _itemId;
    public GameObject _itemSlot;
    public Item _item;
    public int _stackNumber = 1;
    public bool _selected = false;

    public InventoryItem(string itemType, string itemId, GameObject itemSlot, Item item)
    {
        _itemType = itemType;
        _itemId = itemId;
        _itemSlot = itemSlot;
        _item = item;
    }

    public void SelectItem()
    {
        _selected = true;
        _itemSlot.GetComponent<Image>().color = Inventory.Instance._selectedSlotColor;
        Inventory.Instance.SetItemDescription(_item);
        Model3DPreview.Instance.DeleteCurrentModel();
        Model3DPreview.Instance.SetNewModel(_item.prefab);

        foreach (InventoryItem invItem in Inventory.Instance._listItems)
            if (invItem != this)
                invItem.DeselectItem();
    }

    public void DeselectItem()
    {
        _selected = false;
        _itemSlot.GetComponent<Image>().color = _itemSlot.GetComponent<ItemSlot>()._baseColor;
    }

    public void AddStack()
    {
        if (_stackNumber + 1 <= _item.MaxStack)
            _stackNumber++;

        _itemSlot.GetComponent<ItemSlot>().RefreshStack();
    }

    public void RemoveStack()
    {
        _stackNumber--;
        _itemSlot.GetComponent<ItemSlot>().RefreshStack();            
    }

    public void ThrowItem()
    {
        Inventory.Instance.ThrowInFront(_item.prefab);

        DeleteItem();
    }

    public void DeleteItem()
    {
        Statistics.Instance.getPlayerStat("poids").changeValue(-_item.Poids);

        if (_stackNumber - 1 == 0)
        {
            //Si on supprime l'objet, cela assure la désactivation de la description de l'objet maintenant inexistant
            if (_selected)
            {
                Inventory.Instance.SetItemDescription(null);
                Model3DPreview.Instance.DeleteCurrentModel();
            }

            DeleteSlot();
        }
        else
            RemoveStack();
    }

    public void DeleteSlot()
    {
        Inventory.Instance._listItems.Remove(this);
        GameObject.Destroy(_itemSlot);
    }
}

public class Inventory : MonoBehaviour
{

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static Inventory Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private AudioCollection _interact = null;
    private PlayerStat _poidsPlayer = null;

    private Toggle _consommablesToggle = null;
    private Toggle _weaponsToggle = null;
    private Toggle _variousToggle = null;

    //Références UI
    Transform _listingItems = null;

    public GameObject _consommableSlot;
    public GameObject _weaponSlot;
    public GameObject _variousSlot;

    //Couleurs des slots
    public Color _selectedSlotColor;

    public List<InventoryItem> _listItems = new List<InventoryItem>();

    private void Start()
    {
        _listingItems = GameObject.Find("listing_items").transform;
        _poidsPlayer = Statistics.Instance.getPlayerStat("poids");

        _consommablesToggle = GameObject.Find("nourriture_onglet").GetComponent<Toggle>();
        _weaponsToggle = GameObject.Find("armes_onglet").GetComponent<Toggle>();
        _variousToggle = GameObject.Find("various_onglet").GetComponent<Toggle>();

        SetItemDescription(null);
    }

    public void AddItem(string itemType, string idName)
    {
        //TOUTE PREMIERE VERIFICATION
        //--- Si le joueur à de la place dans l'inventaire
        //--- Autrement dit si la variable poids n'excède pas le poids maximum avec le nouveau item
        if (itemType == "consommable")
            if (_poidsPlayer._value + ItemsDatabase.Instance.GetConsommableByName(idName).Poids > _poidsPlayer._maxValue)
                return;

            else if (itemType == "weapon")
                if (_poidsPlayer._value + ItemsDatabase.Instance.GetWeaponByName(idName).Poids > _poidsPlayer._maxValue)
                    return;

                else if (itemType == "various")
                    if (_poidsPlayer._value + ItemsDatabase.Instance.GetVariousByName(idName).Poids > _poidsPlayer._maxValue)
                        return;


        //On initialise une list d'items pour par la suite les comparer et voir ou rajouter si besoin
        List<InventoryItem> checkItems = ReturnExistingItems(itemType, idName);

        //Si la liste est vide, on ajoute simplement l'item dans l'inventaire
        if (checkItems.Count == 0)
        {
            AddItemPhysically(itemType, idName);
        }
        else //Sinon on va comparer chaque item et voir si il reste des places dans les stacks
        {
            bool hasFindItem = false;

            foreach (InventoryItem checkItem in checkItems)
            {
                if (checkItem._stackNumber + 1 <= checkItem._item.MaxStack)
                {
                    //Si il reste des places, on ajoute une stack dans l'item trouvé et on s'arrête la
                    checkItem.AddStack();
                    hasFindItem = true;
                    break;
                }
            }

            //Si il ne reste pas de place nulle part, on crée un nouveau slot
            if (!hasFindItem)
                AddItemPhysically(itemType, idName);
        }

        //Finalement on ajoute le poids de l'item au poids actuel de l'inventaire
        if (itemType == "consommable")
            _poidsPlayer.changeValue(ItemsDatabase.Instance.GetConsommableByName(idName).Poids);
        else if (itemType == "weapon")
            _poidsPlayer.changeValue(ItemsDatabase.Instance.GetWeaponByName(idName).Poids);
        else if (itemType == "various")
            _poidsPlayer.changeValue(ItemsDatabase.Instance.GetVariousByName(idName).Poids);

        //Son quand un item est ajouté dans l'inventaire
        AudioManager.instance.PlayOneShotSound("Player", _interact[0], transform.position, _interact.volume, _interact.spatialBlend, _interact.priority);
    }

    private void AddItemPhysically(string itemType, string idName)
    {
        //---------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        //Création de deux variables vides qui seront assignées ou non par la suite
        Item newItem = null;
        GameObject newSlot = null;


        if (itemType == "consommable")
        {
            newItem = ItemsDatabase.Instance.GetConsommableByName(idName);
            newSlot = Instantiate(_consommableSlot, _listingItems);
        }
        else if (itemType == "weapon")
        {
            newItem = ItemsDatabase.Instance.GetWeaponByName(idName);
            newSlot = Instantiate(_weaponSlot, _listingItems);
        }
        else if (itemType == "various")
        {
            newItem = ItemsDatabase.Instance.GetVariousByName(idName);
            newSlot = Instantiate(_variousSlot, _listingItems);
        }

        //Vérification si l'item existe, sinon détruire le slot fraîchement crée et retourner la fonction
        if (newItem == null)
        {
            Destroy(newSlot);
            return;
        }

        //---------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        //On ajoute l'item à la liste générale de tout les objets
        InventoryItem newInventoryItem = new InventoryItem(itemType, idName, newSlot, newItem);

        _listItems.Add(newInventoryItem);

        //On assigne des attributs au slot crée et on appelle sa fonction d'initialisation qui permet d'actualiser les 
        //informations sur le slot dans l'UI (Nom, Poids, Image ...)
        newSlot.GetComponent<ItemSlot>()._inventoryItem = newInventoryItem;
        newSlot.GetComponent<ItemSlot>().Initialize();

        UpdateItemsFilters();

    }

    private List<InventoryItem> ReturnExistingItems(string type, string idName)
    {
        List<InventoryItem> inventoryItemsList = new List<InventoryItem>();

        foreach (InventoryItem invItem in _listItems)
        {
            if (invItem._itemType == type && invItem._itemId == idName)
            {
                inventoryItemsList.Add(invItem);
            }
        }
        return inventoryItemsList;
    }

    public bool CheckItemExist(string type, string idName)
    {
        if (type.Length == 0)
        {
            foreach (InventoryItem invItem in _listItems)
            {
                if (invItem._itemId == idName)
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (InventoryItem invItem in _listItems)
            {
                if (invItem._itemType == type && invItem._itemId == idName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CheckSpaceAvailable(string itemType, string itemId)
    {
        if (itemType == "consommable")
        {
            Consommable conso = ItemsDatabase.Instance.GetConsommableByName(itemId);
            if (_poidsPlayer._value + conso.Poids <= _poidsPlayer._maxValue)
                return true;
        }
        else if (itemType == "weapon")
        {
            Weapon weapon = ItemsDatabase.Instance.GetWeaponByName(itemId);
            if (_poidsPlayer._value + weapon.Poids <= _poidsPlayer._maxValue)
                return true;
        }
        else if (itemType == "various")
        {
            Various various = ItemsDatabase.Instance.GetVariousByName(itemId);
            if (_poidsPlayer._value + various.Poids <= _poidsPlayer._maxValue)
                return true;
        }

        return false;
    }

    public void ThrowInFront(GameObject prefab)
    {
        GameObject throwed = Instantiate(prefab);
        throwed.transform.position = Camera.main.transform.position;
        throwed.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * (15 + (throwed.GetComponent<Rigidbody>().mass * 10)));
    }


    //Fonction qui va consommer un item
    public void ConsumeItem(InventoryItem invItem)
    {
        Consommable toConsume = ItemsDatabase.Instance.GetConsommableByName(invItem._itemId);

        //Nourriture simple
        if (toConsume.type == ConsommableType.Nourriture)
        {
            HealthEffects.Instance.ChangeFaim(toConsume.Valeur);
        }
        else if (toConsume.type == ConsommableType.Boisson)
        {
            HealthEffects.Instance.ChangeSoif(toConsume.Valeur);
        }
        else if (toConsume.type == ConsommableType.Guerison)
        {
            HealthEffects.Instance.Heal(toConsume.Valeur);
        }
        else if (toConsume.type == ConsommableType.NourritureBlessante)
        {
            HealthEffects.Instance.ChangeFaim(toConsume.Valeur);
            HealthEffects.Instance.TakeDamage(toConsume.Valeur);
        }
        else if (toConsume.type == ConsommableType.NourritureEtBoisson)
        {
            HealthEffects.Instance.ChangeFaim(toConsume.Valeur);
            HealthEffects.Instance.ChangeSoif(toConsume.Valeur);
        }
        else if (toConsume.type == ConsommableType.Poison)
        {

        }

    }


    //Fonction qui met ou pas une description (fonction appelée quand on sélectionne un slot)
    public void SetItemDescription(Item item)
    {
        if (item)
        {
            string description = "";
            description += item.Description;

            //Juste pour les armes, on va ajouter quelques statistiques à la description
            if (item.GetItemType() == "weapon")
            {
                Weapon weapon = ItemsDatabase.Instance.GetWeaponByName(item.IdName);
                description += "\n\nPuissance: " + weapon.Puissance;
                if (weapon.type == WeaponType.Distance)
                {
                    description += "\nPrécision: " + weapon.Precision;
                    description += "\nCadence: " + weapon.Cadence;
                    description += "\nCapacité chargeur: " + weapon.Munitions;
                }

            }

            Tablet.Instance.descriptionText.text = description;
        }
        else
            Tablet.Instance.descriptionText.text = "Aucun objet sélectionné";

    }

    // Fonction pour gérer le filtre d'items dans l'inventaire, qui sera appelé seulement quand on change les méthodes de filtrages
    // ou quand un nouvel objet est récupéré pour actualiser le tout
    public void UpdateItemsFilters()
    {
        foreach (InventoryItem item in _listItems)
        {
            if (item._itemType == "consommable")
            {
                if (_consommablesToggle.isOn)
                    item._itemSlot.SetActive(true);
                else
                    item._itemSlot.SetActive(false);
            }
            else if (item._itemType == "weapon")
            {
                if (_weaponsToggle.isOn)
                    item._itemSlot.SetActive(true);
                else
                    item._itemSlot.SetActive(false);
            }
            else if (item._itemType == "various")
            {
                if (_variousToggle.isOn)
                    item._itemSlot.SetActive(true);
                else
                    item._itemSlot.SetActive(false);
            }
        }




    }

}
