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

    public TilemapManager tilemapManager;

    const int blockSize = 10;
    const int mapSize = 5; // how many blocks is one side
    const int blockGap = 2;

    void GenerateMapLayout()
    {
        int[,] map = new int[mapSize,mapSize];
        int counter = 1;
        int visitIndex = 0;
        while (visitIndex < mapSize*mapSize) {
            int block = visitIndex;
            visitIndex++;
            int x = block % mapSize;
            int y = block / mapSize;
            if (map[y,x] > 0) continue;
            List<string> possibleShapes = new List<string>();
            possibleShapes.Add("single");
            possibleShapes.Add("single");
            bool canGoDown = y+1 < mapSize && map[y+1,x] == 0;
            bool canGoRight = x+1 < mapSize && map[y,x+1] == 0;
            if (canGoDown) possibleShapes.Add("corridordown");
            if (canGoDown) possibleShapes.Add("corridordown");
            if (canGoRight) possibleShapes.Add("corridorright");
            if (canGoRight) possibleShapes.Add("corridorright");
            if (canGoDown && canGoRight) {
                possibleShapes.Add("chamber");
                possibleShapes.Add("chamber");
                possibleShapes.Add("chamber");
                // maybe make this more likely than corridors (add multiple times)
                // since this can have more variety?
            }
            string shape = possibleShapes[Random.Range(0,possibleShapes.Count)];
            switch (shape) {
                case "single":
                    map[y,x] = counter;
                    break;
                case "corridordown":
                    int height = Random.Range(2, (int)(mapSize*0.8));
                    for (int y2 = y; y2+1 < mapSize && y2 < y + height; y2++) {
                        if (map[y2,x] > 0) break;
                        map[y2,x] = counter;
                    }
                    break;
                case "corridorright":
                    int width = Random.Range(2, (int)(mapSize*0.8));
                    for (int x2 = x; x2+1 < mapSize && x2 < x + width; x2++) {
                        if (map[y,x2] > 0) break;
                        map[y,x2] = counter;
                    }
                    break;
                case "chamber":
                    int sizex = Random.Range(2, (int)(mapSize*0.8));
                    int sizey = Random.Range(2, (int)(mapSize*0.8));
                    Debug.Log($"Making chamber, original size {sizex} {sizey}");
                    // cutting down size so it fits within the map constraints
                    while (x + sizex > mapSize) sizex--;
                    while (y + sizey > mapSize) sizey--;
                    for (int s = 1; s < sizex; s++) {
                        if (map[y,x+s] > 0) {
                            sizex = s - 1;
                            break;
                        }
                    }
                    Debug.Log($"Making chamber, pruned size {sizex} {sizey}");
                    for (int y2 = y; y2 < y+sizey; y2++) {
                        for (int x2 = x; x2 < x+sizex; x2++) {
                            map[y2,x2] = counter;
                        }
                    }
                    break;
            }
            counter++;
        }
        // for (int y = 0; y < mapSize; y++) {
        //     string row = "";
        //     for (int x = 0; x < mapSize; x++) {
        //         row += map[y,x] + " ";
        //     }
        //     Debug.Log(row);
        // }
        tilemapManager.SetupMap(map,blockSize,blockGap,mapSize);
    }

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

        GenerateMapLayout();
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
