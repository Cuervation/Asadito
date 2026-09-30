using System.Collections.Generic;
using UnityEngine;

namespace Asadito
{
    internal enum AsaditoUiIcon
    {
        Guest, Locked, Star, Tray, Retry, Next, Flip, Back, Exit, Warning
    }

    /// <summary>Small deterministic vector-like UI marks, rasterized once at runtime.</summary>
    internal static class AsaditoUiIcons
    {
        private const int Resolution = 96;
        private const int SamplesPerAxis = 4;
        private static readonly Dictionary<AsaditoUiIcon, Sprite> Cache = new Dictionary<AsaditoUiIcon, Sprite>();
        private static readonly Vector2[] StarShape = CreateStarShape();
        private static readonly Vector2[] TrayShape = { new Vector2(-.78f, .24f), new Vector2(.78f, .24f), new Vector2(.56f, -.58f), new Vector2(-.56f, -.58f) };
        private static readonly Vector2[] RetryArrow = { new Vector2(.72f, .33f), new Vector2(.26f, .40f), new Vector2(.55f, .77f) };
        private static readonly Vector2[] NextShape = { new Vector2(-.72f, -.58f), new Vector2(-.28f, -.58f), new Vector2(.48f, 0f), new Vector2(-.28f, .58f), new Vector2(-.72f, .58f), new Vector2(.04f, 0f) };
        private static readonly Vector2[] FlipTop = { new Vector2(-.86f, .38f), new Vector2(.38f, .38f), new Vector2(.38f, .62f), new Vector2(.87f, .17f), new Vector2(.38f, -.27f), new Vector2(.38f, -.02f), new Vector2(-.86f, -.02f) };
        private static readonly Vector2[] FlipBottom = { new Vector2(.86f, -.38f), new Vector2(-.38f, -.38f), new Vector2(-.38f, -.62f), new Vector2(-.87f, -.17f), new Vector2(-.38f, .27f), new Vector2(-.38f, .02f), new Vector2(.86f, .02f) };
        private static readonly Vector2[] BackShape = { new Vector2(-.8f, 0f), new Vector2(-.28f, .55f), new Vector2(-.28f, .22f), new Vector2(.78f, .22f), new Vector2(.78f, -.22f), new Vector2(-.28f, -.22f), new Vector2(-.28f, -.55f) };
        private static readonly Vector2[] ExitDoor = { new Vector2(-.7f, -.74f), new Vector2(-.02f, -.74f), new Vector2(-.02f, -.48f), new Vector2(-.46f, -.48f), new Vector2(-.46f, .48f), new Vector2(-.02f, .48f), new Vector2(-.02f, .74f), new Vector2(-.7f, .74f) };
        private static readonly Vector2[] ExitArrow = { new Vector2(-.05f, -.3f), new Vector2(.4f, -.3f), new Vector2(.4f, -.56f), new Vector2(.88f, 0f), new Vector2(.4f, .56f), new Vector2(.4f, .3f), new Vector2(-.05f, .3f) };
        private static readonly Vector2[] WarningTriangle = { new Vector2(0f, .83f), new Vector2(.84f, -.67f), new Vector2(-.84f, -.67f) };
        private static readonly Vector2[] WarningInner = { new Vector2(0f, .52f), new Vector2(.62f, -.4f), new Vector2(-.62f, -.4f) };

        // This project disables domain reload on Play. Clear cached Unity objects on
        // every runtime start because Unity destroys generated sprites on Play exit
        // while this static dictionary otherwise survives and returns stale objects.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => Cache.Clear();

        public static Sprite Get(AsaditoUiIcon icon)
        {
            if (Cache.TryGetValue(icon, out Sprite sprite) && sprite != null && sprite.texture != null) return sprite;
            Cache.Remove(icon);
            var pixels = new Color32[Resolution * Resolution];
            int sampleCount = SamplesPerAxis * SamplesPerAxis;
            for (int py = 0; py < Resolution; py++)
            for (int px = 0; px < Resolution; px++)
            {
                int covered = 0;
                for (int sy = 0; sy < SamplesPerAxis; sy++)
                for (int sx = 0; sx < SamplesPerAxis; sx++)
                {
                    float x = ((px + (sx + .5f) / SamplesPerAxis) / Resolution) * 2f - 1f;
                    float y = ((py + (sy + .5f) / SamplesPerAxis) / Resolution) * 2f - 1f;
                    if (Contains(icon, x, y)) covered++;
                }
                pixels[py * Resolution + px] = new Color32(255, 255, 255, (byte)(covered * 255 / sampleCount));
            }

            var texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false)
            {
                name = "Asadito UI Icon " + icon,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            sprite = Sprite.Create(texture, new Rect(0, 0, Resolution, Resolution), new Vector2(.5f, .5f), 100f);
            sprite.name = "Asadito UI Icon " + icon;
            Cache.Add(icon, sprite);
            return sprite;
        }

