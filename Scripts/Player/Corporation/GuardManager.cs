using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Manticore
{
  public class GuardManager : GraphicsElement
  {
    public bool FirstMove = true;
    private TileManager currentTile;
    public TileManager CurrentTile
    {
      get => currentTile;
      set
      {
        // If tile actually changes, change previous tile
        if (CurrentTile != value)
        {
          PreviousTile = currentTile;
        }
        currentTile = value;
      }
    }
    public TileManager PreviousTile { get; private set; }
    private MapManager mapManager;
    // This is same for all guards => static property so we can set it anywhere for all guards.
    public static bool CanMoveBackNextTurn { get; set; } = false;
    public List<TileManager> NextAllowedTiles
    {
      get
      {
        // Only allow momvement to tiles
        List<TileManager> otherGuardTiles = GeneralManager.Guards.FindAll((it) => it != this).ConvertAll<TileManager>((it) => it.CurrentTile);
        if (CurrentTile == null)
        {
          return mapManager != null ? mapManager.GuardStartingTiles.ToList().FindAll((it) => !otherGuardTiles.Contains(it)) : new List<TileManager>();
        }
        else
        {
          List<TileManager> nextAllowedTiles = CurrentTile.AdjacentTiles.FindAll((tile) =>
            // !otherGuardTiles.Contains(tile) && // Another guard is in not already in the same tile
            tile != PreviousTile && // Tile wasn't previous tile
            !tile.GetDoor(CurrentTile).IsWindow // Next tile isn't a window
          );
          foreach (TileManager tile in nextAllowedTiles)
          {
            Debug.Log(tile.name);
          }
          if (nextAllowedTiles.Count > 0 && !CanMoveBackNextTurn)
            return nextAllowedTiles;
          // If alarm triggered last turn or guard has no other way to go, allow moving backwards
          else
          {
            return CurrentTile.AdjacentTiles.FindAll((tile) => !otherGuardTiles.Contains(tile) && !tile.GetDoor(CurrentTile).IsWindow);
          }
        }
      }
    }
    protected override void Awake()
    {
      base.Awake();
      mapManager = GameObject.Find("Map").GetComponent<MapManager>();
      // Make sure this guard is added to GeneralManager. This is needed here when initializing guard list again on reconnect.
      if (GeneralManager.Guards.Find((it) => it == this) == null)
      {
        GeneralManager.Instance.AddGuard(this);
      }
      gameObject.SetActive(false);
    }


    public void HighlightAllowedMoves()
    {
      if (mapManager == null) 
        mapManager = GameObject.Find("Map").GetComponent<MapManager>();
      mapManager.HighlightGuard(this);
      mapManager.HighlightTiles(NextAllowedTiles, false);
      if (CurrentTile != null) CurrentTile.ChangeAlpha(GraphicsUtils.LIGHT_DARK_ALPHA);
      if (FirstMove && GeneralManager.CEOComputerHacked)
      {
        GeneralUIManager uiManager = GeneralManager.LocalPlayer.UIManager;
        uiManager.OpenPrompt("New guard has arrived!","Choose a starting tile for the new guard.");
        CameraManager cameraManager = GameObject.Find("Main Camera").GetComponent<CameraManager>();
        cameraManager.CenterCamera();
      }
      FirstMove = false;
    }

    public void Move(TileManager tile)
    {
      CurrentTile = tile;
      Vector3 newPosition = new Vector3((tile.transform.position.x + tile.TilePositionOffsetX), (tile.transform.position.y + tile.TilePositionOffsetY), transform.position.z);
      transform.position = newPosition;
      tile.transform.GetChild(1).gameObject.SetActive(false);
      HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
      // If is player is corporation
      if (hacker == null)
      {
        gameObject.SetActive(true);
      }
      else if (hacker.CurrentTile != null && (hacker.CurrentTile == tile || hacker.CurrentTile.AdjacentTiles.Find((it) => it == tile) != null))
      { // Hacker CurrentTile or AdjacentTile is Guard's tile
        gameObject.SetActive(true);
      }
      else
      {
        gameObject.SetActive(false);
      }
    }
  }
}
