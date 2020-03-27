using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pointer 

{
    public string name;
    public Texture2D cursorIcon;

    public string Name 
    {
        get { return name; }   // get method
        set { name = value; }  // set method
    }

    public Texture2D CursorIcon
    {
        get { return cursorIcon; }   // get method
        set { cursorIcon = value; }  // set method
    }
}
