using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowWordUIManager : MonoBehaviour
{
    [SerializeField] public DropZone[] dropZones = new DropZone[12];
    private string currentPow;

    public static PowWordUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnChange()
    {
        currentPow = GetCurrentPowString();
    }

    string GetCurrentPowString()
    {
        return currentPow = string.Join("", dropZones.Select(zone => zone.currentLetter ? zone.currentLetter.name : ""));
    }

    public DropZone GetDropZone(GameObject letter)
    {
        foreach (DropZone dropZone in dropZones)
        {
            if (dropZone.currentLetter == letter)
            {
                return dropZone;
            }
        }

        return null;
    }
}
