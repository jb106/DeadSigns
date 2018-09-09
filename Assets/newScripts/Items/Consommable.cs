using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConsommableType { Nourriture, Boisson, NourritureEtBoisson, Guerison, Poison, NourritureBlessante }


[CreateAssetMenu(fileName = "New Consommable Item")]
[ExecuteInEditMode]
public class Consommable : Item {

    private string itemType = "consommable";
    public ConsommableType type;
    public string required;
    [Range(0.5f, 100f)] public float Valeur = 1.0f;

    public override string GetItemType()
    {
        return itemType;
    }

}

