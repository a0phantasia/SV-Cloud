using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class IDraggable : IMonoBehaviour, IPointerDownHandler, IBeginDragHandler, 
    IEndDragHandler, IDragHandler
{
    protected Canvas canvas;
    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;

    [SerializeField] public UnityEvent<RectTransform> onBeginDragEvent = new UnityEvent<RectTransform>();
    [SerializeField] public UnityEvent<RectTransform> onDragEvent = new UnityEvent<RectTransform>();
    [SerializeField] public UnityEvent<RectTransform> onEndDragEvent = new UnityEvent<RectTransform>();

    protected override void Awake() {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }

    private void OnDestroy() {
        onBeginDragEvent?.RemoveAllListeners();
        onDragEvent?.RemoveAllListeners();
        onEndDragEvent?.RemoveAllListeners();
    }

    public virtual void OnPointerDown(PointerEventData eventData) {
        
    }

    public virtual void OnBeginDrag(PointerEventData eventData) {
        canvasGroup.blocksRaycasts = false;
        onBeginDragEvent?.Invoke(rectTransform);
    }

    public virtual void OnDrag(PointerEventData eventData) {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        onDragEvent?.Invoke(rectTransform);
    }

    public virtual void OnEndDrag(PointerEventData eventData) {
        canvasGroup.blocksRaycasts = true;
        onEndDragEvent?.Invoke(rectTransform);
    }

    public void SetEnable(bool enable) {
        this.enabled = enable;
    }
}
