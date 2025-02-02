using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class PowWordUIManager : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] GameObject bulletPrefab;
    public GameObject bulletsParent;
    [SerializeField] public List<DropZone> dropZones;
    [SerializeField] TextMeshProUGUI specialsText;
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
    }

    private void Start()
    {
        currentPow = GetCurrentPowString();
        if (PowerGameState.Instance)
        PowerGameState.Instance.PowString = currentPow;

        currentPow = initialString;
        foreach (char letter in initialString)
        {
            LetterPickup(letter);
        }
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

        if (specialsText)
        {
            CheckSpecials();
        }

        if (dropZones.Last().isLoaded && dropZones.Count < 5)
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

    public void CheckSpecials()
    {
        if (currentPow == "POWPOWPOWPOWPOW")
        {
            StartCoroutine(SpecialsTextAnimation("MACHINE GUN"));
        }
        if (currentPow.Contains("PPPPPPPPPP"))
        {
            StartCoroutine(SpecialsTextAnimation("RAILGUN"));
        }
        if (currentPow.Contains("POOWW"))
        {
            foreach (DropZone zone in dropZones)
            {
                if (zone.currentCaseString == "POOWW") StartCoroutine(SpecialsTextAnimation("DOUBLE BARREL"));
            }
        }
        if (currentPow.Contains("PPOOW"))
        {
            foreach (DropZone zone in dropZones)
            {
                if (zone.currentCaseString == "PPOOW") StartCoroutine(SpecialsTextAnimation("SONIC SHOT"));
            }
        }
        if (currentPow.Contains("OOOOOOOOOO"))
        {
            StartCoroutine(SpecialsTextAnimation("QUANTUM CANNON"));
        }
        if (currentPow.Contains("WWWWWWWWWW"))
        {
            StartCoroutine(SpecialsTextAnimation("ANNIHILATOR"));
        }
    }

    IEnumerator SpecialsTextAnimation(string special)
    {
        specialsText.transform.localScale = Vector3.one;
        specialsText.text = special;
        specialsText.gameObject.SetActive(true);

        specialsText.transform.DOScale(Vector3.one, 0.25f)  // Animate scale to its original size
            .OnStart(() =>
            {
                // Shake the object immediately when it starts appearing
                specialsText.transform.DOShakeScale(0.3f, 1f);
            })
            .OnComplete(() =>
            {
                // After appearing, wait for the disappear delay before starting the disappearing shake
                DOVirtual.DelayedCall(2.5f, () =>
                {
                    // Shake and disappear (animate the scale to 0 to make it disappear)
                    specialsText.transform.DOShakeScale(0.25f, 1f)
                        .OnComplete(() =>
                        {
                            // Set scale to 0 after shaking to make the object disappear
                            specialsText.transform.localScale = Vector3.zero;
                        });
                });
            });

        yield return new WaitForSeconds(3f);

        specialsText.gameObject.SetActive(false);
    }
}
