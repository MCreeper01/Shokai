using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SlotsController : MonoBehaviour
{
    [Header("Habilities")]
    public Hability[] habilities;
    [Header("Defenses")]
    public Hability[] defenses;
    [Header("HabilitySlots")]
    public RawImage[] habilitySlots;
    [Header("DefenseSlots")]
    public RawImage[] defenseSlots;

    // Start is called before the first frame update
    void Start()
    {
        
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
        foreach (RawImage slot in habilitySlots)
        {
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
                }
                return;
            }
        }
    }
}
