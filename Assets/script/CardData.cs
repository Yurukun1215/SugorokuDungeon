using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Card
{
    //表示するだけのプロパティ
    public string name;
    public string description;

    //内部的なプロパティ
    public int ID;
    public string effect;
    public float effectValue;
    public string command;
}

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public List<Card> HoldCardList;
}