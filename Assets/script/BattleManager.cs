using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[Serializable]
public class BattleUnit
{
    public string name;
    public float hp;
    public float maxHp;
    public float attack;
    public float protect;

    public Slider slider;
}

public class BattleManager : MonoBehaviour
{
    [Header("キャラクターデータ")]
    [SerializeField] private BattleUnit player;
    [SerializeField] private BattleUnit enemy;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI commandLimitText;
    [SerializeField] private TextMeshProUGUI commandText;

    [Header("スクリプト参照インスタンス")]
    [SerializeField] private CardData cardData;

    private bool isPlayerTurn;

    private bool isSuccessCommand;

    private Card useCard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(BattleLoopCoroutine());
    }

    private IEnumerator BattleLoopCoroutine()
    {
        BattleLog("YourTurn");
        //持っているカードを表示する
        while (useCard == null)
        {
            yield return null;
        }
        yield return CommandCoroutine(useCard.command, useCard.commandLimit);
        Debug.Log("Success");
    }


    private IEnumerator CommandCoroutine(string command, float limit)
    {
        float timer = limit;
        int count = (int)limit;
        commandLimitText.text = count.ToString();

        int keyIndex = 0;
        int maxKeyIndex = command.Length;
        commandText.text = command;
        KeyControl key = GetKeyControlFromChar(command[keyIndex]);
        while (timer >= 0)
        {
            //コマンドを受け付ける処理
            if (key.wasPressedThisFrame)
            {
                if (keyIndex != maxKeyIndex)
                {
                    key = GetKeyControlFromChar(command[keyIndex]);
                    TextPartMarkup(commandText, command, keyIndex, Color.red);
                    keyIndex++;
                }
                else
                {
                    isSuccessCommand = true;
                    yield break;
                }
            }

            if (count != Math.Ceiling(timer)) 
            {
                count = (int)Math.Ceiling(timer);
                commandLimitText.text = count.ToString();
            }
            timer -= Time.deltaTime;
            yield return null;
        }
        commandLimitText.text = "TimeUp";
        isSuccessCommand = false;
    }

    private void TextPartMarkup(TextMeshProUGUI tmp, string text, int index, Color color)
    {
        string colorCode = ColorUtility.ToHtmlStringRGB(color);
        string markupText = $"<color=#{colorCode}>";
        
        for (int i = 0; i < text.Length; i++)
        {
            markupText += text[i];
            if (i == index)
                markupText += "</color>";
        }
        tmp.text = markupText;
    }

    KeyControl GetKeyControlFromChar(char c)
    {
        if (Keyboard.current == null) return null;

        string controlName = c.ToString().ToLowerInvariant();
        return Keyboard.current.TryGetChildControl<KeyControl>(controlName);
    }

    private void BattleLog(string log)
    {
        Debug.Log(log);
    }

    public void SelectCard(int id)
    {
        useCard = cardData.GetCard(id);
    }
}