using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerChanger : MonoBehaviour
{
    [SerializeField]
    public Pointer[] mousePointers;

    public string prevPointer;

    // Update is called once per frame
    public void SetCursor(string pointer)
    {
        // use previous pointer perameter to prevent for loop from running continuously
        if (prevPointer != pointer)
        {
            //Itterate through the set cursor 'pointers'
            foreach (Pointer item in mousePointers)
            {

                if (pointer == item.name)
                {                 
                    Cursor.SetCursor(item.cursorIcon, Vector2.zero, CursorMode.Auto);
                    break;
                }
                /*else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
                }*/
            }
            prevPointer = pointer;
        }

    }
}
