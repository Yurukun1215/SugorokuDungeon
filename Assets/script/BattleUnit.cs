using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [Header("プロパティ")]
    [SerializeField] private float hp;
    [SerializeField] private float maxHp;
    [SerializeField] private float attack;
    [SerializeField] private float protect;

    public float GetAttack => attack;

    //[Header("UI")]
    //[SerializeField] private TextMeshProUGUI hpText;
    //[SerializeField] private Slider hpSlider;
    public void OnDamage(float damege)
    {
        hp -= damege * (protect / 100);
        //hpSlider.value = hp / maxHp;
    }
}
