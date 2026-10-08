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
        endAttack = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
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

    [Header("スクリプト参照インスタンス")]
    [SerializeField] private CardData cardData;
    [SerializeField] private StageData stageData;


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

    void Start()
    {
        enemy.SetStatus(stageData.SelectStage.enemy);
        player.Setup();
        enemy.Setup();
        StartCoroutine(BattleLoopCoroutine());

        playerEndAttack = new WaitUntil(() => !player.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
        enemyEndAttack = new WaitUntil(() => !enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
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
        EndBattle();
    }

    private IEnumerator PlayerTurn()
    {
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
        StartCoroutine(DamageEffect(220, 0, 30, damage));
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
        StartCoroutine(DamageEffect(-220, 0, 30, damage));
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
        StartCoroutine(DamageEffect(effectPosX, 0, 30, damage));
        //yield return atk.endAttack;
        yield return new WaitUntil(() => !atk.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));

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
    }

    private IEnumerator DamageEffect(float posX, float startY, float endY, int damage)
    {
        TextMeshProUGUI text = damageEffect.GetComponent<TextMeshProUGUI>();
        text.text = damage.ToString();
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
            ReflectEffect();
            commandLimitText.text = "Success";
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
        //Debug.Log(log);//今はDebug.Log("hoge")にしているが後から画面上に表示するようにする
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

    private void EndBattle()
    {
        StartCoroutine(BattleLog($"<color=red>{winner.name}の勝ち", 10.0f));
    }

    private int CalculateDamage(BattleUnit attacker, BattleUnit defender)
    {
        float damage = attacker.attack * (defenseConstant / (defenseConstant + defender.defence));
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
                enemy.defence += useCard.effectValue;
                break;
        }
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
}