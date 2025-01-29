using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public GameObject currentLetter = null;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableLetter draggedLetter = eventData.pointerDrag.GetComponent<DraggableLetter>();
        if (draggedLetter != null)
        {
            if (currentLetter == null)
            {
                currentLetter = draggedLetter.gameObject;
                currentLetter.transform.position = transform.position;
                draggedLetter.originalDropZone = this;
            }
        }
    }

    public void ManualOnDrop(GameObject letter)
    {
        currentLetter = letter;
        currentLetter.transform.position = transform.position;
    }

    public void removeLetter(GameObject letter)
    {
        if (currentLetter == letter) currentLetter = null;
    }

}
