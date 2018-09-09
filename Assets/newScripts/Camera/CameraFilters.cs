using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering.PostProcessing;

public class CameraFilters : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static CameraFilters Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    PostProcessVolume effectsVolume;

    //Variables pour la valeur de blend de chaque effet
    public float healthBlend = 0f;
    public float hungerBlend = 0f;
    public float sleepBlend = 0f;

    public float maxHealthBlend;
    public float maxHungerBlend;
    public float maxSleepBlend;


    //Variables pour le délai de blend
    private float _lerpDelay = 0.1f;

    private void Start()
    {
        effectsVolume = GameObject.Find("PlayerEffectsPostProcessingVolume").GetComponent<PostProcessVolume>();

        StartCoroutine(EffectsUpdate());
    }


    /* Cette Coroutine va actualiser constamment la valeur de blend des différents effets suivant leurs variables respectives
     * Le système sera le suivant: on donne une valeur à atteindre pour chaque effet (donné par le script HealthEffects)
     * Et la Coroutine s'occupe de lerp jusqu'à cette valeur, c'est aussi simple que ça !
     */ 
    IEnumerator EffectsUpdate()
    {
        while(true)
        {
            SetBloodLerp(Mathf.Clamp(healthBlend, 0, maxHealthBlend));
            SetHungerLerp(Mathf.Clamp(hungerBlend, 0, maxHungerBlend));
            SetSleepLerp(Mathf.Clamp(sleepBlend, 0, maxSleepBlend));

            yield return null;
        }
    }

    private void SetBloodLerp(float bloodAmount)
    {
        Blood bloodCustom = null;
        effectsVolume.profile.TryGetSettings(out bloodCustom);

        bloodCustom.blend.value = Mathf.Lerp(bloodCustom.blend.value, bloodAmount, _lerpDelay);
    }

    private void SetHungerLerp(float hungerAmount)
    {
        Hunger hungerCustom = null;
        effectsVolume.profile.TryGetSettings(out hungerCustom);

        hungerCustom.blend.value = Mathf.Lerp(hungerCustom.blend.value, hungerAmount, _lerpDelay);
    }

    private void SetSleepLerp(float sleepAmount)
    {
        ChromaticAberration sleepCustom = null;
        effectsVolume.profile.TryGetSettings(out sleepCustom);

        sleepCustom.intensity.value = Mathf.Lerp(sleepCustom.intensity.value, sleepAmount, _lerpDelay);
    }


    public void SetBlur(bool active)
    {

    }

    public void SetVigneting(float factor)
    {

    }

    public void SetMotionBlur(float factor)
	{
		
    }

    public void SetChromaticAberration(float factor)
    {

    }

}
