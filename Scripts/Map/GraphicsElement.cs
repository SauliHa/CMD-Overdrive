using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using DG.Tweening;

namespace Manticore
{
  [RequireComponent(typeof(SpriteRenderer), typeof(PhotonView))]
  public abstract class GraphicsElement : MonoBehaviourPun
  {
    protected SpriteRenderer spriteRenderer;
    private Tween fadeTween;
    private Tween scaleTween;
    protected virtual void Awake()
    {
      spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Lighten(bool includeChildren = true)
    {
      if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
      GraphicsUtils.Lighten(spriteRenderer, includeChildren);
    }

    public void Darken(bool includeChildren = true)
    {
      if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
      GraphicsUtils.Darken(spriteRenderer, includeChildren);
    }

    public void ChangeAlpha(float a, bool includeChildren = true)
    {
      if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
      GraphicsUtils.ChangeAlpha(spriteRenderer, a, includeChildren);
    }
    public void StartHighlightAnimation()
    {
      if (fadeTween == null || !fadeTween.IsInitialized())
      {
        Color tempColor = spriteRenderer.color;
        tempColor.a = GraphicsUtils.LIGHT_DARK_ALPHA;
        spriteRenderer.color = tempColor;
        fadeTween = spriteRenderer.DOFade(1, 0.7f).SetLoops(-1, LoopType.Yoyo).SetId("highlight");
      }
      else
      {
        fadeTween.Restart();
      }
    }
    public void StartScaleAnimation()
    {
      if (scaleTween == null || !scaleTween.IsInitialized())
      {
        Vector3 currentScale = transform.localScale;
        Vector3 endScale = new Vector3(currentScale.x + 0.1f, currentScale.y + 0.1f, 1);
        scaleTween = transform.DOScale(endScale, 0.7f).SetLoops(-1, LoopType.Yoyo).SetId("scale");
      }
      else
      {
        scaleTween.Restart();
      }
    }
  }
}
