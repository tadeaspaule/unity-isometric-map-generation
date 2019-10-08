using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyObject : MonoBehaviour
{
    GameManager gameManager;
    Friendly friendly;

    public string charName;
    public string charClass;
    public int health;
    public int maxHealth;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void FriendlyClicked()
    {
        gameManager.PartyCharacterClicked(int.Parse(this.name));
    }
}
