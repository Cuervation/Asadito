using UnityEngine;
using UnityEngine.EventSystems;

namespace Asadito.Runtime
{
    /// <summary>Receives double-taps only on the visible serving-board graphic.</summary>
    public sealed class ServingBoardTouch : MonoBehaviour, IPointerClickHandler
    {
        public Asadito.AsaditoGame Owner;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.clickCount >= 2 && Owner != null)
                Owner.OnServingBoardDoubleTap();
        }
    }
}
