using System;
using System.Collections.Generic;
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
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    [Header("ステージリスト")]
    [SerializeField] private List<Stage> stageList;

    [Header("選択されたステージ")]
    [SerializeField] private Stage selectStage;

    [Header("プレイヤー設定")]
    [SerializeField] private string playerName;
    [SerializeField] private float bgmVolume;
    [SerializeField] private float seVolume;

    public List<Stage> StageList
    {
        get { return stageList; }
    }

    public Stage SelectStage
    {
        get { return selectStage; }
    }

    public string PlayerName
    {
        get { return playerName; }
        set { playerName = value; }
    }

    public float BgmVolume
    {
        get { return bgmVolume; }
        set { bgmVolume = value; }
    }

    public float SeVolume
    {
        get { return seVolume; }
        set { seVolume = value; }
    }

    public void SetVolumeDefault()
    {
        bgmVolume = 0.8f;
        seVolume = 0.8f;
    }

    public void SetStage(int index)
    {
        selectStage = stageList[index];
    }
}
