using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileSelectionManager : MonoBehaviour
{
    
    [SerializeField]
    private GameObject scroll1;
    [SerializeField]
    private GameObject scroll2;
    [SerializeField] 
    private TextMeshProUGUI errorText;
    private ScrollRect scrollRect;
    private ScrollRect scrollRect2;
    private int firstNumber;
    private int secondNumber;
    private int roomNumber;
    private void Awake()
    {
        scrollRect = scroll1.GetComponent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 1f;
        scrollRect2 = scroll2.GetComponent<ScrollRect>();
        scrollRect2.verticalNormalizedPosition = 1f;
    }

    // Update is called once per frame
    
    void Update()
    {
        float control = 0.135f;
        float numberpos = 0;
       
        if (!Input.GetMouseButton(0))
        {
            firstNumber = 4;
            float pos = scrollRect.verticalNormalizedPosition;
            while (pos > control)
            {
                control = control + 0.25f;
                numberpos = numberpos + 0.25f;
                firstNumber--;
            }
            scrollRect.verticalNormalizedPosition = numberpos;
        }
        if (firstNumber == 4)
        {
            scrollRect2.verticalNormalizedPosition = 1;
            secondNumber = 0;
            return;
        }
        control = 0.055f;
        numberpos = 0;
        if (!Input.GetMouseButton(0))
        {
            secondNumber = 9;
            float pos2 = scrollRect2.verticalNormalizedPosition;
            while (pos2 > control)
            {
                control = control + 0.11111f;
                numberpos = numberpos + 0.11111f;
                secondNumber--;
            }
            scrollRect2.verticalNormalizedPosition = numberpos;
        }
    }

    public void RoomSelected()
    {
        string room = "" + firstNumber + secondNumber;
        int theRoom = int.Parse(room);
        if (theRoom <= 40 && theRoom > 0)
        {
            scrollRect.verticalNormalizedPosition = 1f;
            scrollRect2.verticalNormalizedPosition = 1f;
            roomNumber = theRoom;
        }
        else
        {
            errorText.color = Color.red;
            errorText.text = "The room number must be between 1 and 40";
        }
    }

    public int GetTile()
    {
        int number = roomNumber;
        roomNumber = 0;
        return number;
        
    }
    
    public void Cancel()
    {
      //gameObject.SetActive(false);
    }
}
