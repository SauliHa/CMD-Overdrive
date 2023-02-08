using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manticore
{
  public class AudioManager : MonoBehaviour
  {
    [SerializeField]
    private AudioClip successClip, messageClip;
    private AudioSource source;
    private void Awake()
    {
      source = GetComponent<AudioSource>();
      
    }

    public void PlaySuccess()
    {
      source.PlayOneShot(successClip, 0.3f);
    }
    public void PlayMessage()
    {
      source.PlayOneShot(messageClip);
    }
  }
}
