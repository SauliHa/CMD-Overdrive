using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class for camera controls such as panning camera with touch
/// </summary>

namespace Manticore
{
    public class CameraManager : MonoBehaviour
    {
        private Vector3 touchStart;
        public float groundZ = 0;
        public float minDistance;
        public float maxDistance;
        private Vector2 center = new Vector2(11, -13);
        private float minOffSetX;
        private float maxOffSetX;
        private float minOffSetY;
        private float maxOffSetY;

        private void Start()
        {
            minOffSetX = center.x -15;
            maxOffSetX = center.x + 15;
            minOffSetY = center.y - 14;
            maxOffSetY = center.y + 14;
            Vector3 newPosition = new Vector3(11, -13, -30);
            this.transform.position = newPosition;
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
                Vector3 direction = touchStart - GetWorldPosition(groundZ);
                Vector3 pos = transform.position += direction;
                pos.x = Mathf.Clamp(pos.x, minOffSetX, maxOffSetX);
                pos.y = Mathf.Clamp(pos.y, minOffSetY, maxOffSetY);
                this.gameObject.transform.position = pos;
            }
            Zoom(Input.GetAxis("Mouse ScrollWheel") * 5);

        }
        
        private Vector3 GetWorldPosition(float z)
        {
            Ray touchPos = Camera.main.ScreenPointToRay(Input.mousePosition);
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

        public void CenterCamera(GameObject target)
        {
            Vector3 newPosition = new Vector3(target.transform.position.x, target.transform.position.y, this.transform.position.z);
            this.transform.position = newPosition;
        }
        
        //center camera to the center of the map
        public void CenterCamera()
        {
            Vector3 newPosition = new Vector3(11, -13, this.transform.position.z);
            this.transform.position = newPosition;
        }

        public void startingRoomZoom()
        {
            Vector3 newPosition = this.transform.position;
            newPosition.z = -22;
            this.transform.position = newPosition;
        }
    }
}

