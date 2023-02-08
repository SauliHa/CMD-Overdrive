using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollFocusHandler : MonoBehaviour
{
    private ScrollRect scrollRect;
    private int limit = 720;
    // Start is called before the first frame update
    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        scrollRect.horizontalNormalizedPosition = 0f;
    }

    // Update is called once per frame
    
    void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            float pos = scrollRect.horizontalNormalizedPosition;
            if (pos < 0.01f)
            {
                scrollRect.horizontalNormalizedPosition = 0f;
            }
            else if(pos < 0.26f)
            {
                if(pos > 0.24) scrollRect.horizontalNormalizedPosition = 0.25f;
            }
            else if (pos < 0.51f)
            {
                if(pos > 0.49) scrollRect.horizontalNormalizedPosition = 0.5f;
            }
            else if (pos < 0.76f)
            {
                if(pos > 0.74) scrollRect.horizontalNormalizedPosition = 0.75f;
            }
            else if(pos > 0.99)
            {
                scrollRect.horizontalNormalizedPosition = 1f;
            }
        }
    }
    
}
