using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour {

    private GameSettings gameSettings = null;
    private string gameDataProjectFilePath = "/gameSettings.json";

    public List<LightweightPipelineAsset> quality = new List<LightweightPipelineAsset>();

    public TMP_Dropdown resolutionDrop = null;
    Resolution[] resolutionList = null;

    public TMP_Dropdown qualityDrop = null;
    public TMP_Dropdown texturesDrop = null;

    public Toggle fullScreenToggle = null;

    void Start()
    {
        LoadGameSettings();

        initSettings();
    }



    void LoadGameSettings()
    {
        string filePath = Application.dataPath + gameDataProjectFilePath;

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            gameSettings = JsonUtility.FromJson<GameSettings>(dataAsJson);
            //Debug.Log("GameSettings Loaded");
        }
        else
        {
            gameSettings = new GameSettings();
            //Debug.Log("GameSettings by default");
        }
    }

    void SaveGameSettings()
    {
        string dataAsJson = JsonUtility.ToJson(gameSettings);

        string filePath = Application.dataPath + gameDataProjectFilePath;
        File.WriteAllText(filePath, dataAsJson);
        //Debug.Log("GameSettings Saved");
    }

    void initResolutions()
    {
        resolutionList = Screen.resolutions;

        List<string> m_DropOptions = new List<string>();

        for(int i = 0; i < resolutionList.Length; i++)
        {
            m_DropOptions.Add(resolutionList[i].ToString());
        }

        resolutionDrop.AddOptions(m_DropOptions);
        resolutionDrop.value = gameSettings.resolutionIndex;
        resolutionDrop.RefreshShownValue();

        Screen.SetResolution(resolutionList[gameSettings.resolutionIndex].width, resolutionList[gameSettings.resolutionIndex].height, gameSettings.fullScreen);
    }

    void initSettings()
    {
        initResolutions();

        qualityDrop.value = gameSettings.lightWeightQuality;
        qualityDrop.RefreshShownValue();
        GraphicsSettings.renderPipelineAsset = quality[gameSettings.lightWeightQuality];

        texturesDrop.value = gameSettings.texturesQuality;
        texturesDrop.RefreshShownValue();
        QualitySettings.masterTextureLimit = gameSettings.texturesQuality;

        fullScreenToggle.isOn = gameSettings.fullScreen;

    }


    public void ApplyGraphicsAndSave()
    {
        gameSettings.fullScreen = fullScreenToggle.isOn;

        gameSettings.resolutionIndex = resolutionDrop.value;
        Screen.SetResolution(resolutionList[gameSettings.resolutionIndex].width, resolutionList[gameSettings.resolutionIndex].height, gameSettings.fullScreen);

        gameSettings.lightWeightQuality = qualityDrop.value;
        GraphicsSettings.renderPipelineAsset = quality[gameSettings.lightWeightQuality];

        gameSettings.texturesQuality = texturesDrop.value;
        QualitySettings.masterTextureLimit = gameSettings.texturesQuality;

        Hudv2.Instance.changeOptions(false);

        SaveGameSettings();
    }

}
