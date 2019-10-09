using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PartyPanel : MonoBehaviour
{
    GameManager gameManager;
    public GameObject prefab;
    public List<FriendlyObject> party;
    List<MemberPanel> panels = new List<MemberPanel>();
    
    private class MemberPanel
    {
        public Image healthbarFill;
        public Transform cooldownHolder;
        public static GameObject cooldownPrefab;

        public static MemberPanel Make(GameObject prefab, Transform parent, Friendly friendly, int index, GameManager gameManager)
        {
            MemberPanel mp = new MemberPanel();
            GameObject go = Instantiate(prefab, Vector2.zero,Quaternion.identity,parent);
            go.name = index.ToString();
            go.GetComponent<Button>().onClick.AddListener(delegate { gameManager.PartyPanelClicked(index); });
            // this code has to change if you modify the prefab structure / order
            mp.healthbarFill = go.transform.GetChild(0).GetComponent<Image>();
            mp.healthbarFill.fillAmount = friendly.health / friendly.maxHealth;
            // TODO change class icon depending on class
            Image classIcon = go.transform.GetChild(1).GetComponent<Image>();
            go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = friendly.charName;
            mp.cooldownHolder = go.transform.GetChild(3).transform;
            return mp;
        }

        public void UpdateView(Friendly friendly)
        {
            // here update health and cooldowns
            healthbarFill.fillAmount = ((float)friendly.health) / friendly.maxHealth;
            // TODO update cooldowns
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void Setup(List<FriendlyObject> party)
    {
        this.party = party;
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject); // wipe all children first just in case
        }
        panels.Clear();
        for (int i = 0; i < party.Count; i++) {
            panels.Add(MemberPanel.Make(prefab,transform,party[i].friendly,i,gameManager));
        }
    }

    void Update()
    {
        for (int i = 0; i < party.Count; i++) {
            panels[i].UpdateView(party[i].friendly);
        }
    }
}
