using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Stage
{
    public bool isClear;
    public string name;
    public string explanation;

    public int diceCount;//サイコロを振れる回数
    public List<int> cardIds;//{index}マス目で獲得できるカードのID

    public BattleUnit enemy;

    public int bestTime;
    public int bestDamage;
    public int bestSkillValue;
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    [Header("ステージリスト")]
    [SerializeField] private List<Stage> stageList;

    [Header("選択されたステージ")]
    [SerializeField] private Stage selectStage;
    [SerializeField] private int selectStageIndex;

    [Header("プレイヤー設定")]
    public string PlayerName;
    public float BgmVolume;
    public float SeVolume;

    [Header("エディタープロパティ")]
    public int editIndex;
    public string arrayCode;

    public List<Stage> StageList
    {
        get { return stageList; }
    }

    public Stage SelectStage
    {
        get { return selectStage; }
    }

    public void SetVolumeDefault()
    {
        BgmVolume = 0.8f;
        SeVolume = 0.8f;
    }

    public void SetStage(int index)
    {
        selectStage = stageList[index];
        selectStageIndex = index;
    }

    public void KeepRecord(int clearTime, int maxDamage, int getSkillValue)
    {
        int index = selectStageIndex;

        if (!stageList[index].isClear)
            stageList[index].isClear = true;

        if (stageList[index].bestTime < clearTime)
            stageList[index].bestTime = clearTime;

        if (stageList[index].bestDamage < maxDamage)
            stageList[index].bestDamage = maxDamage;

        if (stageList[index].bestSkillValue < getSkillValue)
            stageList[index].bestSkillValue = getSkillValue;

        selectStage = null;
        selectStageIndex = 0;
    }

    public void resetClear()
    {
        foreach (Stage stage in stageList)
        {
            stage.isClear = false;
            stage.bestTime = 0;
            stage.bestDamage = 0;
            stage.bestSkillValue = 0;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StageData))]
[CanEditMultipleObjects]
public class StageDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("クリア履歴をリセット"))
        {
            StageData stageData = (StageData)target;
            stageData.resetClear();
        }

        //if (GUILayout.Button("ステージのマップデータを一括設定"))
        //{
        //    StageData stageData = (StageData)target;
        //    stageData.StageList[stageData.editIndex].cardIds.Clear();
        //    int[] ids = Array.ConvertAll(stageData.arrayCode.Split(", "), int.Parse);
        //    for (int i = 0; i < ids.Length; i++)
        //    {
        //        stageData.StageList[stageData.editIndex].cardIds.Add(ids[i]);
        //    }
        //}
    }
}
#endif
