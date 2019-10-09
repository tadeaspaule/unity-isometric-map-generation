using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<FriendlyObject> party;
    public List<EnemyObject> enemies;

    public PartyPanel partyPanel;

    #region Symbols
    
    public SymbolHolder symbolHolder;

    List<string> symbols = new List<string>(new string[]{"donut","square"});
    string selectedSymbol = null;
    Dictionary<string,int> assignedSymbols = new Dictionary<string, int>();

    public void SymbolClicked(GameObject symbol)
    {
        int index = int.Parse(symbol.name);
        selectedSymbol = symbols[index];
        symbolHolder.HighlightSymbol(index);
    }

    #endregion

    #region Generation

    

    #endregion

    
    // Start is called before the first frame update
    void Start()
    {
        symbolHolder.SetupSymbols(symbols);
        foreach (string s in symbols) {
            assignedSymbols.Add(s,-1);
        }
        // Set up dummy Friendly and Enemy objects for now
        foreach (FriendlyObject fo in party) {
            Friendly f = new Friendly();
            f.health = 30;
            f.maxHealth = 40;
            f.charName = fo.name;
            fo.friendly = f;
        }
        foreach (EnemyObject eo in enemies) {
            Enemy e = new Enemy();
            e.health = 30;
            e.maxHealth = 40;
            e.charName = eo.name;
            eo.enemy = e;
        }
        partyPanel.Setup(party);   
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
        symbolHolder.ClearHighlights();
        
        if (selectedSymbol != null) {
            // put symbol on enemy
            if (assignedSymbols[selectedSymbol] >= 0) {
                // reassigning symbol to a different enemy
                enemies[assignedSymbols[selectedSymbol]].ClearSymbol();
            }
            // check if that enemy had a different symbol already
            foreach (string key in assignedSymbols.Keys) {
                if (assignedSymbols[key] == index) {
                    // enemy has a symbol
                    if (assignedSymbols[key].Equals(selectedSymbol)) return; // assigning the same symbol
                    else {
                        enemies[index].ClearSymbol(); // clearing a different symbol
                        assignedSymbols[key] = -1;
                        break;
                    }
                }
            }
            enemies[index].SetSymbol(selectedSymbol);
            assignedSymbols[selectedSymbol] = index;
        }
        else {
            // clicked enemy without a symbol, maybe some other interaction?
        }
    }

    #endregion
}
