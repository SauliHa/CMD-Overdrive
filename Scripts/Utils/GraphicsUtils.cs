using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
    public class GraphicsUtils
    {
        public const float DARK_ALPHA = 0.05f;
        public const float MEDIUM_DARK_ALPHA = 0.25f;
        public const float LIGHT_DARK_ALPHA = 0.50f;
        public const float HIGHLIGHTED_ALPHA = 1f;
        public static void Darken(SpriteRenderer spriteRenderer, bool includeChildren = true)
        {
            if (includeChildren)
            {
                foreach (SpriteRenderer renderer in spriteRenderer.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.color = ChangeAlpha(renderer.color, DARK_ALPHA);
                }
            }
            else
            {
                spriteRenderer.color = ChangeAlpha(spriteRenderer.color, DARK_ALPHA);
            }
        }
        public static void Lighten(SpriteRenderer spriteRenderer, bool includeChildren = true)
        {
            if (includeChildren)
            {
                foreach (SpriteRenderer renderer in spriteRenderer.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.color = ChangeAlpha(renderer.color, HIGHLIGHTED_ALPHA);
                }
            }
            else
            {
                spriteRenderer.color = ChangeAlpha(spriteRenderer.color, HIGHLIGHTED_ALPHA);
            }
        }
        public static Color Darken(Color color)
        {
            return ChangeAlpha(color, DARK_ALPHA);
        }
        public static Color Lighten(Color color)
        {
            return ChangeAlpha(color, HIGHLIGHTED_ALPHA);
        }
        public static Color ChangeAlpha(Color color, float a)
        {
            Color tempColor = color;
            tempColor.a = a;
            return tempColor;
        }
        public static void ChangeAlpha(SpriteRenderer spriteRenderer, float a, bool includeChildren = true)
        {
            if (includeChildren)
            {
                foreach (SpriteRenderer renderer in spriteRenderer.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.color = ChangeAlpha(renderer.color, a);
                }
            }
            else
            {
                spriteRenderer.color = ChangeAlpha(spriteRenderer.color, a);
            }
        }

        public static IEnumerator ForceScrollDown(ScrollRect rect)
        {
            // Wait for end of frame AND force update all canvases before setting to bottom.
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            rect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }
}
