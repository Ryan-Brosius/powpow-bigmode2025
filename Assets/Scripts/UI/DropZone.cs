using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public GameObject currentLetter = null;
    public string currentCaseString = null;
    public bool isLoaded = false;

    [SerializeField] int cellSize = 16;
    [SerializeField] int verticalPadding = 2;
    [SerializeField] int horizontalPadding = 10;
    public List<GameObject> lettersList;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableLetter draggedLetter = eventData.pointerDrag.GetComponent<DraggableLetter>();
        if (draggedLetter != null)
        {
            /*
             * For single letter cells
            if (currentLetter == null)
            {
                currentLetter = draggedLetter.gameObject;
                currentLetter.transform.position = transform.position;
                draggedLetter.originalDropZone = this;
            }
            */
            if (draggedLetter.originalDropZone) draggedLetter.originalDropZone.removeLetter(draggedLetter.gameObject);
            AddLetter(draggedLetter.gameObject);
        }
    }

    public void ManualOnDrop(GameObject letter)
    {
        // For single letter cells
        // currentLetter = letter;
        // currentLetter.transform.position = transform.position;

        //AddLetter(letter);
        //letter.transform.SetParent(this.transform);

        UpdateChildOrder(lettersList);
        ResizeCaseUI();
    }

    private void AddLetter(GameObject letterToAdd)
    {
        lettersList.Add(letterToAdd);
        letterToAdd.transform.SetParent(this.transform);
        lettersList = SortLetterList(lettersList);
        letterToAdd.GetComponent<DraggableLetter>().originalDropZone = this;
        UpdateChildOrder(lettersList);
        UpdateCaseString();
        ResizeCaseUI();
    }

    public void removeLetter(GameObject letter)
    {
        // For single letter cells
        // if (currentLetter == letter) currentLetter = null;

        if (lettersList.Contains(letter))
        {
            lettersList.Remove(letter);
        }
        lettersList = SortLetterList(lettersList);
        UpdateCaseString();
        ResizeCaseUI();
    }

    private void ResizeCaseUI()
    {
        if (lettersList.Count == 0) this.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize + horizontalPadding, cellSize + verticalPadding);
        else this.GetComponent<RectTransform>().sizeDelta = new Vector2(((cellSize * lettersList.Count) + horizontalPadding), cellSize + verticalPadding);
    }

    private void UpdateChildOrder(List<GameObject> desiredOrder)
    {
        if (transform.childCount != desiredOrder.Count) return;
        for (int i = 0; i < desiredOrder.Count; i++)
        {
            desiredOrder[i].transform.SetSiblingIndex(i);
        }
    }

    List<GameObject> SortLetterList (List<GameObject> letters)
    {
        letters.Sort((a, b) =>
        {
            // Get the letter from the letter values
            string letterA = a.GetComponent<DraggableLetter>().LetterValue.ToString();
            string letterB = b.GetComponent<DraggableLetter>().LetterValue.ToString();

            // Get priority values based on the letter
            int aPriority = GetLetterPriority(letterA);
            int bPriority = GetLetterPriority(letterB);

            return aPriority.CompareTo(bPriority);
        });

        return letters;
    }

    void UpdateCaseString()
    {
        currentCaseString = null;
        foreach(GameObject letter in lettersList)
        {
            currentCaseString += letter.GetComponent<DraggableLetter>().LetterValue.ToString();
        }
        if (currentCaseString != null) isLoaded = true;
        else isLoaded = false;
    }

    int GetLetterPriority(string letterValue)
    {
        switch (letterValue)
        {
            case "P": return 0;
            case "O": return 1;
            case "W": return 2;
            default: return 3;
        }
    }
}
