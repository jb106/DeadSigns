using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsDatabase : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static ItemsDatabase Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    public Dictionary<string, Consommable> consommablesList = new Dictionary<string, Consommable>();
    private Dictionary<string, Weapon> weaponsList = new Dictionary<string, Weapon>();
    private Dictionary<string, Various> variousList = new Dictionary<string, Various>();

    private void Start()
    {
        LoadDatabase();
    }

    void LoadDatabase()
    {
        foreach (Consommable consommable in Resources.LoadAll("ItemDatabase/Consommables"))
        {
            consommablesList.Add(consommable.IdName, consommable);
        }
        foreach (Weapon weapon in Resources.LoadAll("ItemDatabase/Weapons"))
        {
            weaponsList.Add(weapon.IdName, weapon);
        }
        foreach (Various various in Resources.LoadAll("ItemDatabase/Various"))
        {
            variousList.Add(various.IdName, various);
        }
    }

    public Consommable GetConsommableByName(string name)
    {
        if (consommablesList.ContainsKey(name))
            return consommablesList[name];

        return null;
    }

    public Weapon GetWeaponByName(string name)
    {
        if (weaponsList.ContainsKey(name))
            return weaponsList[name];

        return null;
    }

    public Various GetVariousByName(string name)
    {
        if (variousList.ContainsKey(name))
            return variousList[name];

        return null;
    }

}
