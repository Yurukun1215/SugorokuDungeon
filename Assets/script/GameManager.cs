using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//[Serializable]
//public class StageData
//{
//    public int cellValue;
//    public int[] cardIds;
//}

public class GameManager : MonoBehaviour
{
    [Header("マス関連")]
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private List<GameObject> cells;

    [Header("プレイヤー")]
    [SerializeField] private Transform player;

    [Header("ゴールが何マス目か")]
    [SerializeField] private int goal;

    [Header("ゴールフラグ")]
    [SerializeField] private bool goalFlug;

    [Header("ゴール演出")]
    [SerializeField] private GameObject goalText;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI diceCountText;

    [Header("サイコロ関連")]
    [SerializeField] private Image diceImage;
    [SerializeField] private Sprite[] diceSprites;

    [Header("マップ関連")]
    [SerializeField] private GameObject mapBack;
    [SerializeField] private Transform mapParent;
    [SerializeField] private GameObject mapElementPrefab;
    [SerializeField] private List<GameObject> mapElements;
    [SerializeField] private Color playerStayColor;

    // 一応獲得が無いマスのIDを0としているがそんなマス実装しない可能性もある
    [Header("マス毎に獲得できるカードのID")]
    [SerializeField] private List<int> cardIds;

    [Header("CardData(ScriptableObject)")]
    [SerializeField] CardData cardData;

    [Header("StageData(ScriptableObject)")]
    [SerializeField] StageData stageData;


    private float CELL_DISTANCE = 3;        //マス同士の距離
    private float MOVE_SPEED = 20;          //マスを進むスピード
    private float JUMP_HEIGHT = 2;          //プレイヤーのジャンプの高さ
    private float PLAYER_POSITION_Y = -1;   //プレイヤーのデフォルトのY座標

    private int dice;
    private int diceCount;
    private bool canDropDice = true;
    private bool isStopDice = true;
    private int playerStayCell = 0;

    private void Start()
    {
        cardIds.Clear();
        cardIds = stageData.CardIds;
        goal = cardIds.Count - 1;
        diceCount = stageData.DiceCount;
        DiceCountSet();
        CellSet();
    }

    private void CellSet()
    {
        foreach (GameObject obj in cells)
        {
            Destroy(obj);
        }
        cells.Clear();
        for (int i = 0; i < cardIds.Count; i++)
        {
            GameObject obj = Instantiate(cellPrefab, cellParent);
            cells.Add(obj);
            Vector3 pos;
            pos.x = i * CELL_DISTANCE;
            pos.y = 0;
            pos.z = 0;
            obj.transform.localPosition = pos;
        }
    }

    public void DropDice()
    {
        if (isStopDice)
        {
            if (canDropDice && diceCount > 0)
            {
                diceCount--;
                DiceCountSet();
                isStopDice = false;
                dice = Dice();
                StartCoroutine(DiceValueAnim());
            }
            else
            {
                Debug.Log("<color=red>今はサイコロを振れません");
            }
        }
        else
        {
            isStopDice = true;
        }
    }

    private void DiceCountSet()
    {
        diceCountText.text = $"HoldDice = {diceCount}";
    }

    public void OpenMap()
    {
        for (int i = 0; i < cardIds.Count; i++)
        {
            GameObject obj = Instantiate(mapElementPrefab, mapParent);
            mapElements.Add(obj);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = i.ToString();
            if (i == playerStayCell)
                obj.GetComponent<Image>().color = playerStayColor;
        }
        mapBack.SetActive(true);
    }
    public void CloseMap()
    {
        foreach (GameObject obj in mapElements)
        {
            Destroy(obj);
        }
        mapElements.Clear();
        mapBack.SetActive(false);
    }

    private int Dice()
    {
        return Random.Range(1, 7);
    }

    private IEnumerator DiceValueAnim(float delayValue = 0.05f)
    {
        WaitForSeconds delay = new WaitForSeconds(delayValue);
        canDropDice = false;

        while (!isStopDice)
        {
            for (int i = 0; i < 6; i++)
            {
                diceImage.sprite = diceSprites[i];
                if (isStopDice) break;
                yield return delay;
            }
        }
        diceImage.sprite = diceSprites[dice - 1];
        yield return new WaitForSeconds(delayValue * 10);
        StartCoroutine(PlayerMoveCoroutine(dice));
    }

    private IEnumerator PlayerMoveCoroutine(int moveCell)
    {
        if (playerStayCell + moveCell >= goal)
        {
            moveCell = goal - playerStayCell;
            goalFlug = true;
        }
        Vector2 pos = cellParent.position;
        Vector2 Ppos = player.position;
        float moveDistance = 0;
        for (int i = 0; i < moveCell; i++)
        {
            while (moveDistance < CELL_DISTANCE)
            {
                pos.x -= MOVE_SPEED * Time.deltaTime;
                moveDistance += MOVE_SPEED * Time.deltaTime;
                cellParent.position = pos;

                float t = moveDistance / CELL_DISTANCE;

                Ppos.y = PLAYER_POSITION_Y + Mathf.Sin(t * Mathf.PI) * JUMP_HEIGHT;
                player.position = Ppos;

                yield return null;
            }
            playerStayCell ++;
            pos.x = -playerStayCell * CELL_DISTANCE;
            cellParent.position = pos;
            Ppos.y = PLAYER_POSITION_Y;
            player.position = Ppos;
            moveDistance = 0;
            yield return new WaitForSeconds(0.5f);
        }
        cardData.AddCard(cardIds[playerStayCell]);
        if (goalFlug)
        {
            goalText.SetActive(true);
        }
        else
        {
            canDropDice = true;
        }
    }

    public void GoBattle()
    {
        SceneManager.LoadScene("BattleScene");
    }
}