using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    public bool isGameContinue = false;


    void Start()
    {
        isGameContinue = true;
        Cursor.visible = false;
    }


    void Update()
    {

    }
}
