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
    public float defense;
    public bool isAlive;

    public Slider slider;

    public void Damage(int value)
    {
        hp -= value;
        slider.value = hp / maxHp;
        Debug.Log($"{name}に{value}のダメージ");
        if (hp <= 0)
            isAlive = false;
    }
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

    private bool canSelectCard;
    private bool isSuccessCommand;
    private Card useCard;
    private float defenseConstant = 250;
    private bool endBattle = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(BattleLoop());
    }
    IEnumerator BattleLoop()
    {
        while (!endBattle)
        {
            yield return StartCoroutine(BattleLoopCoroutine());
        }
            
    }

    private IEnumerator BattleLoopCoroutine()
    {
        BattleLog("あなたのターンです");
        if (cardData.HoldCardList.Count != 0)
        {
            canSelectCard = true;
            while (useCard == null)
            {
                yield return null;
            }
            canSelectCard = false;
            yield return CommandCoroutine(useCard.command, useCard.commandLimit);
            if (isSuccessCommand)
                ReflectEffect();
            commandLimitText.text = null;
            commandText.text = null;
            useCard = null;
        }
        BattleLog("あなたの攻撃");
        enemy.Damage(CalculateDamage(player, enemy));
        if (!enemy.isAlive)
        {
            EndBattle(true);
            yield break;
        }
        BattleLog("敵の攻撃");
        player.Damage(CalculateDamage(enemy, player)); 
        if (!player.isAlive)
        {
            EndBattle(false);
            yield break;
        }
        yield return new WaitForSeconds(2.0f);
    }

    private void EndBattle(bool win)
    {
        endBattle = true;
        if (win)
            Debug.Log("<color=red>あなたの勝ち");
        else
            Debug.Log("<color=red>あなたの負け");
    }

    private int CalculateDamage(BattleUnit attacker, BattleUnit defender)
    {
        float damage = attacker.attack * (defenseConstant / (defenseConstant + defender.defense));
        return (int)damage;
    }

    private void ReflectEffect()
    {
        switch (useCard.effect)
        {
            case Effect.Attack:
                player.attack += useCard.effectValue;
                break;
            case Effect.Defense:
                enemy.defense += useCard.effectValue;
                break;
        }
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
                TextPartMarkup(commandText, command, keyIndex, Color.red);
                keyIndex++;
                if (keyIndex < maxKeyIndex)
                {
                    key = GetKeyControlFromChar(command[keyIndex]);
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
        if (canSelectCard)
        {
            useCard = cardData.GetCard(id);
            cardData.RemoveCard(id);
        }
    }
}