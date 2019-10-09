using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyObject : MonoBehaviour
{
    GameManager gameManager;
    Enemy enemy;
    
    public SpriteRenderer symbolHolder;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void EnemyClicked()
    {
        gameManager.EnemyCharacterClicked(this.gameObject);
    }

    public void SetSymbol(string symbol)
    {
        symbolHolder.gameObject.SetActive(true);
        symbolHolder.sprite = Resources.Load<Sprite>($"symbols/{symbol}");
    }

    public void ClearSymbol()
    {
        symbolHolder.gameObject.SetActive(false);
    }
}
