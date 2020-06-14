using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopController : AController
{
    [Header ("SHOP")]
    public GameObject shop;
    public float shopAnimationSpeed;
    public float shopMinimumHeight;
    public float shopMaximumHeight;
    [Header("Habilities")]
    public Hability[] habilities;
    [Header("Defenses")]
    public Hability[] defenses;
    [Header("HabilitySlots")]
    public RawImage[] habilitySlots;
    [Header("DefenseSlots")]
    public RawImage[] defenseSlots;

    private int maxMinesPerRound;
    private int maxTTurretsPerRound;
    private int maxFTurretsPerRound;

    // Start is called before the first frame update
    public void StartGame()
    {
        GameEvents.instance.onPreparationFinish += OnPreparationFinish;
        GameEvents.instance.onRoundFinish += OnRoundFinish;

        Hability mine = Array.Find(defenses, defense => defense.name == "Mine");
        maxMinesPerRound = mine.maxCharges;
        Hability tTurret = Array.Find(defenses, defense => defense.name == "TerrainTurret");
        maxTTurretsPerRound = tTurret.maxCharges;
        Hability aTurret = Array.Find(defenses, defense => defense.name == "AirTurret");
        maxFTurretsPerRound = aTurret.maxCharges;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetIconToHabilitySlot(string name)
    {        
        Hability h = Array.Find(habilities, hability => hability.name == name);
        if (h == null)
        {
            Debug.LogWarning(name + " not found.");
            return;
        }
        if (h.cost > gc.player.cash) return;
        foreach (RawImage slot in habilitySlots)
        {
            foreach (RawImage slot2 in habilitySlots)
            {
                SlotInfo sInfo2 = slot2.gameObject.GetComponent<SlotInfo>();
                if (sInfo2.content == h.name)
                {
                    if (sInfo2.charges < h.maxCharges)
                    {
                        if (sInfo2.charges + h.charges <= h.maxCharges) sInfo2.ChangeCharges(h.charges);
                        else sInfo2.ChangeCharges(h.maxCharges - h.charges);
                        AudioManager.instance.PlayOneShotSound("Buy", gc.player.transform.position);
                        gc.player.cash -= h.cost;
                        gc.uiController.ChangeCash(gc.player.cash);
                        return;
                    }
                    else return;
                }
            }
            if (!slot.gameObject.activeSelf || slot.gameObject.GetComponent<SlotInfo>().content == h.name)
            {
                SlotInfo sInfo = slot.gameObject.GetComponent<SlotInfo>();
                slot.texture = h.icon;
                slot.gameObject.SetActive(true);
                sInfo.content = h.name;
                if (sInfo.charges < h.maxCharges)
                {
                    if (sInfo.charges + h.charges <= h.maxCharges) sInfo.ChangeCharges(h.charges);
                    else sInfo.ChangeCharges(h.maxCharges - h.charges);
                    AudioManager.instance.PlayOneShotSound("Buy", gc.player.transform.position);
                    gc.player.cash -= h.cost;
                    gc.uiController.ChangeCash(gc.player.cash);
                }
                return;
            }
        }
    }

    public void SetIconToDefenseSlot(string name)
    {
        Hability d = Array.Find(defenses, defense => defense.name == name);
        if (d == null)
        {
            Debug.LogWarning(name + " not found.");
            return;
        }
        if (d.cost > gc.player.cash) return;
        switch (name)
        {
            case "Mine":
                if (gc.player.actualMineDefenses >= maxMinesPerRound) return;
                break;
            case "TerrainTurret":
                if (gc.player.actualTTurretDefenses >= maxTTurretsPerRound) return;
                break;
            case "AirTurret":
                if (gc.player.actualFTurretDefenses >= maxFTurretsPerRound) return;
                break;
        }
        foreach (RawImage slot in defenseSlots)
        {
            if (!slot.gameObject.activeSelf || slot.gameObject.GetComponent<SlotInfo>().content == d.name)
            {
                SlotInfo sInfo = slot.gameObject.GetComponent<SlotInfo>();
                slot.texture = d.icon;
                slot.gameObject.SetActive(true);
                sInfo.content = d.name;
                if (sInfo.charges < d.maxCharges)
                {
                    if (sInfo.charges + d.charges <= d.maxCharges) sInfo.ChangeCharges(d.charges);
                    else sInfo.ChangeCharges(d.maxCharges - d.charges);
                    switch(name)
                    {
                        case "Mine":
                            gc.player.actualMineDefenses += d.charges;
                            break;
                        case "TerrainTurret":
                            gc.player.actualTTurretDefenses += d.charges;
                            break;
                        case "AirTurret":
                            gc.player.actualFTurretDefenses += d.charges;
                            break;
                    }
                    AudioManager.instance.PlayOneShotSound("Buy", gc.player.transform.position);
                    gc.player.cash -= d.cost;
                    gc.uiController.ChangeCash(gc.player.cash);
                }                
                return;
            }
        }
    }

    void OnPreparationFinish()
    {
        StartCoroutine(MoveShop(true));
    }

    void OnRoundFinish()
    {
        StartCoroutine(MoveShop(false));
    }

    public IEnumerator MoveShop (bool hide)
    {
        if (hide)
        {
            shop.transform.GetChild(0).gameObject.GetComponent<CapsuleCollider>().enabled = false;
            if (gc.player.atShop)
            {
                gc.player.atShop = false;
                gc.uiController.HideInteractiveText();
                if (gc.uiController.shopInterface.activeSelf) gc.player.Shop(false);
            }
            while (shop.transform.position.y > shopMinimumHeight)
            {
                shop.transform.Translate(-Vector3.forward * shopAnimationSpeed * Time.deltaTime);
                yield return 0;
            }
            shop.transform.position = new Vector3(shop.transform.position.x, shopMinimumHeight, shop.transform.position.z);                       
        }
        else
        {
            while (shop.transform.position.y < shopMaximumHeight)
            {
                shop.transform.Translate(Vector3.forward * shopAnimationSpeed * Time.deltaTime);
                yield return 0;
            }
            shop.transform.position = new Vector3(shop.transform.position.x, shopMaximumHeight, shop.transform.position.z);
            shop.transform.GetChild(0).gameObject.GetComponent<CapsuleCollider>().enabled = true;
        }
    }

    public int ReturnCost(string name)
    {
        Hability h = Array.Find(habilities, hability => hability.name == name);
        if (h == null)
        {
            h = Array.Find(defenses, defense => defense.name == name);
            if (h == null)
            {
                Debug.LogWarning(name + " not found.");
                return 0;
            }                
        }
        return h.cost;
    }

    public void ResetHabilitiesAndDefenses()
    {
        foreach (RawImage hSlot in habilitySlots)
        {
            SlotInfo sInfo = hSlot.GetComponent<SlotInfo>();
            sInfo.Reset();
        }
        foreach (RawImage dSlot in defenseSlots)
        {
            SlotInfo sInfo = dSlot.GetComponent<SlotInfo>();
            sInfo.Reset();
        }
    }
}