        private static bool Contains(AsaditoUiIcon icon, float x, float y)
        {
            switch (icon)
            {
                case AsaditoUiIcon.Guest:
                    return Circle(x, y, 0f, .39f, .19f) || Ellipse(x, y, 0f, -.25f, .39f, .32f);
                case AsaditoUiIcon.Locked:
                    bool body = InRect(x, y, -.38f, -.55f, .38f, .08f);
                    bool shackle = y >= .015f && Mathf.Abs(Mathf.Sqrt(x * x + (y - .08f) * (y - .08f)) - .29f) <= .095f;
                    return (body || shackle) && !(body && Circle(x, y, 0f, -.19f, .055f));
                case AsaditoUiIcon.Star:
                    return InPolygon(x, y, StarShape);
                case AsaditoUiIcon.Tray:
                    return InPolygon(x, y, TrayShape) ||
                           Line(x, y, -.82f, .35f, .82f, .35f, .085f) || Ellipse(x, y, -.34f, .48f, .25f, .13f) || Ellipse(x, y, 0f, .49f, .25f, .13f) || Ellipse(x, y, .34f, .48f, .25f, .13f);
                case AsaditoUiIcon.Retry:
                    float radius = Mathf.Sqrt(x * x + y * y);
                    float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
                    return (Mathf.Abs(radius - .58f) < .13f && angle >= 45f && angle <= 325f) ||
                           InPolygon(x, y, RetryArrow);
                case AsaditoUiIcon.Next:
                    return InPolygon(x, y, NextShape);
                case AsaditoUiIcon.Flip:
                    return InPolygon(x, y, FlipTop) || InPolygon(x, y, FlipBottom);
                case AsaditoUiIcon.Back:
                    return InPolygon(x, y, BackShape);
                case AsaditoUiIcon.Exit:
                    return InPolygon(x, y, ExitDoor) || InPolygon(x, y, ExitArrow);
                case AsaditoUiIcon.Warning:
                    bool triangle = InPolygon(x, y, WarningTriangle);
                    bool innerTriangle = InPolygon(x, y, WarningInner);
                    bool mark = InRect(x, y, -.07f, -.2f, .07f, .34f) || Circle(x, y, 0f, -.43f, .085f);
                    return (triangle && !innerTriangle) || (innerTriangle && mark);
                default:
                    return false;
            }
        }

        private static bool Circle(float x, float y, float cx, float cy, float radius)
        {
            float dx = x - cx, dy = y - cy;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static bool Ellipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            float dx = (x - cx) / rx, dy = (y - cy) / ry;
            return dx * dx + dy * dy <= 1f;
        }

        private static bool InRect(float x, float y, float left, float bottom, float right, float top)
        {
            return x >= left && x <= right && y >= bottom && y <= top;
        }

        private static bool Line(float x, float y, float x1, float y1, float x2, float y2, float width)
        {
            float dx = x2 - x1, dy = y2 - y1;
            float t = Mathf.Clamp01(((x - x1) * dx + (y - y1) * dy) / (dx * dx + dy * dy));
            return Circle(x, y, x1 + dx * t, y1 + dy * t, width * .5f);
        }

        private static Vector2[] CreateStarShape()
        {
            var points = new Vector2[10];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = Mathf.PI * .5f + i * Mathf.PI / 5f;
                float radius = (i & 1) == 0 ? .87f : .39f;
                points[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }
            return points;
        }

        private static bool InPolygon(float x, float y, Vector2[] points)
        {
            bool inside = false;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                Vector2 a = points[i], b = points[j];
                if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            return inside;
        }
    }
}
