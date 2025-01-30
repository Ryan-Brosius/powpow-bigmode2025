using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowWordUIManager : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject bulletsParent;
    [SerializeField] public List<DropZone> dropZones;
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

        if (dropZones.Last().isLoaded)
        {
            CreateBlankBullet();
        }
        foreach (DropZone zone in dropZones)
        {
            if (!zone.isLoaded && zone != dropZones.Last())
            {
                dropZones.Remove(zone);
                Destroy(zone.gameObject);
                return;
            }
        }
    }

    string GetCurrentPowString()
    {
        //return currentPow = string.Join("", dropZones.Select(zone => zone.currentLetter ? zone.currentLetter.GetComponent<DraggableLetter>().LetterValue.ToString() : ""));

        return currentPow = string.Join("", dropZones.Select(zone => zone.isLoaded ? zone.currentCaseString : ""));
    }

    void CreateBlankBullet()
    {
        GameObject newBullet = Instantiate(bulletPrefab, bulletsParent.transform);
        
        dropZones.Add(newBullet.GetComponent<DropZone>());
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
