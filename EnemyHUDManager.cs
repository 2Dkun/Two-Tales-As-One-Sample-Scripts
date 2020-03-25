using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHUDManager : MonoBehaviour {

    public SpriteRenderer[] components;
    public Transform hpBar, shadow;
    public Vector3 full, empty;
    public float height = 4;

    private Transform foePos;
    private bool shown;

    // Text stuff
    public TextMeshProUGUI enemyName;
    public TextMeshProUGUI hpText;

    // Start is called before the first frame update
    void Start() {
        // Hide the bar until foe gets hit
        SetAlpha(0);
    }

    // Update is called once per frame
    void Update() {
        transform.localPosition = foePos.localPosition;
        transform.localPosition += new Vector3(0, height, 0);
       // if(hpText) hpText.UpdatePosition(foePos.position + textShift);

        // Update shadow
        Vector3 shadowPos = shadow.localPosition;
        Vector3 originalPos = hpBar.localPosition;
        if (shadowPos != originalPos) {
            // Gradually move shadow bar to original
            shadow.localPosition =
                Vector3.MoveTowards(shadowPos, originalPos, 1.2f * Time.deltaTime);
        }

        // Reveal HP bar if haven't
        float barAlpha = components[0].color.a;
        if (shown && barAlpha < 1) {
            SetAlpha(barAlpha+Time.deltaTime);
        }
    }

    public void UpdateHPBar(float curHP, float maxHP) {
        // Update text
        hpText.text = curHP.ToString();
        // Show updates bars if hidden
        float percent = curHP / maxHP;
        if (!shown && percent > 0) shown = true;
        // Get position of new Vector3 based on given percent
        Vector3 newPos = (full - empty) * percent;
        newPos += empty;
        hpBar.transform.localPosition = newPos;
    }

    public void SetPosition(Transform t) {
        foePos = t;
    }

    public void SetAlpha(float alpha) {
        if (alpha > 1) alpha = 1;
        if (alpha < 0) alpha = 0;
        for (int i = 0; i < components.Length; i++) {
            Color newAlph = components[i].color;
            newAlph.a = alpha;
            components[i].color = newAlph;
        }
        enemyName.color = new Color(1, 1, 1, alpha);
        hpText.color = new Color(1, 1, 1, alpha);
    }

    public void ShowBar() { SetAlpha(1); }

    public void InitText(string name, float hp) {
        enemyName.text = name;
        hpText.text = hp.ToString();
    }
}
