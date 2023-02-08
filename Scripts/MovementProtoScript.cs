using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for simple movement of player pawn on top of a game board.
/// Camera can be moved by pressing and dragging on top of the map, while pawn can be moved by dragging it.
/// </summary>


namespace Manticore
{
    public class MovementProtoScript : MonoBehaviour
    {
        
        private Vector3 touchStart;
        private bool draggingPiece;
        private GameObject player;
        public float groundZ = 0;
        private float distanceToObject;
        private Vector3 newPos;
        private Vector3 offset;
        public float minDistance;
        public float maxDistance;

        void Start()
        {
            draggingPiece = false;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchStart = GetWorldPosition(groundZ);
            }
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                Zoom(difference * 0.01f);
            }
            else if (Input.GetMouseButton(0))
            {

                if (draggingPiece)
                {
                    newPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToObject);
                    newPos = Camera.main.ScreenToWorldPoint(newPos);
                    player.gameObject.transform.position = newPos + offset;
                }
                else
                {
                    Vector3 direction = touchStart - GetWorldPosition(groundZ);
                    this.gameObject.transform.position += direction;
                }

            }
            if (Input.GetMouseButtonUp(0))
            {
                draggingPiece = false;
            }
            Zoom(Input.GetAxis("Mouse ScrollWheel") * 5);
        }
        private Vector3 GetWorldPosition(float z)
        {
            Ray touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //Use Raycast to determine if player's touch input hit the collider of the pawn or not
            if (Physics.Raycast(touchPos, out hit))
            {
                if (hit.collider != null)
                {
                    player = hit.transform.gameObject;
                    distanceToObject = Vector3.Distance(player.transform.position, Camera.main.transform.position);
                    newPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToObject);
                    newPos = Camera.main.ScreenToWorldPoint(newPos);
                    offset = player.transform.position - newPos;
                    draggingPiece = true;
                }
                else
                {
                    draggingPiece = false;
                }
            }
            Plane ground = new Plane(Vector3.forward, new Vector3(0, 0, z));
            float distance;
            ground.Raycast(touchPos, out distance);
            return touchPos.GetPoint(distance);
        }
        private void Zoom(float increment)
        {
            Vector3 newPosition = this.transform.position;
            newPosition.z = Mathf.Clamp(newPosition.z + increment, minDistance, maxDistance);
            this.transform.position = newPosition;
        }
    }
}


