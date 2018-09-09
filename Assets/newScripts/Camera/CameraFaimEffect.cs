using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class CameraFaimEffect : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static CameraFaimEffect Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] Texture2D _faimTexture = null;
    [SerializeField] Texture2D _faimNormal = null;

    [SerializeField] private float _faimAmount = 0.0f;
    [SerializeField] private float _minFaimAmount = 0.0f;

    [SerializeField] private float _distorsion = 1.0f;

    [SerializeField] private Shader _shader = null;
    private Material _material = null;


    public float faimAmount { get { return _faimAmount; } set { _faimAmount = value; } }
    public float minFaimAmount { get { return _minFaimAmount; } set { _minFaimAmount = value; } }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_shader == null) return;

        if (_material == null)
        {
            _material = new Material(_shader);
        }

        if (_material == null) return;

        //Envoyer les données au shader
        _material.SetTexture("_Tex", _faimTexture);
        _material.SetTexture("_Normal", _faimNormal);

        _material.SetFloat("_Distorsion", _distorsion);
        _material.SetFloat("_Amount", _faimAmount);

        Graphics.Blit(source, destination, _material);
    }
}
