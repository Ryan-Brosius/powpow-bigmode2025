using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCase : MonoBehaviour
{
    [SerializeField] GameObject prefabE;
    [SerializeField] int cellSize = 16;
    [SerializeField] int verticalPadding = 2;
    [SerializeField] int horizontalPadding = 10;
    public List<GameObject> lettersList;

    public void InitializeHealthUI(int health)
    {
        for (int i = 0; i < health; i++)
        {
            lettersList.Add(Instantiate(prefabE, this.transform));
        }
        ResizeCaseUI();
    }

    public void UpdateHealthUI(int damage)
    {
        if (damage == 1)
        {
            Destroy(lettersList[lettersList.Count -1].gameObject);
            lettersList.Remove(lettersList[lettersList.Count - 1]);
        } 
            
        else if (damage == -1)
        {
            lettersList.Add(Instantiate(prefabE, this.transform));
        } 
        ResizeCaseUI();
    }

    private void ResizeCaseUI()
    {
        if (lettersList.Count == 0) this.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize + horizontalPadding, cellSize + verticalPadding);
        else this.GetComponent<RectTransform>().sizeDelta = new Vector2(((cellSize * lettersList.Count) + horizontalPadding), cellSize + verticalPadding);
    }
}
