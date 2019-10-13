using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CharacterObject : MonoBehaviour
{
    protected GameManager gameManager;
    
    public Image healthbar;
    
    // Start is called before the first frame update
    protected void BaseStart()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    protected abstract void UpdateHealth();

    void Update()
    {
        UpdateHealth();
    }
}
