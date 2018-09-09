using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Melee, Distance }

[CreateAssetMenu(fileName = "New Weapon Item")]
[ExecuteInEditMode]
public class Weapon : Item
{

    private string itemType = "weapon";
    public WeaponType type;
    [Range(0.5f, 100f)] public float Puissance = 1.0f;
    [Range(0.5f, 10f)] public float Precision = 1.0f;
    [Range(0.1f, 5f)] public float Cadence = 1.0f;
    [Range(1, 50)] public int Munitions = 1;
    [Range(0f, 5f)] public float Recul = 1.0f;
    [Range(1, 10)] public int Tirs = 1;

    public override string GetItemType()
    {
        return itemType;
    }

}
