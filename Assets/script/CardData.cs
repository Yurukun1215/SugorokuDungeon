using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum Effect
{
    Attack,
    Defense
}

[Serializable]
public class Card
{
    //表示するだけのプロパティ
    public string name;
    public string description;

    //内部的なプロパティ
    public int ID;
    public Effect effect;
    public float effectValue;
    public string command;
    public float commandLimit;
}

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    [Header("CardList")]
    [SerializeField] private List<Card> allCardList;

    [SerializeField] private List<Card> holdCardList;

    [Header("CardEditor")]
    public int editCardId;
    public int editCardValue = 1;

    public List<Card> AllCardList
    {
        get { return allCardList; }
    }

    public List<Card> HoldCardList
    {
        get { return holdCardList; }
    }

    public Card GetCard(int id)
    {
        foreach(Card card in allCardList)
        {
            if (card.ID == id)
                return card;
        }
        return null;
    }

    public void AddCard(int id)
    {
        foreach (Card card in allCardList)
        {
            if (card.ID == id)
            {
                holdCardList.Add(card);
                return;
            }
        }
    }

    public void RemoveCard(int id)
    {
        foreach (Card card in holdCardList)
        {
            if (card.ID == id)
            {
                holdCardList.Remove(card);
                return;
            }
        }
    }
}

[CustomEditor(typeof(CardData))]
[CanEditMultipleObjects]
public class CardDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("カードを追加"))
        {
            CardData cardData = (CardData)target;

            for (int i = 0; i < cardData.editCardValue; i++)
                cardData.AddCard(cardData.editCardId);

            cardData.editCardValue = 1;
            cardData.editCardId = 0;
        }

        if (GUILayout.Button("カードを削除"))
        {
            CardData cardData = (CardData)target;

            for (int i = 0; i < cardData.editCardValue; i++)
                cardData.RemoveCard(cardData.editCardId);

            cardData.editCardValue = 1;
            cardData.editCardId = 0;
        }

        if (GUILayout.Button("カードを全削除"))
        {
            CardData cardData = (CardData)target;

            cardData.HoldCardList.Clear();
        }
    }
}