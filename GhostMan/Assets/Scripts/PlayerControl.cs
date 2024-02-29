using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public GameControl gameControlScript;

    float x;
    float y;
    float speed = 5f;


    void Start()
    {

    }


    void Update()
    {
        if (gameControlScript.isGameContinue)       // oyun devam ediyorsa
        {
            x = Input.GetAxis("Horizontal");        // yatay eksen
            y = Input.GetAxis("Vertical");          // diken eksen
        }
    }

    private void FixedUpdate()
    {
        if (gameControlScript.isGameContinue)   // oyun devam ediyorsa
        {

            // hareketlerin daha yumuþak gerçekleþmesi için
            x *= Time.deltaTime * speed;
            y *= Time.deltaTime * speed;

            transform.Translate(x, 0f, y);

        }
        
    }

   
}
