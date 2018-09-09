using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public string IdName = string.Empty;
    public string Nom = string.Empty;
    [TextArea] public string Description;
    [TextArea] public string DescriptionShop;
    [Range(0.5f, 20f)] public float Poids = 1.0f;
    public float prix;
    public Sprite ImagePreview;
    public GameObject prefab;
    [Range(1, 99)] public float MaxStack = 1;

    public abstract string GetItemType();
}
