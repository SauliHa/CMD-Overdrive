using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Manticore
{
  public class EventUtils
  {
    public static TileManager GetTileOverMouse()
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;

      //Check if click/touch hit something, and if it hit a tile
      if (Physics.Raycast(ray, out hit) && (hit.transform.tag == "Tile") && (!EventSystem.current.IsPointerOverGameObject() || IsPointerOverUIBorder()))
      {
        TileManager clickedTile = hit.transform.GetComponent<TileManager>();
        return clickedTile;
      }
      return null;
    }
    public static bool IsPointerOverUIBorder()
    {
      GameObject currentObj = EventSystem.current.currentSelectedGameObject;
      return EventSystem.current.IsPointerOverGameObject() && currentObj != null && currentObj.CompareTag("UIBorder");
    }
    public static DoorManager GetDoorOverMouse()
    {
      GameObject obj = GetObjectOverMouse();

      //Check if click/touch hit something, and if it hit a tile
      if (obj != null && (obj.tag == "Door") && (!EventSystem.current.IsPointerOverGameObject() || IsPointerOverUIBorder()))
      {
        DoorManager clickedDoor = obj.GetComponent<DoorManager>();
        return clickedDoor;
      }
      return null;
    }

    public static GameObject GetObjectOverMouse()
    {
      Vector3 mousePos = Input.mousePosition;
      // Z of raycast needs to be set to the distance of camera and obj. Since all objects are on z == 0 => distance is absolute value of camera's Z
      // EDIT: Not sure if has to be exactly the distance, but has to be more than the z pos of the object. This works atleast.
      RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Math.Abs(Camera.main.transform.position.z))), Vector2.zero);
      if (hit.collider != null) return hit.collider.gameObject;
      return null;
    }
  }
}