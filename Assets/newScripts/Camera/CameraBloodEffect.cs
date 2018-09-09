using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class CameraBloodEffect : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static CameraBloodEffect Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] Texture2D _bloodTexture = null;
    [SerializeField] Texture2D _bloodNormal = null;

    [SerializeField] private float _bloodAmount = 0.0f;
    [SerializeField] private float _minBloodAmount = 0.0f;

    [SerializeField] private float _distorsion = 1.0f;
    [SerializeField] private bool _autoFade = true;
    [SerializeField] private float _fadeSpeed = 0.05f;

    [SerializeField]
    private Shader _shader = null;
    private Material _material = null;


    public float bloodAmount { get { return _bloodAmount; } set { _bloodAmount = value; } }
    public float minBloodAmount { get { return _minBloodAmount; } set { _minBloodAmount = value; } }

    public float fadeSpeed { get { return _fadeSpeed;} set { _fadeSpeed = value; } }
    public bool autoFade { get { return _autoFade; } set { autoFade = value; } }


    private void Update()
    {
        if(_autoFade)
        {
            _bloodAmount -= _fadeSpeed * Time.deltaTime;
            _bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);
        }
    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_shader == null) return;

        if(_material==null)
        {
            _material = new Material(_shader);
        }

        if (_material == null) return;

        //Envoyer les données au shader
        _material.SetTexture("_BloodTex", _bloodTexture);
        _material.SetTexture("_BloodNormal", _bloodNormal);

        _material.SetFloat("_BloodDistorsion", _distorsion);
        _material.SetFloat("_BloodAmount", _bloodAmount);

        Graphics.Blit(source, destination, _material);
    }
}
