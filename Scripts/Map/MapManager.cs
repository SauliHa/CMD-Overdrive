using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Malee.List;
using DG.Tweening;

namespace Manticore
{
  public class MapManager : MonoBehaviour
  {
    public List<TileManager> Tiles { get; private set; } = new List<TileManager>();
    public List<DoorManager> Doors { get; private set; } = new List<DoorManager>();
    private GameObject tilesContainer;
    private GameObject doorsContainer;

    [SerializeField]
    private TilesList startingTiles;
    private List<TileManager> startingTileList { get => startingTiles.ToList(); }
    [SerializeField]
    private TilesList guardStartingTiles;
    [SerializeField]
    private TilesList guardStartingTilesAfterBreach;
    public TilesList GuardStartingTiles { get { if (GeneralManager.CEOComputerHacked) { return startingTiles; } return guardStartingTiles; } }

    [SerializeField]
    private TilesList cameraTiles;
    public List<TileManager> CameraTiles { get => cameraTiles.ToList(); }
    private static Color DARKENED_COLOR = new Color(1f, 1f, 1f, 0.1f);
    private static Color DEFAULT_COLOR = new Color(1f, 1f, 1f, 1f);
    public List<LockTileManager> LockTiles
    {
      get
      {
        return Tiles.FindAll((tile) => tile is LockTileManager).Cast<LockTileManager>().ToList();
      }
    }
    //public bool AreAllLocksOpen { get => LockTiles.Find((tile) => tile.IsLocked) == null; } // not in use since only 3 locks needs to be opened
    public bool ArethreeOutOfFourLocksOpen { get => CanCeoDoorBeOpened(); }
    public List<DoorManager> OpenDoors { get => Doors.FindAll((it) => !it.IsLockedForCorporation && !it.HasMotionDetector && !it.IsWindow); }
    // Allow door locking to doors that are not windows and do not have motion detectors
    public List<DoorManager> DoorsToBeLocked { get => Doors.FindAll((it) => !it.HasMotionDetector && !it.IsWindow); }
    public List<DoorManager> OpenDoorsAndWindows { get => Doors.FindAll((it) => !it.IsLocked && !it.HasMotionDetector); }
    public List<DoorManager> LockedDoorsForCorporation { get => Doors.FindAll((it) => it.IsLockedForCorporation && !it.IsCEODoor); }
    public List<DoorManager> DoorsWithMotionDetector { get => Doors.FindAll((it) => it.HasMotionDetector); }
    private void Awake()
    {
      tilesContainer = transform.Find("Tiles").gameObject;
      doorsContainer = transform.Find("Doors").gameObject;
      foreach (Transform child in tilesContainer.transform)
      {
        Tiles.Add(child.GetComponent<TileManager>());
      }
      foreach (Transform child in doorsContainer.transform)
      {
        DoorManager door = child.GetComponent<DoorManager>();
        // Remove all locks and motion detectors from doors when starting
        // NOTE: If you want to test by locking many doors on dev, comment these two lines
        if (door.IsCEODoor)
        {
          door.IsLocked = true;
          door.IsLockedForCorporation = true;
        }
        else
        {
          door.IsLocked = false;
          door.IsLockedForCorporation = false;
          door.HasMotionDetector = false;
        }
        Doors.Add(door);
      }
    }
    //Naming of tiles
    // private void OnValidate()
    // {
    //   Tiles.Clear();
    //   tilesContainer = transform.Find("Tiles").gameObject;
    //   foreach (Transform child in tilesContainer.transform)
    //   {

    //     Tiles.Add(child.GetComponent<TileManager>());
    //     child.gameObject.name = "Room " + Tiles.Count.ToString();
    //   }
    // }

    private void Update()
    {
      if (Input.GetKeyDown(KeyCode.Return))
      {
        int doorIndex = UnityEngine.Random.Range(0, Doors.Count - 1);
        DoorManager door = Doors[doorIndex];
        door.IsLocked = !door.IsLocked;
      }
    }

