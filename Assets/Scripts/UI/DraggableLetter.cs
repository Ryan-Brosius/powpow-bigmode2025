using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    public DropZone originalDropZone;
    [SerializeField] char letterValue;
    [SerializeField] LetterFloat letterFloatScript;

    public char LetterValue
    {
        get { return letterValue; }
        private set {}
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (letterFloatScript) letterFloatScript.makeFloat = false;
        originalPosition = rectTransform.position;
        /*
        originalPosition = rectTransform.position;
        originalDropZone = PowWordUIManager.Instance.GetDropZone(gameObject);
        if (originalDropZone != null)
        {
            originalDropZone.removeLetter(this.gameObject);

            this.transform.SetParent(GameObject.Find("Canvas").transform);
        }

        foreach (DropZone drop in PowWordUIManager.Instance.dropZones)
        {
            drop.removeLetter(gameObject);
        }
        */

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (originalDropZone == null)
        {
            if (letterFloatScript)
            {
                letterFloatScript.startPosition = rectTransform.position;
                letterFloatScript.makeFloat = true;
            }
        }
            
        if (originalDropZone != null)
        {
            originalDropZone.ManualOnDrop(gameObject);
            if (originalPosition != null) transform.position = originalPosition;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (PowWordUIManager.Instance) PowWordUIManager.Instance.OnChange();
    }

    /*
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
    */
}
