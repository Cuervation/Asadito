using UnityEngine;
using UnityEngine.EventSystems;

namespace Asadito.Runtime
{
    /// <summary>Receives a single, explicit tap on the visible serving board to serve the order.</summary>
    public sealed class ServingBoardTouch : MonoBehaviour, IPointerClickHandler
    {
        public Asadito.AsaditoGame Owner;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && Owner != null)
                Owner.OnServingBoardTap();
        }
    }
}
