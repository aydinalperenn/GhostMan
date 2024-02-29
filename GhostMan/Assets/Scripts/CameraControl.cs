using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public GameControl gameControlScript;


    float sensibility = 2f;     // hassasiyet
    float softness = 2f;        // yumuþaklýk

    Vector2 switchPosition;     // geçiþ pozisyonu
    Vector2 camPosition;        // kamera pozisyonu

    GameObject playerObj;

    void Start()
    {
        playerObj = transform.parent.gameObject;
        camPosition.y = 6f;
    }


    void Update()
    {
        if (gameControlScript.isGameContinue)        // oyun devam ediyorsa
        {
            Vector2 mousePos = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // daha gerçeðe yakýn hareket için
            mousePos = Vector2.Scale(mousePos, new Vector2(softness * sensibility, softness * sensibility));
            switchPosition.x = Mathf.Lerp(switchPosition.x, mousePos.x, 1f / softness);
            switchPosition.y = Mathf.Lerp(switchPosition.y, mousePos.y, 1f / softness);

            camPosition += switchPosition;
            if (camPosition.y > 85)
            {
                camPosition.y = 85;
            }
            else if (camPosition.y < -75)
            {
                camPosition.y = -75;
            }

            transform.localRotation = Quaternion.AngleAxis(-camPosition.y, Vector3.right);   // saðdaki vector 3 kýsmý þu þekilde: kameraya yatay doðrultuda bir þiþ saplayýp ekseninde döndürüyor
            playerObj.transform.localRotation = Quaternion.AngleAxis(camPosition.x, playerObj.transform.up);    // oyuncuya dikey doðrultuda bir þiþ saplayýp yatay doðrultuda döndürüyor
        }


    }
}
