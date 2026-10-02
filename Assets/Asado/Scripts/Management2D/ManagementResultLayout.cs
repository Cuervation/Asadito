using System.Collections;
using UnityEngine;

namespace Asadito
{
    /// <summary>Fits the authored portrait composition uniformly inside the live safe-area Canvas.</summary>
    public sealed class ManagementResultLayout : MonoBehaviour
    {
        RectTransform bounds, rect;
        CanvasGroup group;
        float reveal=1f;
        public void Configure(RectTransform bounds, float width=960, float height=1660, bool animate=true)
        {
            this.bounds=bounds;rect=(RectTransform)transform;
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(width,height);
            Apply();
            // Persistent intro cards are constructed while inactive; their parent owns the fade.
            if(animate)
            {
                group=gameObject.AddComponent<CanvasGroup>();
                StartCoroutine(Enter());
            }
        }
        void OnEnable()=>Canvas.willRenderCanvases+=Apply;
        void OnDisable()=>Canvas.willRenderCanvases-=Apply;
        void Apply()
        {
            if(bounds==null||rect==null)return;
            float fit=Mathf.Min(1f,(bounds.rect.width-40)/rect.rect.width,(bounds.rect.height-56)/rect.rect.height);
            rect.localScale=Vector3.one*Mathf.Max(.01f,fit)*Mathf.Lerp(.985f,1f,reveal);
        }
        IEnumerator Enter()
        {
            group.alpha=0;group.interactable=false;group.blocksRaycasts=true;float elapsed=0;
            while(elapsed<.20f)
            {
                elapsed+=Time.unscaledDeltaTime;reveal=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.20f));
                group.alpha=reveal;Apply();yield return null;
            }
            reveal=1;group.alpha=1;group.interactable=true;Apply();
        }
    }
}
