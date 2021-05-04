using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public LinkManager link;
    public BasePlayer sword, shield, curPlayer;
    [SerializeField] private bool playAsSword;
    
    protected DungeonManager dungeon;
    
    // TEMPPPP
    public SpriteRenderer hpBar;
    public Sprite srd, sld;
    public Attack heal;
    
    private bool BothCharsIn => sword && shield;
    [HideInInspector] public float dashTime = -100;
    
    private void Start() {
        if (!link) link = FindObjectOfType<LinkManager>();
        if (!sword) sword = FindObjectOfType<Sword>();
        if (!shield) shield = FindObjectOfType<Shield>();
        
        // Set active player on public bool + try to compensate for single character
        curPlayer = playAsSword ? shield : sword;
        if (playAsSword && sword == null) { curPlayer = shield; playAsSword = false;}
        else if (!playAsSword && shield == null) { curPlayer = sword; playAsSword = true;}
        if (playAsSword) hpBar.sprite = srd;

        if(sword) sword.ToggleSpirit(!playAsSword);
        if(shield) shield.ToggleSpirit(playAsSword);
        if (!dungeon) dungeon = FindObjectOfType<DungeonManager>();
        dungeon.UpdateCam(GetActivePlayer());
    }

    public BasePlayer GetActivePlayer() { return playAsSword ? sword : shield; }

    public void UpdatePlayers()
    {
        if (isDead) return;
        if (sword) sword.UpdateEntity();
        if (shield) shield.UpdateEntity();
    }

    public void CreateLink(Vector3 linkPosition, float direction) {
        link.Testy(sword.playerData.baseLink, linkPosition, direction);
    }

    #region Functions to Polish Later

    public IEnumerable<BasePlayer> GetPlayers() 
    {
        BasePlayer[] players = {sword, shield};
        return players;
    }

    private bool isDead;
    public void Die()
    {
        isDead = true;
        dungeon.RespawnPlayer();
    }

    private float curTimer = -1000;
    public void AttemptSwap()
    {
        if (!BothCharsIn) return;
        if (shield.InImmobileState) return;
        if (sword.InImmobileState) return;
        
        // TODO: make an actual conditional later
        if (Time.time - curTimer >= 0.3)
        {
            playAsSword = !playAsSword;
            sword.SwapSpirit();
            shield.SwapSpirit();
            curPlayer = (curPlayer == shield) ? sword : shield;
            curTimer = Time.time;

            if (hpBar.sprite != srd) hpBar.sprite = srd;
            else hpBar.sprite = sld;
        }
    }

    #endregion
}
