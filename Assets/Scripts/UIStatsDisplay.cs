using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStatsDisplay : MonoBehaviour
{
    public GameObject statsDisplay;
    

    // Start is called before the first frame update

    private void Start()
    {
        statsDisplay.SetActive(false);
    }
    private void OnMouseOver()
    {

        statsDisplay.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y * 0.2f);
        Debug.Log("Show UI");
        statsDisplay.SetActive(true);
    }

    private void OnMouseExit()
    {
        Debug.Log("Hide UI");
        statsDisplay.SetActive(false);
    }
}
