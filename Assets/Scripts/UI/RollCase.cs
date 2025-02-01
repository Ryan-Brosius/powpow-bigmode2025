using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollCase : MonoBehaviour
{
    [SerializeField] GameObject prefabR;
    [SerializeField] int cellSize = 7;
    [SerializeField] int verticalPadding = 4;
    [SerializeField] int horizontalPadding = 9;
    public List<GameObject> lettersList;

    public void InitializeRollUI(int maxCharges)
    {
        for (int i = 0; i < maxCharges; i++)
        {
            lettersList.Add(Instantiate(prefabR, this.transform));
        }
        ResizeCaseUI();
    }

    public void UpdateHealthUI(int damage)
    {
        if (damage == 1)
        {
            Destroy(lettersList[lettersList.Count - 1].gameObject);
            lettersList.Remove(lettersList[lettersList.Count - 1]);
        }

        else if (damage == -1)
        {
            lettersList.Add(Instantiate(prefabR, this.transform));
        }
        ResizeCaseUI();
    }

    public void UpdateMaxRolls(int maxCharges)
    {
        if (lettersList.Count < maxCharges)
        {
            for (int i = 0; i < (maxCharges - lettersList.Count); i++)
            {
                lettersList.Add(Instantiate(prefabR, this.transform));
            }
            ResizeCaseUI();
        }
    }

    public void UpdateCurrentRolls(int currentCharges)
    {
        for (int i = 0; i < lettersList.Count; i++)
        {
            if (i < currentCharges) lettersList[i].GetComponent<Image>().color = Color.white;
            else if (i >= currentCharges) lettersList[i].GetComponent<Image>().color = Color.gray;
        }
    }

    private void ResizeCaseUI()
    {
        if (lettersList.Count == 0) this.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize + horizontalPadding, cellSize + verticalPadding);
        else this.GetComponent<RectTransform>().sizeDelta = new Vector2(((cellSize * lettersList.Count) + horizontalPadding), cellSize + verticalPadding);
    }
}
