using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class SceneManagerEx
{
    public static string previousSceneName { get; private set; }

    public static void LoadScene(string sceneName)
    {
        previousSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }
}

public class SelectManager : MonoBehaviour
{
    [Header("ステージ選択タブ関連")]
    [SerializeField] private GameObject stage;
    [SerializeField] private Transform stageParent;
    [SerializeField] private GameObject stageElementPrefab;
    [SerializeField] private List<GameObject> stageElements;

    [Header("フェードパネル関連")]
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;

    [SerializeField] private Image fadePanel;
    private bool isFadePanel;
    private Color fadePanelColor;
    private int fadeDirection;
    private float changeAlphaValue = 1f;

    [Header("オプションタブ関連")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    [Header("StageData(ScriptableObject)")]
    [SerializeField] private StageData stageData;

    private GameObject nowOpenTab = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StageSet();
        nameInput.text = stageData.PlayerName;
        bgmSlider.value = stageData.BgmVolume;
        seSlider.value = stageData.SeVolume;

        TabOpen(stage);

        if (SceneManagerEx.previousSceneName == "TitleScene")
            OnFadePanel(startColor, -1);
        else
            OnFadePanel(endColor, -1);
    }

    // Update is called once per frame
    void Update()
    {
        if (isFadePanel)
        {
            fadePanelColor.a += changeAlphaValue * Time.deltaTime * fadeDirection;
            fadePanel.color = fadePanelColor;
            if (fadeDirection < 0 && fadePanelColor.a <= 0)
            {
                isFadePanel = false;
                fadePanelColor.a = 0;
                fadePanel.color = fadePanelColor;
                fadePanel.raycastTarget = false;
            }
            else if (fadeDirection > 0 && fadePanelColor.a >= 1)
            {
                SceneManagerEx.LoadScene("SugorokuScene");
            }
        }
    }

    private void OnFadePanel(Color color, int direction)
    {
        fadePanelColor = color;
        if (direction < 0)
            fadePanelColor.a = 1f;
        else if (direction > 0)
            fadePanelColor.a = 0f;
        fadePanel.color = fadePanelColor;
        fadeDirection = direction;
        fadePanel.raycastTarget = true;
        isFadePanel = true;
    }

    private void StageSet()
    {
        List<Stage> stageList = stageData.StageList;

        foreach (GameObject obj in stageElements)
        {
            Destroy(obj);
        }
        stageElements.Clear();

        //foreach (Stage stage in stageList)
        for (int i = 0; i < stageList.Count; i++)
        {
            GameObject obj = Instantiate(stageElementPrefab, stageParent);
            stageElements.Add(obj);
            int index = i;
            obj.GetComponent<Button>().onClick.AddListener(() => StartStage(index));
            SetChild(obj.transform, stageList[i].isClear, stageList[i].name, stageList[i].explanation);
        }
    }

    private void SetChild(Transform tf, bool isClear, string name, string explanation)
    {
        tf.GetChild(0).gameObject.SetActive(isClear);
        tf.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
        tf.GetChild(2).GetComponent<TextMeshProUGUI>().text = explanation;
    }

    public void StartStage(int index)
    {
        stageData.SetStage(index);
        OnFadePanel(endColor, 1);
    }

    public void TabOpen(GameObject tab)
    {
        if (nowOpenTab == tab) 
            return;
        if (nowOpenTab != null)
            nowOpenTab.SetActive(false);

        nowOpenTab = tab;
        tab.SetActive(true);
    }

    public void SetName(TMP_InputField input)
    {
        stageData.PlayerName = input.text;
    }

    public void InputName(TMP_InputField input)
    {
        input.text = stageData.PlayerName;
    }

    public void SetBgmSlider(Slider slider)
    {
        stageData.BgmVolume = slider.value;
    }
    public void SetSeSlider(Slider slider)
    {
        stageData.SeVolume = slider.value;
    }
    public void SetVolumeDefault()
    {
        stageData.SetVolumeDefault();
        bgmSlider.value = 0.8f;
        bgmSlider.value = 0.8f;
    }

    public void EnterURL(TextMeshProUGUI tmp)
    {
        tmp.fontStyle = FontStyles.Underline;
    }

    public void ExitURL(TextMeshProUGUI tmp)
    {
        tmp.fontStyle = FontStyles.Normal;
    }

    public void ClickURL(TextMeshProUGUI tmp)
    {
        Application.OpenURL(tmp.text);
    }

    public void ClickTwitterURL(TextMeshProUGUI tmp)
    {
        Application.OpenURL($"https://x.com/{tmp.text.Substring(1)}");
    }

    public void ReturnTitle()
    {
        SceneManagerEx.LoadScene("TitleScene");
    }
}