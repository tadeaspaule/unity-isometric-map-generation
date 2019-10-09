using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CharacterObject : MonoBehaviour
{
    protected GameManager gameManager;
    
    public SpriteRenderer symbolHolder;
    public Image healthbar;
    
    // Start is called before the first frame update
    protected void BaseStart()
    {
        gameManager = FindObjectOfType<GameManager>();
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

    protected abstract void UpdateHealth();

    void Update()
    {
        UpdateHealth();
    }
}
