using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowWordUIManager : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] GameObject bulletPrefab;
    public GameObject bulletsParent;
    [SerializeField] public List<DropZone> dropZones;
    private string currentPow;
    public GameObject gunMenu;
    public Transform hudPosition;
    public Transform menuPosition;
    private bool menuOpen = false;

    [Header("Letter Objects")]
    [SerializeField] GameObject letterP;
    [SerializeField] GameObject letterO;
    [SerializeField] GameObject letterW;

    [Header("Bullet Stuff")]
    [SerializeField] int currentLength;
    [SerializeField] int maxLength = 15;
    [SerializeField] string initialString = "POW";

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

        currentPow = initialString;
        foreach (char letter in initialString)
        {
            LetterPickup(letter);
        }
    }

    private void Start()
    {
        currentPow = GetCurrentPowString();
        PowerGameState.Instance.PowString = currentPow;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleMenuScreen();
        }
    }



    public void ToggleMenuScreen()
    {
        if (!menuOpen)
        {
            gunMenu.SetActive(true);
            menuOpen = true;
            Time.timeScale = 0;
            bulletsParent.transform.position = menuPosition.position;
        }
        else
        {
            gunMenu.SetActive(false);
            menuOpen = false;
            Time.timeScale = 1;
            bulletsParent.transform.position = hudPosition.position;
        }
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

    public void LetterPickup(char letterToAdd)
    {
        if (currentLength >= maxLength) return;

        if (letterToAdd == 'P')
        {
            dropZones[0].AddLetter(Instantiate(letterP, dropZones[0].transform));
        } 
        else if (letterToAdd == 'O')
        {
            dropZones[0].AddLetter(Instantiate(letterO, dropZones[0].transform));
        }
        else if (letterToAdd == 'W')
        {
            dropZones[0].AddLetter(Instantiate(letterW, dropZones[0].transform));
        }
        currentLength++;
        OnChange();
    }
}