    private bool CanCeoDoorBeOpened()
    {
      int i = 0;
      foreach (var tile in LockTiles)
      {
        if (!tile.IsLocked)
        {
          i++;
        }
      }
      if (i >= 3)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
    
    public void HighlightTiles(List<TileManager> highlightedTiles, bool useAnimation = true)
    {
      foreach (TileManager tile in Tiles)
      {
        // Make other tiles darker (50% transparent) => highlighted tiles pops out
        if (highlightedTiles.FindIndex((it) => it == tile) < 0)
        {
          tile.Darken();
        }
        else if (useAnimation)
        {
          tile.StartHighlightAnimation();
        }
        else
        {
          tile.Lighten();
        }
      }
    }
    public void HighlightDoors(List<DoorManager> highlightedDoors, bool useAnimation = true)
    {
      foreach (DoorManager door in Doors)
      {
        // Make other doors darker (50% transparent) => doors tiles pops out
        if (highlightedDoors.FindIndex((it) => it == door) < 0)
        {
          door.Darken();
        }
        else if (useAnimation)
        {
          door.StartHighlightAnimation();
          door.StartScaleAnimation();
        }
        else
        {
          door.Lighten();
        }
      }
    }
    public void HighlightGuard(GuardManager highlightedGuard)
    {
      foreach (GuardManager guard in GeneralManager.Guards)
      {
        if (guard == highlightedGuard) guard.Lighten();
        else guard.Darken();
      }
    }
    public void HighlightStartingTiles()
    {
      HighlightTiles(startingTileList);
    }

    public bool CheckIfStartingTile(TileManager clickedTile)
    {
      foreach (TileManager tile in startingTileList)
      {
        if (tile == clickedTile)
        {
          return true;
        }
      }
      return false;
    }

    public void RemoveHighlighting()
    {
      DOTween.Pause("highlight");
      DOTween.Pause("scale");
      HackerManager hacker = GeneralManager.LocalPlayer as HackerManager;
      if (hacker != null)
      {
        TileManager currentTile = hacker.CurrentTile;
        foreach (TileManager tile in Tiles)
        {
          if (currentTile == null)
          {
            tile.Darken();
            break;
          }
          // On default mode, make current tile lighter than adjacent tiles and other than that all others dark
          if (tile == currentTile)
          {
            tile.Lighten();
          }
          else if (currentTile.AdjacentTiles.Find((adjacentTile) => adjacentTile == tile) == tile)
          {
            tile.ChangeAlpha(GraphicsUtils.LIGHT_DARK_ALPHA);
          }
          else
          {
            tile.Darken();
          }
        }
        foreach (DoorManager door in Doors)
        {
          if (currentTile == null)
          {
            door.Darken();
          }
          else if (door.HasTile(currentTile))
          {
            door.Lighten();
          }
          else
          {
            door.Darken();
          }
        }
      }
      else
      {
        foreach (TileManager tile in Tiles)
        {
          tile.Lighten();
        }
        foreach (DoorManager door in Doors)
        {
          door.Lighten();
        }
        foreach (GuardManager guard in GeneralManager.Guards)
        {
          guard.Lighten();
        }

      }
    }
    public void DarkenDoors()
    {
      foreach (DoorManager door in Doors)
      {
        door.Darken();
      }
    }
    public void DarkenTiles()
    {
      foreach (TileManager tile in Tiles)
      {
        tile.Darken();
      }
    }
    public void DarkenGuards()
    {
      foreach (GuardManager guard in GeneralManager.Guards)
      {
        guard.Darken();
      }
    }

    public DoorManager FindDoor(TileManager firstTile, TileManager secondTile)
    {
      return Doors.Find((it) => it.HasTile(firstTile) && it.HasTile(secondTile));
    }

    [Serializable]
    public class TilesList : ReorderableArray<TileManager> { }
  }
}
