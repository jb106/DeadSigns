using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VariousType { Unlock, Utilitaire };

[CreateAssetMenu(fileName = "New Various Item")]
[ExecuteInEditMode]
public class Various : Item
{
    private string itemType = "various";
    public VariousType type;

    public override string GetItemType()
    {
        return itemType;
    }

}
