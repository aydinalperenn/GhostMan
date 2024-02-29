using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandingCollider : MonoBehaviour
{
    PacmanControl pacmanControlScript;

    public GameObject Pacman;

    public string foodTag = "Food_Small"; // Hedef tag
    public float expansionSpeed = 1f; // Büyüme hýzý

    public bool isExpanding = false; // Collider büyütülüyor mu?

    private SphereCollider sphereCollider; // SphereCollider bileþeni
    float startRadius;
    //public List<GameObject> collidedGameObjects; // Çarptýðý nesnelerin tutulacaðý liste


    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>(); // SphereCollider bileþenini al
        startRadius = sphereCollider.radius;
        //collidedGameObjects = new List<GameObject>(); // Boþ bir diziyle baþlatýlýyor

        pacmanControlScript = FindObjectOfType<PacmanControl>().GetComponent<PacmanControl>();
    }

    private void Update()
    {
        transform.position = Pacman.transform.position;
        // diðer koddan isexpanding true olarak deðiþtirildiðinde collider büyümeye baþlayacak.
        if (isExpanding)
        {
            float currentRadius = sphereCollider.radius;
            float targetRadius = currentRadius + (expansionSpeed * Time.deltaTime);

            sphereCollider.radius = Mathf.Lerp(currentRadius, targetRadius, expansionSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (isExpanding)
        //{
        //    if (other.CompareTag(foodTag))
        //    {
        //        isExpanding = false;    // collider belirli taga sahip objeye çarptýðýnda büyümeyi býrakacak ve baþlangýç radiusuna dönecek.
        //                                //collidedGameObjects.Add(other.gameObject);
        //        pacmanControlScript.collidedFoodsWhileColliderExpanding.Add(other.gameObject);
        //        sphereCollider.radius = startRadius;

        //    }
        //}

        if (other.CompareTag(foodTag))
        {
           // isExpanding = false;    // collider belirli taga sahip objeye çarptýðýnda büyümeyi býrakacak ve baþlangýç radiusuna dönecek.
                                    //collidedGameObjects.Add(other.gameObject);
            pacmanControlScript.collidedFoodsWhileColliderExpanding.Add(other.gameObject);
            sphereCollider.radius = startRadius;

        }

    }
}
