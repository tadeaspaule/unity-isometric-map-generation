using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SymbolHolder : MonoBehaviour
{
    GameManager gameManager;

    public GameObject symbolPrefab;

   
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void SetupSymbols(List<string> symbols)
    {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < symbols.Count; i++) {
            GameObject go = Instantiate(symbolPrefab,Vector2.zero,Quaternion.identity,transform);
            go.name = i.ToString();
            go.GetComponent<Button>().onClick.AddListener(delegate { gameManager.SymbolClicked(go); });
            go.GetComponent<Image>().sprite = Resources.Load<Sprite>($"symbols/{symbols[i]}");
        }
        ClearHighlights();
    }

    public void HighlightSymbol(int index)
    {
        ClearHighlights();
        transform.GetChild(index).GetComponent<Image>().color = Color.white;
    }

    public void ClearHighlights()
    {
        foreach (Transform child in transform) {
            child.GetComponent<Image>().color = new Color(1f,1f,1f,0.7f);
        }
    }
}
