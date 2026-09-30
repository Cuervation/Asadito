using UnityEngine;
using UnityEngine.EventSystems;

namespace Asadito.Runtime
{
    /// <summary>
    /// Pointer target for a grill piece. A single pointer-down selection path is shared by taps and drags;
    /// the invisible target can be larger than the rendered food without changing its silhouette.
    /// </summary>
    public sealed class FoodPieceTouch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter
    {
        private const int NoPointer = int.MinValue;

        public AsaditoGame Owner;
        public int PortionIndex;

        private int pointerId = NoPointer;
        private bool dragging;
        private bool selectedBeforePointerDown;

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            return Owner == null || Owner.IsFoodTargetClosest(PortionIndex, screenPoint, eventCamera);
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (Owner == null || pointerId != NoPointer) return;
            selectedBeforePointerDown = Owner.IsFoodPieceSelected(PortionIndex);
            if (!Owner.BeginFoodPointer(PortionIndex, e.pointerId)) return;
            pointerId = e.pointerId;
            dragging = false;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (Owner == null || pointerId != e.pointerId || dragging) return;
            bool shouldFlip = selectedBeforePointerDown;
            ReleasePointer();
            if (shouldFlip && Owner != null) Owner.FlipSelectedPortionFromTap(PortionIndex);
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (!OwnsPointer(e.pointerId)) return;
            dragging = true;
            Owner.BeginFoodDrag(PortionIndex);
        }

        public void OnDrag(PointerEventData e)
        {
            if (OwnsPointer(e.pointerId) && dragging)
                Owner.DragFood(PortionIndex, e.pointerId, e.position);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!OwnsPointer(e.pointerId) || !dragging) return;
            Owner.EndFoodDrag(PortionIndex, e.pointerId, e.position);
            ReleasePointer();
        }

        private bool OwnsPointer(int id)
        {
            return Owner != null && pointerId == id && Owner.IsActiveFoodPointer(PortionIndex, id);
        }

        private void ReleasePointer()
        {
            if (Owner != null) Owner.EndFoodPointer(PortionIndex, pointerId);
            pointerId = NoPointer;
            dragging = false;
            selectedBeforePointerDown = false;
        }
    }
}
