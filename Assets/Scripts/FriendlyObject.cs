using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyObject : MonoBehaviour
{
    GameManager gameManager;
    Friendly friendly;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void FriendlyClicked()
    {
        gameManager.PartyCharacterClicked(this.gameObject);
    }
}
