using UnityEngine;
using UnityEngine.EventSystems;

namespace Asadito.Runtime
{
    /// <summary>Touch target for raking embers between heat cells.</summary>
    public sealed class EmberCellTouch : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public AsaditoGame Owner;
        public int X;
        public int Y;
        private int startX;
        private int startY;

        public void OnPointerDown(PointerEventData e) { startX = X; startY = Y; }
        public void OnDrag(PointerEventData e)
        {
            if (Owner != null) Owner.DragEmbers(startX, startY, e.position);
        }
    }
}
