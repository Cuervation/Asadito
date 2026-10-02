using UnityEngine;

namespace Asadito
{
    /// <summary>Responds only to Canvas dimension changes; keeps fixed-size mobile UI inside its safe width.</summary>
    public sealed class ManagementWidthFit : MonoBehaviour
    {
        RectTransform bounds, rect;
        float desiredWidth, margin, anchorX, fraction;
        float lastWidth=float.NaN;
        public void Configure(RectTransform bounds,float width,float margin=32,float anchorX=-1,float fraction=1)
        {
            this.bounds=bounds; desiredWidth=width; this.margin=margin; this.anchorX=anchorX; this.fraction=fraction;
            rect=(RectTransform)transform; lastWidth=float.NaN; Apply();
        }
        void OnRectTransformDimensionsChange() => Apply();
        void OnEnable() { Canvas.willRenderCanvases += Apply; Apply(); }
        void OnDisable() { Canvas.willRenderCanvases -= Apply; }
        void Apply()
        {
            if(bounds==null||rect==null) return;
            if(bounds.rect.width==lastWidth) return;
            lastWidth=bounds.rect.width;
            float available=bounds.rect.width*fraction-margin;
            if(anchorX>=0) available=bounds.rect.width*2*Mathf.Min(anchorX,1-anchorX)-margin;
            float width=Mathf.Max(1,Mathf.Min(desiredWidth,available));
            if(Mathf.Abs(rect.rect.width-width)>.01f) rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);
        }
    }
}
