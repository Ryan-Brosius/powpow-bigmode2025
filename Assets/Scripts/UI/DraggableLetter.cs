using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    public DropZone originalDropZone;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalDropZone = PowWordUIManager.Instance.GetDropZone(gameObject);

        foreach (DropZone drop in PowWordUIManager.Instance.dropZones)
        {
            drop.removeLetter(gameObject);
        }

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (originalDropZone != null)
        {
            originalDropZone.ManualOnDrop(gameObject);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableLetter other = eventData.pointerDrag.GetComponent<DraggableLetter>();
        var dropZone1 = originalDropZone;
        var dropZone2 = other.originalDropZone;

        originalDropZone.ManualOnDrop(other.gameObject);
        other.originalDropZone.ManualOnDrop(gameObject);
        other.originalDropZone = dropZone1;
        originalDropZone = dropZone2;
    }
}
