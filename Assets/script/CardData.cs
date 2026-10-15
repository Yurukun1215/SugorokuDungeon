using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum Effect
{
    PlayerAttack,
    EnemyDefense,
    Damage,
    Heal,
    EnemyAttack,
    PlayerDefense
}

[Serializable]
public class Card
{
    //表示するだけのプロパティ
    public string name;
    public string description;

    //内部的なプロパティ
    public int durationTurns;
    public int ID;
    public Effect effect;
    public int effectValue;
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

#if UNITY_EDITOR
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

        if (GUILayout.Button("カードを全種類追加"))
        {
            CardData cardData = (CardData)target;

            for (int i = 1; i <= cardData.AllCardList.Count; i++)
            {
                for (int j = 0; j < cardData.editCardValue; j++)
                    cardData.AddCard(i);
            }

            cardData.editCardValue = 1;
        }

        if (GUILayout.Button("カードの説明文を自動挿入"))
        {
            CardData cardData = (CardData)target;

            foreach (Card card in cardData.AllCardList)
            {
                switch (card.effect)
                {
                    case Effect.PlayerAttack:
                        card.description = $"自身の攻撃力を{card.effectValue}アップ";
                        break;
                    case Effect.EnemyDefense:
                        card.description = $"敵の防御力を{-card.effectValue}ダウン";
                        break;
                    case Effect.Damage:
                        card.description = $"敵に{-card.effectValue}の固定ダメージ";
                        break;
                    case Effect.Heal:
                        card.description = $"自身のHPを{card.effectValue}回復";
                        break;
                    case Effect.EnemyAttack:
                        card.description = $"敵の攻撃力を{-card.effectValue}ダウン";
                        break;
                    case Effect.PlayerDefense:
                        card.description = $"自身の防御力を{card.effectValue}アップ";
                        break;
                }
            }
        }

        if (GUILayout.Button("カードのIDを自動設定"))
        {
            CardData cardData = (CardData)target;

            for (int i = 0; i < cardData.AllCardList.Count; i++)
            {
                cardData.AllCardList[i].ID = i + 1;
            }
        }
    }
}
#endif