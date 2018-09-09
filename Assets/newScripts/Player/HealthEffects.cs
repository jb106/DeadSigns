using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--- C'est le script qui va gérer les effets provoquées par les valeurs des barres de survie
//-------------------------- A savoir la faim, la soif, le sommeil, ainsi que la barre de vie
//- Et ce script peut aussi contenir toutes les fonctions liées aux dégâts que le joueur subit

    /* L'état de fonctionnement imaginé est le suivant:
    --- Le script Tablet à une fonction qui met à jour de manière optimisée
    --- les barres de vie. L'idée est d'appeler une fonction ici qui mettra
    --- à jour la vision ou les effets du joueur, à chaque MAJ de la tablette
    */


public class HealthEffects : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    [SerializeField] private AudioCollection _hurt = null;
    [SerializeField] private AudioCollection _eat = null;
    [SerializeField] private AudioCollection _drink = null;
    [SerializeField] private AudioCollection _soin = null;

    public static HealthEffects Instance = null;

    private void Awake()
    {
        Instance = this;
    }


    private float healthStartEffect = 0.3f;
    private float hungerStartEffect = 0.3f;
    private float thirstStartEffect = 0.3f;
    private float sleepStartEffect = 0.3f;


    private void Update()
    {

    }


    public void UpdateFaimEffects()
    {
        PlayerStat faimStat = Statistics.Instance.getPlayerStat("faim");
        //Si la faim est inférieur à 30% alors on commence à ressentir les effets sur la vision
        if (faimStat._value < (faimStat._maxValue * hungerStartEffect))
        {
            float deficit = faimStat._value / (faimStat._maxValue * hungerStartEffect);

            deficit = deficit - 1;
            //Si la valeur est négative, on la rend positive
            if (deficit < 0)
                deficit = -deficit;

            //On assigne au script CameraFilters la valeur de blend
            CameraFilters.Instance.hungerBlend = deficit;
        }
        else
        {
            CameraFilters.Instance.hungerBlend = 0;
        }

        //Partie qui va lancer une coroutine si la vie est à 0 pour dégrader progressivement la santé
        if (faimStat._empty && !faimStat._degradation)
            StartCoroutine(DegradationVie(faimStat));
        
    }

    public void UpdateSoifEffects()
    {
        PlayerStat soifStat = Statistics.Instance.getPlayerStat("soif");
        //Si la soif est inférieur à 30% alors on commence à ressentir les effets sur la vision
        if (soifStat._value < (soifStat._maxValue * thirstStartEffect))
        {
            float deficit = soifStat._value / (soifStat._maxValue * thirstStartEffect);
            
        }
        else
        {
            
        }

        //Partie qui va lancer une coroutine si la vie est à 0 pour dégrader progressivement la santé
        if (soifStat._empty && !soifStat._degradation)
            StartCoroutine(DegradationVie(soifStat));

    }


    public void UpdateSommeilEffects()
    {
        PlayerStat sommeilStat = Statistics.Instance.getPlayerStat("sommeil");
        //Si la soif est inférieur à 30% alors on commence à ressentir les effets sur la vision
        if (sommeilStat._value < (sommeilStat._maxValue * sleepStartEffect))
        {
            float deficit = sommeilStat._value / (sommeilStat._maxValue * sleepStartEffect);

            deficit = deficit - 1;
            //Si la valeur est négative, on la rend positive
            if (deficit < 0)
                deficit = -deficit;

            //On assigne au script CameraFilters la valeur de blend
            CameraFilters.Instance.sleepBlend = deficit;
        }
        else
        {
            CameraFilters.Instance.sleepBlend = 0;
        }

        //Partie qui va lancer une coroutine si la vie est à 0 pour dégrader progressivement la santé
        if (sommeilStat._empty && !sommeilStat._degradation)
            StartCoroutine(DegradationVie(sommeilStat));

    }

    public void UpdateHealthEffects()
    {
        PlayerStat healthStat = Statistics.Instance.getPlayerStat("vie");
        //Si la faim est inférieur à 30% alors on commence à ressentir les effets sur la vision
        if (healthStat._value < (healthStat._maxValue * healthStartEffect))
        {
            float deficit = healthStat._value / (healthStat._maxValue * healthStartEffect);
            //On obtient ici une valeur qui est a 1 quand la vie est à 20% et une valeur qui est a 0 quand la vie est a 0%

            //Il faut donc la transformer en son inverse, que quand on est à 20% la valeur soit à 0 etc.
            deficit = deficit - 1;
            //Si la valeur est négative, on la rend positive
            if (deficit < 0)
                deficit = -deficit;

            //On assigne au script CameraFilters la valeur de blend
            CameraFilters.Instance.healthBlend = deficit;
        }
        else
        {
            //Si la vie est au dessus de 20% alors on peut rendre l'effet égal à 0 car on ne veut pas afficher de sang passé cette valeur
            CameraFilters.Instance.healthBlend = 0;
        }
    }

    IEnumerator DegradationVie(PlayerStat stat)
    {
        print("degradationStart");
        stat._degradation = true;

        while (stat._empty)
        {
            TakeDamage(10f);
            yield return new WaitForSeconds(3f);
        }

        stat._degradation = false;
        print("degradationEnd");
    }


    public void TakeDamage(float damage)
    {
        PlayerStat playerHealth = Statistics.Instance.getPlayerStat("vie");
        playerHealth.changeValue(-damage);
        

        AudioManager.instance.PlayOneShotSound("Player", _hurt[0], transform.position, _hurt.volume, _hurt.spatialBlend, _hurt.priority);
    }

    public void Heal(float value)
    {
        PlayerStat playerHealth = Statistics.Instance.getPlayerStat("vie");
        playerHealth.changeValue(value);
        
        AudioManager.instance.PlayOneShotSound("Player", _soin[0], transform.position, _soin.volume, _soin.spatialBlend, _soin.priority);
    }

    public void ChangeFaim(float value)
    {
        AudioManager.instance.PlayOneShotSound("Player", _eat[0], transform.position, _eat.volume, _eat.spatialBlend, _eat.priority);
        Statistics.Instance.getPlayerStat("faim").changeValue(value);
    }

    public void ChangeSoif(float value)
    {
        AudioManager.instance.PlayOneShotSound("Player", _drink[0], transform.position, _drink.volume, _drink.spatialBlend, _drink.priority);
        Statistics.Instance.getPlayerStat("soif").changeValue(value);
    }


}
