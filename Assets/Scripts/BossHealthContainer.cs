using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class BossHealthContainer : MonoBehaviour
{
    public GameObject prefab;

    List<Enemy> bosses;
    List<Transform> bossObjects;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetupBosses(List<Enemy> bosses)
    {
        this.bosses = bosses;
        bossObjects = new List<Transform>();
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        foreach (Enemy boss in bosses) {
            GameObject go = Instantiate(prefab,Vector2.zero,Quaternion.identity,transform);
            go.transform.GetChild(0).GetComponent<Image>().fillAmount = boss.health / boss.maxHealth;
            go.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = boss.charName;
            bossObjects.Add(go.transform);
        }
    }

    void Update()
    {
        for (int i = 0; i < bosses.Count; i++) {
            bossObjects[i].GetChild(0).GetComponent<Image>().fillAmount = bosses[i].health / bosses[i].maxHealth;
        }
    }
}
