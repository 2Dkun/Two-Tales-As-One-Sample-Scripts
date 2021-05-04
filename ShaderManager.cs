using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows other scripts to easily adjust the shaders of a character rig
public class ShaderManager : MonoBehaviour {

    private SpriteRenderer[] renders;
    private Material[] originalMats;

    private DungeonManager dungeon;
    private Material m1, m2, m3;

    private Timer timer;

    void Awake() {
        renders = GetComponentsInChildren<SpriteRenderer>();
        originalMats = new Material[renders.Length];
        for (int i = 0; i < renders.Length; i++)
            originalMats[i] = renders[i].material;

        dungeon = FindObjectOfType<DungeonManager>();
        m1 = dungeon.materials.hitlagShader;
        m2 = dungeon.materials.spiritShader;
        m3 = dungeon.materials.blinkShader;
    }

    public void FlashWhite() {
        StartCoroutine(FlashWhite(dungeon.constants.hurtTime*Time.deltaTime));
    }

    public void InvincibleShader() { ChangeMaterial(m3); }
    public void SpiritShader() { ChangeMaterial(m2); }

    private IEnumerator FlashWhite(float waitTime) {
        ChangeMaterial(m1);
        yield return new WaitForSeconds(waitTime);
        RevertMaterial();
    }

    private void ChangeMaterial(Material m)
    {
        if (m == null) return;
        for (int i = 0; i < renders.Length; i++)
            renders[i].material = m;
    }

    public void RevertMaterial() {
        for (int i = 0; i < renders.Length; i++)
            renders[i].material = originalMats[i];
    }

}
