using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manticore
{
  public class PromptMessage
  {
    public string Title { get; set; }
    public string Description { get; set; }
    public Action OnAccept { get; set; }
    public Action OnCancel { get; set; }
    public bool HasCancelButton { get; set; } = true;

    public PromptMessage(string title, string description, Action onAccept, Action onCancel, bool hasCancelButton = true)
    {
      Title = title;
      Description = description;
      OnAccept = onAccept;
      OnCancel = onCancel;
      HasCancelButton = hasCancelButton;
    }
    public PromptMessage(string title, string description, Action onAccept)
    {
      Title = title;
      Description = description;
      OnAccept = onAccept;
      OnCancel = () => { };
      HasCancelButton = true;
    }
    public PromptMessage(string title, string description)
    {
      Title = title;
      Description = description;
      OnAccept = () => { };
      OnCancel = () => { };
      HasCancelButton = true;
    }
  }
}
