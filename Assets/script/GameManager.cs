using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class StageData
{
    public int cellValue;
    public int[] cardIds;
}

public class GameManager : MonoBehaviour
{
    [Header("マスのGameObjectの親")]
    [SerializeField] private Transform cells;

    [Header("プレイヤー")]
    [SerializeField] private Transform player;

    [Header("ゴールが何マス目か")]
    [SerializeField] private int goal;

    [Header("ゴールフラグ")]
    [SerializeField] private bool goalFlug;

    [Header("ゴール演出")]
    [SerializeField] private GameObject goalText;

    // 一応獲得が無いマスのIDを0としているがそんなマス実装しない可能性もある
    [Header("マス毎に獲得できるカードのID")]
    [SerializeField] private int[] cardIds;

    [Header("CardData(ScriptableObject)")]
    [SerializeField] CardData cardData;

    private float CELL_DISTANCE = 3;        //マス同士の距離
    private float MOVE_SPEED = 20;          //マスを進むスピード
    private float JUMP_HEIGHT = 2;          //プレイヤーのジャンプの高さ
    private float PLAYER_POSITION_Y = -1;   //プレイヤーのデフォルトのY座標

    private int dice;
    private int playerStayCell = 0;

    public void DropDice()
    {
        dice = Dice();
        Debug.Log("Dice == " + dice);
        StartCoroutine(PlayerMoveCoroutine(dice));
    }

    private void CardSet()
    {
        //for (int i = 0; i < items.Count + 2; i++)
        //{
        //    if (itemIcons.Count <= i)
        //        itemIcons.Add(Instantiate(itemIcons[0], containtsParent));

        //    itemIcons[lastIndex].sprite = items[i].pattern;
        //    //itemIcons[lastIndex].color = items[i].color;
        //    itemIcons[lastIndex].gameObject.name = i.ToString();
        //}
    }

    private int Dice()
    {
        return UnityEngine.Random.Range(1, 7);
    }

    private IEnumerator PlayerMoveCoroutine(int moveCell)
    {
        if (playerStayCell + moveCell >= goal)
        {
            moveCell = goal - playerStayCell;
            goalFlug = true;
        }
        Vector2 pos = cells.position;
        Vector2 Ppos = player.position;
        float moveDistance = 0;
        for (int i = 0; i < moveCell; i++)
        {
            while (moveDistance < CELL_DISTANCE)
            {
                pos.x -= MOVE_SPEED * Time.deltaTime;
                moveDistance += MOVE_SPEED * Time.deltaTime;
                cells.position = pos;

                float t = moveDistance / CELL_DISTANCE;

                Ppos.y = PLAYER_POSITION_Y + Mathf.Sin(t * Mathf.PI) * JUMP_HEIGHT;
                player.position = Ppos;

                yield return null;
            }
            playerStayCell ++;
            pos.x = -playerStayCell * CELL_DISTANCE;
            cells.position = pos;
            Ppos.y = PLAYER_POSITION_Y;
            player.position = Ppos;
            moveDistance = 0;
            yield return new WaitForSeconds(0.5f);
        }
        Card card = cardData.GetCard(cardIds[playerStayCell]);
        cardData.HoldCardList.Add(card);
        if (goalFlug)
        {
            goalText.SetActive(true);
        }
    }

    public void GoBattle()
    {
        SceneManager.LoadScene("BattleScene");
    }
}