using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectManager : MonoBehaviour
{
    [Header("ステージ選択タブ関連")]
    [SerializeField] private GameObject stage;
    [SerializeField] private Transform stageParent;
    [SerializeField] private GameObject stageElementPrefab;
    [SerializeField] private List<GameObject> stageElements;

    [Header("StageData(ScriptableObject)")]
    [SerializeField] private StageData stageData;

    private GameObject nowOpenTab = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StageSet();
        TabOpen(stage);
    }

    // Update is called once per frame
    void Update()
    {
        
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
        SceneManager.LoadScene("SugorokuScene");
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

    public void ReturnTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}