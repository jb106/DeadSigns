using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tablet : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static Tablet Instance = null;

    private void Awake()
    {
        Instance = this;
    }


    //Références objets tablette --------------------------------------
    private GameObject _tabletParent = null;
    private GameObject _raccourciBarresParent = null;

    //Animator cardiogramme et texte
    private Animator _cardioAnimator = null;
    private TextMeshProUGUI _cardioText = null;

    //UI GameObjects tablette -----------------------------------------
    private TextMeshProUGUI _faimText;
    private TextMeshProUGUI _soifText;
    private TextMeshProUGUI _sommeilText;

    private TextMeshProUGUI _raccourciFaimText;
    private TextMeshProUGUI _raccourciSoifText;
    private TextMeshProUGUI _raccourciSommeilText;

    private Image _faimFill;
    private Image _soifFill;
    private Image _sommeilFill;

    private TextMeshProUGUI _poidsInventory;

    public TextMeshProUGUI descriptionText;


    //Variables privées -------------------------------
    [SerializeField] public bool tabletOn = false;

    private void Start()
    {

        //Récupération des références
        _tabletParent = GameObject.Find("TabletParent");
        _raccourciBarresParent = GameObject.Find("raccourciBarres");

        _cardioAnimator = GameObject.Find("cardiogramme").GetComponent<Animator>();
        _cardioText = GameObject.Find("cardioText").GetComponent<TextMeshProUGUI>();

        //Récupération de tout les UI de la tablette
        _faimText = GameObject.Find("faimPercent").GetComponent<TextMeshProUGUI>();
        _soifText = GameObject.Find("soifPercent").GetComponent<TextMeshProUGUI>();
        _sommeilText = GameObject.Find("sommeilPercent").GetComponent<TextMeshProUGUI>();

        _raccourciFaimText = GameObject.Find("raccourciFaimPercent").GetComponent<TextMeshProUGUI>();
        _raccourciSoifText = GameObject.Find("raccourciSoifPercent").GetComponent<TextMeshProUGUI>();
        _raccourciSommeilText = GameObject.Find("raccourciSommeilPercent").GetComponent<TextMeshProUGUI>();

        _faimFill = GameObject.Find("faimFill").GetComponent<Image>();
        _soifFill = GameObject.Find("soifFill").GetComponent<Image>();
        _sommeilFill = GameObject.Find("sommeilFill").GetComponent<Image>();

        _poidsInventory = GameObject.Find("poidsInventaire").GetComponent<TextMeshProUGUI>();

        descriptionText = GameObject.Find("descriptionText").GetComponent<TextMeshProUGUI>();

    }

    private void Update()
    {
        if (Input.GetButtonDown("tabletKey") && canOpenInventory())
            setTablet();

        if (Input.GetButtonDown("raccourciKey") && canOpenInventory())
            setRaccourcibars();


        updateStatsTablet();

        //Actualisation du cardiogramme vie 
        cardioAnimation();
    }

    bool canOpenInventory()
    {
        bool can = false;

        if (!FPSController.Instance.goThroughObstacle && !Hudv2.Instance._pauseMode)
            return true;

        return can;
    }

    void setRaccourcibars()
    {
        _raccourciBarresParent.SetActive(!_raccourciBarresParent.activeSelf);
    }

    public void setTablet()
    {
        Vector3 tabletPosition = new Vector3(0, tabletOn? 1000 : 0, 0);

        //Activation de la tablette
        _tabletParent.GetComponent<RectTransform>().localPosition = tabletPosition;

        //Affichage ou non de la souris
        Hudv2.Instance.setMouseCursor(tabletOn);

        //Restriction sur le joueur
        FPSController.Instance._canLook = tabletOn;
        FPSController.Instance._canMove = tabletOn;

        //Effet de filtre flou
        CameraFilters.Instance.SetBlur(!tabletOn);

        //Changement de state
        tabletOn = !tabletOn;

    }


    //Fonctions d'actualisation des données tablette
    void updateStatsTablet()
    {
        PlayerStat _stat = null;

        //Actualisation de l'affichage de la faim
        if (Statistics.Instance.getPlayerStat("faim")._hasChanged)
        {
            _stat = Statistics.Instance.getPlayerStat("faim");

            int percentValue = (int)((_stat._value / _stat._maxValue) * 100);
            _faimText.text = (percentValue).ToString() + "%";
            _raccourciFaimText.text = (percentValue).ToString() + "%";
            _faimFill.fillAmount = (_stat._value / _stat._maxValue);

            //On appel ici le script HealthEffects qui va actualiser les effets de la faim 
            HealthEffects.Instance.UpdateFaimEffects();

            _stat._hasChanged = false;
        }

        //Actualisation de l'affichage de la soif
        if (Statistics.Instance.getPlayerStat("soif")._hasChanged)
        {
            _stat = Statistics.Instance.getPlayerStat("soif");

            int percentValue = (int)((_stat._value / _stat._maxValue) * 100);
            _soifText.text = (percentValue).ToString() + "%";
            _raccourciSoifText.text = (percentValue).ToString() + "%";
            _soifFill.fillAmount = (_stat._value / _stat._maxValue);

            //On appel ici le script HealthEffects qui va actualiser les effets de la soif 
            HealthEffects.Instance.UpdateSoifEffects();

            _stat._hasChanged = false;
        }

        //Actualisation de l'affichage du sommeil
        if (Statistics.Instance.getPlayerStat("sommeil")._hasChanged)
        {
            _stat = Statistics.Instance.getPlayerStat("sommeil");

            int percentValue = (int)((_stat._value / _stat._maxValue) * 100);
            _sommeilText.text = (percentValue).ToString() + "%";
            _raccourciSommeilText.text = (percentValue).ToString() + "%";
            _sommeilFill.fillAmount = (_stat._value / _stat._maxValue);

            //On appel ici le script HealthEffects qui va actualiser les effets du sommeil
            HealthEffects.Instance.UpdateSommeilEffects();

            _stat._hasChanged = false;
        }

        //Actualisation de l'affichage du poids
        if(Statistics.Instance.getPlayerStat("poids")._hasChanged)
        {
            _stat = Statistics.Instance.getPlayerStat("poids");

            //Petit calcul pour paramétrer la vitesse du joueur par rapport au poids qu'il porte, en assignant une variable tiredFactor qui se trouve dans 
            //Le FPS controller. Ce calcul devra retourner une vitesse mini de 0.75 et une vitesse maxi de 1.2 (ecart de 0.45)
            float tiredFactor = _stat._value / _stat._maxValue;
            tiredFactor = 1.2f - (0.45f * tiredFactor);
            FPSController.Instance.tiredFactor = tiredFactor;

            _poidsInventory.text = "Poids total: " + _stat._value.ToString() + "/" + _stat._maxValue + " kg";
            _stat._hasChanged = false;
        }
    }

    // Partie qui gère l'affichage dans le compartiment cardiogramme de
    // l'animation du rythme cardiaque ainsi que l'indication texte de la vie
    void cardioAnimation()
    {
        if (Statistics.Instance.getPlayerStat("vie")._hasChanged)
        {
            float playerHealth = Statistics.Instance.getPlayerStat("vie")._value;

            if (playerHealth < 33f) //Etat mauvais
            {
                _cardioAnimator.SetInteger("state", 1);
                _cardioText.text = "Danger";
                _cardioAnimator.SetFloat("speed", 1f);
            }
            else if (playerHealth >= 33f && playerHealth < 66f) //Etat moyen 
            {
                _cardioAnimator.SetInteger("state", 2);
                _cardioText.text = "Attention";
                _cardioAnimator.SetFloat("speed", 1.2f);
            }
            else if (playerHealth >= 66f) //Etat bien
            {
                _cardioAnimator.SetInteger("state", 3);
                _cardioText.text = "Stable";
                _cardioAnimator.SetFloat("speed", 1.5f);
            }

            //On appelle ici le script qui va actualiser les effets de la vie
            HealthEffects.Instance.UpdateHealthEffects();

            Statistics.Instance.getPlayerStat("vie")._hasChanged = false;
        }
    }

}
