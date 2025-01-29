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

    private void Start()
    {
        currentPow = GetCurrentPowString();
        PowerGameState.Instance.PowString = currentPow;
    }

    public void OnChange()
    {
        currentPow = GetCurrentPowString();
        PowerGameState.Instance.PowString = currentPow;
    }

    string GetCurrentPowString()
    {
        return currentPow = string.Join("", dropZones.Select(zone => zone.currentLetter ? zone.currentLetter.GetComponent<DraggableLetter>().LetterValue.ToString() : ""));
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
