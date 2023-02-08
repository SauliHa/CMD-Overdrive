using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace Manticore
{
  public class AnimationTest : MonoBehaviour
  {
    bool isPlaying = true;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
      spriteRenderer = GetComponent<SpriteRenderer>();
      Color color = spriteRenderer.color;
      DOTween.ToAlpha(() =>
      {
        Debug.Log("Getting color, alpha: " + spriteRenderer.color.a);
        return spriteRenderer.color;
      }, x => spriteRenderer.color = x, spriteRenderer.color.a - 0.2f, 1).SetLoops(-1, LoopType.Yoyo).SetId("highlight");
      // spriteRenderer.DOFade(spriteRenderer.color.a - 0.3f, 1).SetLoops(-1, LoopType.Yoyo).SetId("highlight");
      transform.DOScale(new Vector3(1.05f, 1.05f, 1), 1).SetLoops(-1, LoopType.Yoyo).SetId("highlight");
    }

    public void ToggleAnimation()
    {
      if (isPlaying)
      {
        DOTween.Pause("highlight");
      }
      else
      {
        DOTween.Restart("highlight");
      }
      isPlaying = !isPlaying;
    }
  }
}
