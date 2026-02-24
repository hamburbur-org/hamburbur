using UnityEngine;
using UnityEngine.EventSystems;

namespace hamburbur.Components;

public class DragObject : MonoBehaviour, IDragHandler
{
    private Canvas        canvas;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas        = GetComponentInParent<Canvas>();
    }

    public void OnDrag(PointerEventData eventData) =>
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
}