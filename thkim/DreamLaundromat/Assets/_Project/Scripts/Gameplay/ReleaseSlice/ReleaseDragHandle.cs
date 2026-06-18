using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Action<ReleaseDragHandle> onBeginDrag;
        private Action<ReleaseDragHandle, PointerEventData> onEndDrag;
        private CanvasGroup canvasGroup;
        private Vector3 originalScale = Vector3.one;

        public ReleaseDragPayload Payload { get; private set; }

        public void Configure(
            ReleaseDragSourceKind sourceKind,
            int slotId,
            Action<ReleaseDragHandle> beginDrag,
            Action<ReleaseDragHandle, PointerEventData> endDrag)
        {
            Payload = new ReleaseDragPayload(sourceKind, slotId);
            onBeginDrag = beginDrag;
            onEndDrag = endDrag;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalScale = transform.localScale;
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0.72f;
            canvasGroup.blocksRaycasts = false;
            transform.localScale = originalScale * 1.035f;
            onBeginDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // V1 keeps cards in layout and uses lift/alpha feedback only. Free
            // ghost movement can be added later without changing drag rules.
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            transform.localScale = originalScale;
            onEndDrag?.Invoke(this, eventData);
        }
    }
}
