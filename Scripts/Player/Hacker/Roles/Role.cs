using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

namespace Manticore
{
  public enum RoleType
  {
    Corporation,
    Edge,
    Shift,
    Bugz,
    Chroma
  }
  /// <summary>
  /// Script for a single role
  /// </summary>
  [CreateAssetMenu(fileName = "New role", menuName = "Roles")]
  public class Role : ScriptableObject
  {
    public string Name;
    public string Description;
    public Sprite IconSprite;
    public Sprite ImageSprite;
    public RoleType RoleType;
    public Color RoleColor;
  }
}

