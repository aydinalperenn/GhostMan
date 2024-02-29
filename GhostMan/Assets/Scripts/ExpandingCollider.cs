using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandingCollider : MonoBehaviour
{
    PacmanControl pacmanControlScript;

    public GameObject Pacman;

    public string foodTag = "Food_Small"; // Hedef tag
    public float expansionSpeed = 1f; // B�y�me h�z�

    public bool isExpanding = false; // Collider b�y�t�l�yor mu?

    private SphereCollider sphereCollider; // SphereCollider bile�eni
    float startRadius;
    //public List<GameObject> collidedGameObjects; // �arpt��� nesnelerin tutulaca�� liste


    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>(); // SphereCollider bile�enini al
        startRadius = sphereCollider.radius;
        //collidedGameObjects = new List<GameObject>(); // Bo� bir diziyle ba�lat�l�yor

        pacmanControlScript = FindObjectOfType<PacmanControl>().GetComponent<PacmanControl>();
    }

    private void Update()
    {
        transform.position = Pacman.transform.position;
        // di�er koddan isexpanding true olarak de�i�tirildi�inde collider b�y�meye ba�layacak.
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
        //        isExpanding = false;    // collider belirli taga sahip objeye �arpt���nda b�y�meyi b�rakacak ve ba�lang�� radiusuna d�necek.
        //                                //collidedGameObjects.Add(other.gameObject);
        //        pacmanControlScript.collidedFoodsWhileColliderExpanding.Add(other.gameObject);
        //        sphereCollider.radius = startRadius;

        //    }
        //}

        if (other.CompareTag(foodTag))
        {
           // isExpanding = false;    // collider belirli taga sahip objeye �arpt���nda b�y�meyi b�rakacak ve ba�lang�� radiusuna d�necek.
                                    //collidedGameObjects.Add(other.gameObject);
            pacmanControlScript.collidedFoodsWhileColliderExpanding.Add(other.gameObject);
            sphereCollider.radius = startRadius;

        }

    }
}
