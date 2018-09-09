using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Hudv2 : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static Hudv2 Instance = null;

    private void Awake()
    {
        Instance = this;
    }



    //Barres de vie
    private Slider _enduranceBarre = null;

    //Elements divers
    private TextMeshProUGUI _fpsCounter = null;
    [SerializeField] private GameObject _menuParent = null;
    [SerializeField] private GameObject _optionsParent = null;
    private Image _cursorInteraction = null;

    public bool _pauseMode = false;
    private bool _inOptions = false;

    //Variable de couleur pour le pointeur
    public Color _cursorInteractColor;

    private void Start()
    {

        _enduranceBarre = GameObject.Find("enduranceBarre").GetComponent<Slider>();

        _fpsCounter = GameObject.Find("fpsCounter").GetComponent<TextMeshProUGUI>();
        _cursorInteraction = GameObject.Find("pointeur").GetComponent<Image>();

    }

    private void Update()
    {
        BarsUpdate();

        //Calcul et affichage des fps
        if (Time.timeScale != 0)
        {
            float fps = Mathf.Round((1.0f / Time.deltaTime) * 100f) / 100f;
            _fpsCounter.text = fps.ToString() + " fps";
        }

        //Fonction pause
        if (Input.GetKeyDown(KeyCode.Escape))
            pauseChange();

    }

    // --------------------------------------- MENU FONCTIONS ------------ BEGIN
    public void pauseChange()
    {
        if (!_inOptions) // Si le joueur n'est pas dans les options alors ouvre ou ferme le menu pause
        {
            //Ferme l'inventaire si le joueur met en pause et que l'inventaire est ouvert
            if (!_pauseMode)
                if (Tablet.Instance.tabletOn)
                    Tablet.Instance.setTablet();

            float timeScale = _pauseMode ? 1.0f : 0.0f;
            Time.timeScale = timeScale;
            _pauseMode = !_pauseMode;
            _menuParent.SetActive(_pauseMode);
            setMouseCursor(!_pauseMode);
        }
        else // Si le joueur est dans les options alors ça ferme les options pour retourner dans le menu pause
        {
            changeOptions(false);
        }
    }

    public void restartGame()
    {
        if (Time.timeScale == 0) Time.timeScale = 1.0f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void changeOptions(bool open)
    {
        _inOptions = open;
        _optionsParent.SetActive(open);
        _menuParent.SetActive(!open);
    }


    public void quitGame()
    {
        Application.Quit();
    }


    // --------------------------------------- MENU FONCTIONS ------------ END

    void BarsUpdate()
    {
        PlayerStat endurance = Statistics.Instance.getPlayerStat("endurance");
        _enduranceBarre.value = endurance._value / endurance._maxValue;
    }

    public void setMouseCursor(bool active)
    {
        FPSController.Instance._mouseLook.SetCursor(active);   
    }

    public void SetInteractionCursor(bool active)
    {
        if (active)
            _cursorInteraction.color = _cursorInteractColor;
        else
            _cursorInteraction.color = new Color(255, 255, 255, 200);
    }

}
