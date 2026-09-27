using UnityEngine;
using UnityEngine.EventSystems;

namespace Asadito.Runtime
{
    /// <summary>Touch target for moving an uncooked or cooking portion around the grill.</summary>
    public sealed class FoodPieceTouch : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public AsaditoGame Owner;
        public int PortionIndex;

        public void OnBeginDrag(PointerEventData e)
        {
            if (Owner != null) Owner.BeginFoodDrag(PortionIndex);
        }

        public void OnDrag(PointerEventData e)
        {
            if (Owner != null) Owner.DragFood(PortionIndex, e.position);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (Owner != null) Owner.EndFoodDrag(PortionIndex, e.position);
        }
    }
}
