using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private BattleUnit player;
    private BattleUnit enemy;

    private bool isPlayerTurn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlayerTurn)
        {
            player.OnDamage(enemy.GetAttack);
            isPlayerTurn = true;
        }
    }
}
