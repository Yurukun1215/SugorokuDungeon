using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    [Header("SugorokuScene")]
    [SerializeField] private int diceCount; //サイコロを振れる回数
    [SerializeField] private List<int> cardIds; //[index]マス目で獲得できるカードのID

    public int DiceCount
    {
        get { return diceCount; }
    }

    public List<int> CardIds
    {
        get { return cardIds; }
    }

    [Header("BattleScene")]
    [SerializeField] private BattleUnit enemy;

    public BattleUnit Enemy
    {
        get { return enemy; }
    }
}
