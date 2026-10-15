using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public enum AnimKey
{

}

[Serializable]
public class BattleUnit
{
    public string name;
    public float hp;
    public float maxHp;
    public float attack;
    public float defence;
    public bool isAlive;

    public Transform parent;
    public Slider slider;
    public TextMeshProUGUI nameText;

    public Transform transform;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public WaitUntil endAttack;

    public void Setup() 
    {
        hp = maxHp;
        slider.maxValue = maxHp;
        slider.value = hp;
        isAlive = true;
        nameText.text = name;
        //endAttack = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
    }

    public void SetStatus(BattleUnit unit)
    {
        name = unit.name;
        hp = unit.hp;
        maxHp = unit.maxHp;
        attack = unit.attack;
        defence = unit.defence;
    }

    public void Damage(int value)
    {
        hp -= value;
        slider.value = hp;
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
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private GameObject damageEffect;

    [Header("インベントリ関連")]
    [SerializeField] private GameObject inventoryBack;
    [SerializeField] private Transform inventoryParent;
    [SerializeField] private GameObject inventoryElementPrefab;
    [SerializeField] private List<GameObject> inventoryElements;

    [Header("フェードパネル関連")]
    [SerializeField] private Color fadeColor;

    [SerializeField] private Image fadePanel;
    private bool isFadePanel;
    private Color fadePanelColor;
    private int fadeDirection;
    private float changeAlphaValue = 1f;

    [Header("リザルトパネル")]
    [SerializeField] RectTransform result;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI recordText;

    [Header("スクリプト参照インスタンス")]
    [SerializeField] private CardData cardData;
    [SerializeField] private StageData stageData;

    private bool auto;

    //リザルト用変数
    private float masterTimer = 0;
    private int clearTime = 0;
    private int getSkillValue = 0;
    private int maxDamage = 0;

    //カードを選択できる状態か
    private bool canSelectCard;
    //使用中のカードの情報
    private Card useCard;
    //ダメージ計算に使う防御定数
    private float defenseConstant = 250;
    //勝者
    private BattleUnit winner;

    private WaitUntil playerEndAttack;
    private WaitUntil enemyEndAttack;

    private float ATTACK_RANGE = 8.5f;
    private float ATTACK_SPEED = 10f;
    private float DAMAGE_EFFECT_SPEED = 50f;
    private float RESULT_OPEN_SPEED = 1000f;

    void Start()
    {
        getSkillValue = cardData.HoldCardList.Count;
        player.name = stageData.PlayerName;
        enemy.SetStatus(stageData.SelectStage.enemy);
        player.Setup();
        enemy.Setup();


        //playerEndAttack = new WaitUntil(() => !player.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
        //enemyEndAttack = new WaitUntil(() => !enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));

        OnFadePanel(fadeColor, -1);
    }
    void Update()
    {
        masterTimer += Time.deltaTime;
        if (isFadePanel)
        {
            fadePanelColor.a += changeAlphaValue * Time.deltaTime * fadeDirection;
            fadePanel.color = fadePanelColor;
            if (fadeDirection < 0 && fadePanelColor.a <= 0)
            {
                isFadePanel = false;
                fadePanelColor.a = 0;
                fadePanel.color = fadePanelColor;
                fadePanel.raycastTarget = false;
                StartCoroutine(BattleLoopCoroutine());
            }
            else if (fadeDirection > 0 && fadePanelColor.a >= 1)
            {
                SceneManagerEx.LoadScene("SelectScene");
            }
        }
    }

    private void OnFadePanel(Color color, int direction)
    {
        fadePanelColor = color;
        if (direction < 0)
            fadePanelColor.a = 1f;
        else if (direction > 0)
            fadePanelColor.a = 0f;
        fadePanel.color = fadePanelColor;
        fadeDirection = direction;
        fadePanel.raycastTarget = true;
        isFadePanel = true;
    }

    private IEnumerator BattleLoopCoroutine()
    {
        while (true)
        {
            yield return BattleLog("あなたのターンです");

            yield return PlayerTurn();

            if (CheckEndBattle()) break;

            yield return BattleLog("敵のターンです");

            yield return EnemyTurn();

            if (CheckEndBattle()) break;

            yield return new WaitForSeconds(2.0f);
        }
        StartCoroutine(EndBattle());
    }

    private IEnumerator PlayerTurn()
    {
        if (!auto)
            yield return UseCardCoroutine();

        //yield return PlayerAttackCoroutine();
        yield return AttackCoroutine(player, enemy, 1);
    }

    private IEnumerator EnemyTurn()
    {
        //yield return EnemyAttackCoroutine();
        yield return AttackCoroutine(enemy, player, -1);
    }

    private IEnumerator PlayerAttackCoroutine()
    {
        yield return BattleLog($"{player.name}の攻撃");
        Vector3 pos = player.transform.position;
        float start = pos.x;
        float end = pos.x + ATTACK_RANGE;
        player.animator.Play("Run");
        while (pos.x < end)
        {
            pos.x += Time.deltaTime * ATTACK_SPEED;
            player.transform.position = pos;
            yield return null;
        }
        player.animator.Play("Attack");
        int damage = CalculateDamage(player, enemy);
        enemy.Damage(damage);
        enemy.animator.Play("Hurt");
        StartCoroutine(DamageEffect(220, 0, 30, damage, Color.red));
        yield return playerEndAttack;
        
        player.spriteRenderer.flipX = true;
        player.animator.Play("Run");

        while (pos.x > start)
        {
            pos.x -= Time.deltaTime * ATTACK_SPEED;
            player.transform.position = pos;
            yield return null;
        }
        player.animator.Play("Idle");
        player.spriteRenderer.flipX = false;
    }

    private IEnumerator EnemyAttackCoroutine()
    {
        yield return BattleLog($"{enemy.name}の攻撃");
        Vector3 pos = enemy.transform.position;
        float start = pos.x;
        float end = pos.x - ATTACK_RANGE;
        enemy.animator.Play("Walk");
        while (pos.x > end)
        {
            pos.x -= Time.deltaTime * ATTACK_SPEED;
            enemy.transform.position = pos;
            yield return null;
        }
        enemy.animator.Play("Attack");
        int damage = CalculateDamage(enemy, player);
        player.Damage(damage);
        player.animator.Play("Hurt");
        StartCoroutine(DamageEffect(-220, 0, 30, damage, Color.red));
        while (pos.x < start)
        {
            if (enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                yield return null;
                continue;
            }
            pos.x += Time.deltaTime * ATTACK_SPEED;
            enemy.transform.position = pos;
            yield return null;
        }
    }

    private IEnumerator AttackCoroutine(BattleUnit atk, BattleUnit def, int direction)
    {
        yield return BattleLog($"{atk.name}の攻撃");
        Vector3 pos = atk.transform.position;
        float start = pos.x;
        float end = pos.x + (ATTACK_RANGE * direction);
        atk.animator.Play("Run");
        while ((pos.x * direction) < (end * direction))
        {
            pos.x += Time.deltaTime * ATTACK_SPEED * direction;
            atk.transform.position = pos;
            yield return null;
        }
        atk.animator.Play("Attack");
        int damage = CalculateDamage(atk, def);
        def.Damage(damage);
        def.animator.Play("Hurt");
        float effectPosX = def.parent.localPosition.x - (50 * direction);
        StartCoroutine(EffectAnim(enemy.spriteRenderer, Color.red, useCard.effectValue, effectPosX));
        //yield return atk.endAttack;
        yield return new WaitUntil(() => atk.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

        atk.spriteRenderer.flipX = true;
        atk.animator.Play("Run");

        while ((pos.x * direction) > (start * direction))
        {
            pos.x -= Time.deltaTime * ATTACK_SPEED * direction;
            atk.transform.position = pos;
            yield return null;
        }
        atk.animator.Play("Idle");
        atk.spriteRenderer.flipX = false;

        if (atk.name == player.name)
        {
            if (damage > maxDamage)
                maxDamage = damage;
        }
    }

    private IEnumerator DamageEffect(float posX, float startY, float endY, int damage, Color color)
    {
        TextMeshProUGUI text = damageEffect.GetComponent<TextMeshProUGUI>();
        text.text = damage.ToString();
        text.color = color;
        RectTransform rect = damageEffect.GetComponent<RectTransform>();
        Vector3 pos = rect.anchoredPosition;
        pos.x = posX;
        pos.y = startY;
        rect.anchoredPosition = pos;
        while (true)
        {
            pos.y += Time.deltaTime * DAMAGE_EFFECT_SPEED;
            rect.anchoredPosition = pos;
            if (pos.y >= endY)
                break;
            yield return null;
        }
        text.text = null;
    }

    private IEnumerator UseCardCoroutine()
    {
        if (cardData.HoldCardList.Count == 0)
            yield break;

        logText.text = "カードを選択してください";
        canSelectCard = true;
        while (useCard == null)
        {
            if (auto)
                yield break;

            yield return null;
        }
        logText.text = null;
        canSelectCard = false;
        string command = useCard.command;
        float limit = useCard.commandLimit;

        bool isSuccessCommand = false;

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
                    break;
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
        if (isSuccessCommand)
        {
            commandLimitText.text = "Success";
            yield return ReflectEffect();
        }
        else
        {
            commandLimitText.text = "TimeUp";
        }
        yield return new WaitForSeconds(1.0f);
        commandLimitText.text = null;
        commandText.text = null;
        useCard = null;
    }
    private IEnumerator BattleLog(string log, float duration = 1.0f)
    {
        logText.text = log;
        yield return new WaitForSeconds(duration);
        logText.text = null;
    }

    private IEnumerator EndBattle()
    {
        if (winner == player)
        {
            resultText.text = "勝利";
            clearTime = (int)masterTimer;
            recordText.text = $"・クリアタイム　{clearTime / 60} : {clearTime % 60}\r\n・最大ダメージ　{maxDamage}\r\n・獲得スキル数　{getSkillValue}";
            stageData.KeepRecord(clearTime, maxDamage, getSkillValue);
        }
        else
        {
            resultText.text = "敗北";
            recordText.text = "・クリアタイム　ーー\r\n・最大ダメージ　ーー\r\n・獲得スキル数　ーー";
        }
        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer >= 3f || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
                break;
            yield return null;
        }
        Vector3 pos = result.anchoredPosition;
        while (true) { 
            pos.y -= RESULT_OPEN_SPEED * Time.deltaTime;
            result.anchoredPosition = pos;
            if (pos.y < 0)
            {
                pos.y = 0;
                result.anchoredPosition = pos;
            }
            yield return null;
        }
    }

    private IEnumerator ReflectEffect()
    {
        float effectPosX;
        yield return new WaitForSeconds(0.5f);
        switch (useCard.effect)
        {
            case Effect.PlayerAttack:
                player.attack += useCard.effectValue;
                effectPosX = player.parent.localPosition.x + 50;
                yield return EffectAnim(player.spriteRenderer, Color.orange, useCard.effectValue, effectPosX);
                break;
            case Effect.EnemyDefense:
                enemy.defence += useCard.effectValue;
                effectPosX = enemy.parent.localPosition.x - 50;
                yield return EffectAnim(enemy.spriteRenderer, Color.blue, useCard.effectValue, effectPosX);
                break;
            case Effect.Damage:
                enemy.hp += useCard.effectValue;
                effectPosX = enemy.parent.localPosition.x - 50;
                yield return EffectAnim(enemy.spriteRenderer, Color.red, useCard.effectValue, effectPosX);
                break;
            case Effect.Heal:
                player.hp += useCard.effectValue;
                effectPosX = player.parent.localPosition.x + 50;
                yield return EffectAnim(player.spriteRenderer, Color.green, useCard.effectValue, effectPosX);
                break;
        }
    }
    
    private IEnumerator EffectAnim(SpriteRenderer targetRenderer, Color endColor, int value, float effectPosX, float duration = 0.75f)
    {
        StartCoroutine(DamageEffect(effectPosX, 0, 30, value, endColor));
        bool effected = false;
        float timer = 0;
        Color startColor = Color.white;
        float t = 0;
        while (true) 
        {
            timer += Time.deltaTime;

            t = Mathf.Clamp01(timer / duration);

            Color lerpedColor = Color.Lerp(startColor, endColor, t);
            targetRenderer.color = lerpedColor;

            if (lerpedColor == endColor)
            {
                effected = true;
                timer = 0;
                startColor = endColor;
                endColor = Color.white;
            }

            if (effected && lerpedColor == endColor)
            {
                break;
            }
            yield return null;
        }
    }

    //以下関数

    private bool CheckEndBattle()
    {
        if (!enemy.isAlive)
        {
            winner = player; 
            enemy.animator.Play("Death");
            Debug.Log("enemyは死んだ");
            return true;
        }
        if (!player.isAlive)
        {
            winner = enemy;
            player.animator.Play("Death");
            return true;
        }
        return false;
    }

    private int CalculateDamage(BattleUnit attacker, BattleUnit defender)
    {
        float damage = attacker.attack * (defenseConstant / (defenseConstant + defender.defence));
        return (int)damage;
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


    public void OpenInventory()
    {
        foreach (Card card in cardData.HoldCardList)
        {
            GameObject obj = Instantiate(inventoryElementPrefab, inventoryParent);
            obj.GetComponent<Button>().onClick.AddListener(() => SelectCard(obj, card.ID));
            SetChild(obj.transform, card.name, card.description, card.command.Length);
            inventoryElements.Add(obj); 
        }
        inventoryBack.SetActive(true);
    }

    private void SetChild(Transform tf, string name, string description, int commandLength)
    {
        tf.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        tf.GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        tf.GetChild(2).GetComponent<TextMeshProUGUI>().text = "command\n" + new string('*', commandLength);
    }

    public void CloseInventory()
    {
        foreach (GameObject obj in inventoryElements)
        {
            Destroy(obj);
        }
        inventoryElements.Clear();
        inventoryBack.SetActive(false);
    }

    public void SelectCard(GameObject obj, int id)
    {
        if (canSelectCard)
        {
            useCard = cardData.GetCard(id);
            cardData.RemoveCard(id);
            inventoryElements.Remove(obj);
            Destroy(obj);
            CloseInventory();
        }
    }

    public void ReturnGuild()
    {
        OnFadePanel(fadeColor, 1);
    }

    public void AutoPlay(Image image)
    {
        if (auto)
        {
            auto = false;
            image.color = Color.white;
        }
        else 
        {
            auto = true;
            image.color = Color.yellow;
        }
    }
}