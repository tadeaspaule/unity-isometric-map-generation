using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyObject : CharacterObject
{
    public Friendly friendly;
    
    // Start is called before the first frame update
    void Start()
    {
        BaseStart();
    }

    public void FriendlyClicked()
    {
        gameManager.PartyCharacterClicked(this.gameObject);
    }

    protected override void UpdateHealth()
    {
        // Uncomment this when you assign a Friendly object to this GameObject
        // healthbar.fillAmount = ((float)friendly.health) / friendly.maxHealth;
    }
}
