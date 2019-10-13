using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<FriendlyObject> party;
    public List<EnemyObject> enemies;

    #region Generation

    public TilemapManager tilemapManager;

    #endregion

    
    // Start is called before the first frame update
    void Start()
    {
        tilemapManager.GenerateMapLayout();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Click events

    public void PartyPanelClicked(int index)
    {
        Debug.Log($"Party panel clicked {index}");
        party[index].friendly.health -= 5;
    }

    public void PartyCharacterClicked(GameObject partyCharacter)
    {
        Debug.Log($"Clicking on party character with name {partyCharacter.name}");
        int index = int.Parse(partyCharacter.name.Substring(6));

    }

    public void EnemyCharacterClicked(GameObject enemy)
    {
        Debug.Log($"Clicking on enemy character with name {enemy.name}");
        int index = int.Parse(enemy.name.Substring(6));
    }

    #endregion
}
