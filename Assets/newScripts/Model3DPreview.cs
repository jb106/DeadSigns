using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Model3DPreview : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static Model3DPreview Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    public RenderTexture rText;
    public LayerMask previewMask;

    Camera _previewCam = null;
    GameObject _currentObjectPreview = null;

    private void Start()
    {
        GameObject temp = GameObject.Find("camPreview");
        

        /*
        new GameObject("camPreview");
        GameObject temp = GameObject.Find("camPreview");
        temp.AddComponent<Camera>();
        */

        _previewCam = temp.GetComponent<Camera>();

        /*
        _previewCam = temp.GetComponent<Camera>();
        _previewCam.targetTexture = rText;
        _previewCam.clearFlags = CameraClearFlags.SolidColor;
        _previewCam.backgroundColor = new Color(0, 0, 0);
        _previewCam.nearClipPlane = 0.01f;
        _previewCam.gameObject.layer = LayerMask.NameToLayer("Preview3D");
        _previewCam.cullingMask = previewMask;
        */



        new GameObject("previewSun");
        temp = GameObject.Find("previewSun");
        temp.layer = LayerMask.NameToLayer("Preview3D");
        temp.AddComponent<Light>();
        temp.GetComponent<Light>().type = LightType.Directional;
        temp.GetComponent<Light>().intensity = 0.4f;
        temp.GetComponent<Light>().cullingMask = previewMask;

    }

    public void SetNewModel(GameObject model)
    {
        _currentObjectPreview = Instantiate(model);
        _currentObjectPreview.gameObject.layer = LayerMask.NameToLayer("Preview3D");
        _currentObjectPreview.AddComponent<TurnAround>();
        Destroy(_currentObjectPreview.GetComponent<Rigidbody>());
        _currentObjectPreview.transform.position = _previewCam.transform.position + _previewCam.transform.forward/2f;

        Renderer objRender = _currentObjectPreview.GetComponent<Renderer>();
        float boundMax = objRender.bounds.max.x > objRender.bounds.max.y ? objRender.bounds.max.x : objRender.bounds.max.y;
        _previewCam.fieldOfView = GetFieldOfView(_currentObjectPreview.transform.position, boundMax);
    }

    public void DeleteCurrentModel()
    {
        GameObject tempObj = _currentObjectPreview;
        _currentObjectPreview = null;
        Destroy(tempObj);
    }

    float GetFieldOfView(Vector3 objectPosition, float objectHeight)
    {
        Vector3 diff = objectPosition - _previewCam.transform.position;
        float distance = Vector3.Dot(diff, _previewCam.transform.forward);
        float angle = Mathf.Atan((objectHeight * 1.2f) / distance);
        return angle * 2f * Mathf.Rad2Deg;
    }

}
