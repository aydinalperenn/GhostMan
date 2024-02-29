using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

public class PacmanControl : MonoBehaviour
{
    ExpandingCollider expandingColliderScript;

    public List<GameObject> collidedFoodsWhileColliderExpanding = new List<GameObject>();

    private Dictionary<GameObject, List<GameObject>> adjacencyList;

    FoodManager foodManagerScript;

    NavMeshAgent navMeshAgent;

    GameObject[] foods;


    public Transform player; // oyuncunun pozisyonunu takip etmek i�in kullan�lacak
    public float minDistanceToPlayer = 10f; // d��man�n oyuncuya ne kadar yak�n olmas� gerekti�i


    private void Awake()
    {
        adjacencyList = new Dictionary<GameObject, List<GameObject>>();

        // Tag'i "Food_Small" olan t�m yiyecekleri bulun
        foods = GameObject.FindGameObjectsWithTag("Food_Small");

        // Her yiyece�i adjacency listesine ekleyin ve kom�u yiyecekleri bulun
        foreach (GameObject food in foods)
        {
            adjacencyList[food] = new List<GameObject>();

            // Belli bir mesafeden daha yak�n olan kom�u yiyecekleri bulun
            Collider[] colliders = Physics.OverlapSphere(food.transform.position, 5.5f);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Food_Small") && collider.gameObject != food)
                {
                    adjacencyList[food].Add(collider.gameObject);
                }
            }
        }
    }


    private void Start()
    {
        // Olu�turulan adjacency listesini konsola yazd�r�n
        PrintGraph();

        navMeshAgent = GetComponent<NavMeshAgent>();
        foodManagerScript = FindObjectOfType<FoodManager>().GetComponent<FoodManager>();

        expandingColliderScript = FindObjectOfType<ExpandingCollider>().GetComponent<ExpandingCollider>();

        GameObject go;
        go = FindClosestFood();
        navMeshAgent.SetDestination(go.transform.position);
    }


    float distanceToPlayer;
    Vector3 direction;
    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= minDistanceToPlayer)
        {
            // Oyuncuya yakla�t�k�a h�z� art�r
            navMeshAgent.speed = 7f;
            //direction = (transform.position - player.transform.position).normalized;

            expandingColliderScript.isExpanding = true;
            
        }
        else
        {
            // Oyuncudan uzakla�t�k�a h�z� d���r
            expandingColliderScript.isExpanding = false;
            navMeshAgent.speed = 3.5f;
        }


    }

    private void LateUpdate()
    {
        if (distanceToPlayer <= minDistanceToPlayer)
        {
            if (collidedFoodsWhileColliderExpanding.Count != 0)
            {
                Vector3 direction;
                Vector3 foodDirection;
                GameObject targetFood = null;
                float targetAngle = 60;
                foreach(GameObject food in collidedFoodsWhileColliderExpanding)
                {
                    if (food != null)
                    {
                        direction = (transform.position - player.transform.position).normalized;
                        foodDirection = (food.transform.position - transform.position).normalized;
                        float angle = Vector3.Angle(direction, foodDirection);
                        Debug.Log(angle);
                        if (angle < targetAngle)
                        {
                            targetFood = food;
                        }
                    }
                    
                }

                if (targetFood != null)
                {
                    navMeshAgent.SetDestination(targetFood.transform.position);
                }


            }
        }
    }





    Collider yenilecekYemek;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Food_Small"))
        {
            yenilecekYemek = other;
            // Yemekten kom�ular�n oldu�u listeyi al
            List<GameObject> neighbors = adjacencyList[other.gameObject];

            // Y�n�ne y�nlendir
            GameObject closestNeighbor = FindClosestNeighbor(neighbors);
            if (closestNeighbor != null)
            {
                try
                {
                    navMeshAgent.SetDestination(closestNeighbor.transform.position);
                }

                catch (Exception ex)
                {
                    Debug.Log(ex);
                }




            }

            // Yeme�i adjacency listesinden ��kar
            RemoveFoodFromAdjacencyList(other.gameObject);
            if (collidedFoodsWhileColliderExpanding.Contains(other.gameObject))
            {
                collidedFoodsWhileColliderExpanding.Remove(other.gameObject);
            }
            
            Destroy(other.gameObject);
            foodManagerScript.addSmallFoodScore();

            Debug.Log("TestSmall");
        }
    }

    private GameObject FindClosestNeighbor(List<GameObject> neighbors)
    {
        GameObject closestNeighbor = null;
        if (neighbors.Count != 0)
        {

            float closestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;

            foreach (GameObject neighbor in neighbors)
            {
                if (neighbor != null)   // Hala sahnede mevcutsa
                {
                    float distance = Vector3.Distance(currentPosition, neighbor.transform.position);
                    if (distance < closestDistance)
                    {
                        closestNeighbor = neighbor;
                        closestDistance = distance;
                    }
                }

            }
        }
        //else
        //{   // 
        //    if (adjacencyList.Keys.Count != 0)      // kom�usu yoksa ve oyun sahnesinde hala yemek varsa
        //    {   // de�i�tirdim, normalde findclosestfood

        //        closestNeighbor = FindClosestFood();

        //    }
        //}
        else
        {
            closestNeighbor = GetRandomClosestFood(); // Rastgele bir yiyecek ata        
        }


        return closestNeighbor;
    }

    private GameObject FindClosestFood()
    {
        GameObject closestFood = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        // adjacencyList'in source k�sm�ndaki yemekleri kontrol edin
        foreach (var food in adjacencyList.Keys)
        {
            float distance = Vector3.Distance(currentPosition, food.transform.position);
            if (distance < closestDistance && food != null && adjacencyList.ContainsKey(food))
            {
                closestFood = food;
                closestDistance = distance;
            }
        }

        //return closestFood;


        // closestFood null de�ilse ve distance sonsuz de�ilse
        if (closestFood != null && closestDistance != Mathf.Infinity)
        {
            return closestFood;
        }
        else
        {
            return GetRandomClosestFood(); // Rastgele bir yemek d�nd�r
        }

    }



    private GameObject GetRandomClosestFood()
    {
        List<GameObject> availableFoods = new List<GameObject>();

        foreach (GameObject food in foods)
        {
            // (Vector3.Distance(food.transform.position, this.transform.position) > 1)
            if (food != null && adjacencyList.ContainsKey(food) && food != yenilecekYemek.gameObject) // kendini eklemeyecek
            {
                availableFoods.Add(food);
            }
        }

        if (availableFoods.Count > 0)
        {
            int index = 0;
            int indexOfClosestFood = -1;
            float closestDistance = Mathf.Infinity;
            float distance;
            Vector3 currentPosition = transform.position;
            foreach (GameObject food in availableFoods)
            {
                distance = Math.Abs(Vector3.Distance(currentPosition, food.transform.position));    // mutlak de�er Abs
                if (distance < closestDistance)
                {
                    indexOfClosestFood = index;
                    closestDistance = distance;
                }
                index++;
            }
            return availableFoods[indexOfClosestFood];



            //int randomIndex = UnityEngine.Random.Range(0, availableFoods.Count);
            //return availableFoods[randomIndex];

        }
        else
        {
            return null;
        }
    }

    private void RemoveFoodFromAdjacencyList(GameObject food)
    {
        // Yemekten kom�ular�n oldu�u listeyi al
        List<GameObject> neighbors = adjacencyList[food];

        // Kom�ular� adjacencyList'ten ��kar
        foreach (var neighbor in neighbors)
        {
            adjacencyList[neighbor].Remove(food);
        }

        // Yeme�i adjacencyList'ten ��kar
        adjacencyList.Remove(food);
    }

    private void PrintGraph()
    {
        foreach (var vertex in adjacencyList)
        {
            Debug.Log("Ba�lant�lar " + vertex.Key.name + ": ");
            foreach (var neighbor in vertex.Value)
            {
                Debug.Log(neighbor.name);
            }
            Debug.Log("-----------------------");
        }
    }
}














//// Optimize yar�m kod
//using UnityEngine;
//using System.Collections.Generic;
//using UnityEngine.AI;
//using System;

//public class PacmanControl : MonoBehaviour
//{
//    private Dictionary<GameObject, List<GameObject>> adjacencyList;

//    FoodManager foodManagerScript;

//    NavMeshAgent navMeshAgent;

//    GameObject[] foods;


//    public Transform player; // oyuncunun pozisyonunu takip etmek i�in kullan�lacak
//    public float minDistanceToPlayer = 10f; // d��man�n oyuncuya ne kadar yak�n olmas� gerekti�i


//    private void Awake()
//    {
//        adjacencyList = new Dictionary<GameObject, List<GameObject>>();

//        // Tag'i "Food_Small" olan t�m yiyecekleri bulun
//        foods = GameObject.FindGameObjectsWithTag("Food_Small");

//        // Her yiyece�i adjacency listesine ekleyin ve kom�u yiyecekleri bulun
//        foreach (GameObject food in foods)
//        {
//            adjacencyList[food] = new List<GameObject>();

//            // Belli bir mesafeden daha yak�n olan kom�u yiyecekleri bulun
//            Collider[] colliders = Physics.OverlapSphere(food.transform.position, 5.5f);
//            foreach (Collider collider in colliders)
//            {
//                if (collider.CompareTag("Food_Small") && collider.gameObject != food)
//                {
//                    adjacencyList[food].Add(collider.gameObject);
//                }
//            }
//        }
//    }


//    private void Start()
//    {
//        // Olu�turulan adjacency listesini konsola yazd�r�n
//        PrintGraph();

//        navMeshAgent = GetComponent<NavMeshAgent>();
//        foodManagerScript = FindObjectOfType<FoodManager>().GetComponent<FoodManager>();

//        GameObject go;
//        go = FindClosestFood();
//        navMeshAgent.SetDestination(go.transform.position);
//    }

//    private void Update()
//    {
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
//        if (distanceToPlayer <= minDistanceToPlayer)
//        {
//            navMeshAgent.speed = 7f;
//            // Burada oyuncuya z�t y�nde kendine en yak�n yeme�e y�nelecek
//        }
//        else
//        {
//            navMeshAgent.speed = 3.5f;
//            return;
//        }
//    }




//    private void PrintGraph()
//    {
//        foreach (var vertex in adjacencyList)
//        {
//            Debug.Log("Ba�lant�lar " + vertex.Key.name + ": ");
//            foreach (var neighbor in vertex.Value)
//            {
//                Debug.Log(neighbor.name);
//            }
//            Debug.Log("-----------------------");
//        }
//    }


//    Collider yenilecekYemek;
//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.CompareTag("Food_Small"))
//        {
//            yenilecekYemek = other;
//            // Yemekten kom�ular�n oldu�u listeyi al
//            List<GameObject> neighbors = adjacencyList[other.gameObject];

//            // Y�n�ne y�nlendir
//            GameObject closestNeighbor = FindClosestNeighbor(neighbors);
//            if (closestNeighbor != null)
//            {
//                try
//                {
//                    navMeshAgent.SetDestination(closestNeighbor.transform.position);
//                }

//                catch (Exception ex)
//                {
//                    Debug.Log(ex);
//                }




//            }

//            // Yeme�i adjacency listesinden ��kar
//            RemoveFoodFromAdjacencyList(other.gameObject);

//            Destroy(other.gameObject);
//            foodManagerScript.addSmallFoodScore();

//            Debug.Log("TestSmall");
//        }
//    }

//    private GameObject FindClosestNeighbor(List<GameObject> neighbors)
//    {
//        GameObject closestNeighbor = null;
//        if (neighbors.Count != 0)
//        {

//            float closestDistance = Mathf.Infinity;
//            Vector3 currentPosition = transform.position;

//            foreach (GameObject neighbor in neighbors)
//            {
//                if (neighbor != null)   // Hala sahnede mevcutsa
//                {
//                    float distance = Vector3.Distance(currentPosition, neighbor.transform.position);
//                    if (distance < closestDistance)
//                    {
//                        closestNeighbor = neighbor;
//                        closestDistance = distance;
//                    }
//                }

//            }
//        }
//        //else
//        //{   // 
//        //    if (adjacencyList.Keys.Count != 0)      // kom�usu yoksa ve oyun sahnesinde hala yemek varsa
//        //    {   // de�i�tirdim, normalde findclosestfood

//        //        closestNeighbor = FindClosestFood();

//        //    }
//        //}
//        else
//        {
//            closestNeighbor = GetRandomClosestFood(); // Rastgele bir yiyecek ata        
//        }


//        return closestNeighbor;
//    }

//    private GameObject FindClosestFood()
//    {
//        GameObject closestFood = null;
//        float closestDistance = Mathf.Infinity;
//        Vector3 currentPosition = transform.position;

//        // adjacencyList'in source k�sm�ndaki yemekleri kontrol edin
//        foreach (var food in adjacencyList.Keys)
//        {
//            float distance = Vector3.Distance(currentPosition, food.transform.position);
//            if (distance < closestDistance && food != null && adjacencyList.ContainsKey(food))
//            {
//                closestFood = food;
//                closestDistance = distance;
//            }
//        }

//        //return closestFood;


//        // closestFood null de�ilse ve distance sonsuz de�ilse
//        if (closestFood != null && closestDistance != Mathf.Infinity)
//        {
//            return closestFood;
//        }
//        else
//        {
//            return GetRandomClosestFood(); // Rastgele bir yemek d�nd�r
//        }

//    }



//    private GameObject GetRandomClosestFood()
//    {
//        List<GameObject> availableFoods = new List<GameObject>();

//        foreach (GameObject food in foods)
//        {
//            // (Vector3.Distance(food.transform.position, this.transform.position) > 1)
//            if (food != null && adjacencyList.ContainsKey(food) && food != yenilecekYemek.gameObject) // kendini eklemeyecek
//            {
//                availableFoods.Add(food);
//            }
//        }

//        if (availableFoods.Count > 0)
//        {
//            int index = 0;
//            int indexOfClosestFood = -1;
//            float closestDistance = Mathf.Infinity;
//            float distance;
//            Vector3 currentPosition = transform.position;
//            foreach (GameObject food in availableFoods)
//            {
//                distance = Math.Abs(Vector3.Distance(currentPosition, food.transform.position));    // mutlak de�er Abs
//                if (distance < closestDistance)
//                {
//                    indexOfClosestFood = index;
//                    closestDistance = distance;
//                }
//                index++;
//            }
//            return availableFoods[indexOfClosestFood];



//            //int randomIndex = UnityEngine.Random.Range(0, availableFoods.Count);
//            //return availableFoods[randomIndex];

//        }
//        else
//        {
//            return null;
//        }
//    }

//    private void RemoveFoodFromAdjacencyList(GameObject food)
//    {
//        // Yemekten kom�ular�n oldu�u listeyi al
//        List<GameObject> neighbors = adjacencyList[food];

//        // Kom�ular� adjacencyList'ten ��kar
//        foreach (var neighbor in neighbors)
//        {
//            adjacencyList[neighbor].Remove(food);
//        }

//        // Yeme�i adjacencyList'ten ��kar
//        adjacencyList.Remove(food);
//    }


//}

















//// Optimize olmayan yol
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;

//public class PacmanControl : MonoBehaviour
//{
//    FoodManager foodManagerScript;

//    NavMeshAgent navMeshAgent;


//    private List<GameObject> foodList = new List<GameObject>();     // yemekleri i�erisine koymak i�in bir gameobject listesi olu�turdum
//    GameObject closestFood;
//    public Transform player; // oyuncunun pozisyonunu takip etmek i�in kullan�lacak
//    public float minDistanceToPlayer = 10f; // d��man�n oyuncuya ne kadar yak�n olmas� gerekti�i


//    void Start()
//    {
//        navMeshAgent = GetComponent<NavMeshAgent>();
//        foodManagerScript = FindObjectOfType<FoodManager>().GetComponent<FoodManager>();


//        // Oyundaki t�m yiyeceklerin tag'ini kontrol edip foodList'e ekliyorum
//        GameObject[] foodArray = GameObject.FindGameObjectsWithTag("Food_Small");
//        foreach (GameObject food in foodArray)
//        {
//            foodList.Add(food);
//        }
//        closestFood = null;

//    }


//    void Update()
//    {
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
//        if (distanceToPlayer <= minDistanceToPlayer)
//        {
//            Vector3 directionToPlayer = (transform.position - player.position).normalized;
//            Vector3 targetPosition = FindFurthestFood(transform.position, directionToPlayer);
//            navMeshAgent.destination = targetPosition;
//            navMeshAgent.speed = 7f;
//            closestFood = null;
//        }
//        else
//        {
//            navMeshAgent.speed = 3.5f;
//            // Karakterin ve t�m yiyeceklerin konumunu al�p karaktere en yak�n yiyece�i buluyoruz
//            if (closestFood == null)
//            {
//                Vector3 characterPosition = transform.position;
//                float distanceToClosestFood = Mathf.Infinity; // max de�ere e�itliyoruz ki daha do�ru i�lem yapabilelim.
//                GameObject newClosestFood = null;
//                foreach (GameObject food in foodList)
//                {
//                    Vector3 foodPosition = food.transform.position;     // yeme�in pos
//                    float distanceToFood = Vector3.Distance(characterPosition, foodPosition);       // karakterle yemek aras�ndaki pos

//                    if (distanceToFood < distanceToClosestFood)     // en k���k de�erden k���kse
//                    {
//                        distanceToClosestFood = distanceToFood;     // en k���k de�er olsun
//                        newClosestFood = food;     // atama yapt�k
//                    }
//                }
//                closestFood = newClosestFood;
//            }
//            // Karakterin hedefini en yak�n yiyece�e ayarlay�p ve navMesh'i y�nlendiriyorum
//            if (closestFood != null)
//            {
//                Vector3 targetPosition = closestFood.transform.position;
//                navMeshAgent.destination = targetPosition;
//            }
//        }

//    }

//    // Belirtilen konumdan en uzaktaki yiyece�i bulan yard�mc� bir fonksiyon
//    Vector3 FindFurthestFood(Vector3 currentPosition, Vector3 direction)
//    {
//        float furthestDistance = -Mathf.Infinity; // en k���k de�ere e�itliyoruz ki daha do�ru i�lem yapabilelim.
//        Vector3 furthestFoodPosition = Vector3.zero;

//        foreach (GameObject food in foodList)
//        {
//            Vector3 foodPosition = food.transform.position;
//            Vector3 foodDirection = (foodPosition - currentPosition).normalized;
//            float dotProduct = Vector3.Dot(foodDirection, direction);

//            // Yemeklerin oyuncudan uzakla�acaklar� y�n�n dot product'i pozitif olmal�
//            // Yani oyuncuya tam z�t y�nde olmal�lar
//            if (dotProduct > 0 && dotProduct > furthestDistance)
//            {
//                furthestDistance = dotProduct;
//                furthestFoodPosition = foodPosition;
//            }
//        }

//        return furthestFoodPosition;
//    }



//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.CompareTag("Food_Big"))
//        {

//            Destroy(other.gameObject);

//            // foodlistten yeme�i ��akr�yoruz ki pacman yeni hedefe gitsin
//            foodList.Remove(other.gameObject);
//            closestFood = null;

//            Debug.Log("TestBig");
//        }

//        if (other.gameObject.CompareTag("Food_Small"))
//        {
//            Destroy(other.gameObject);
//            foodManagerScript.addSmallFoodScore();


//            // foodlistten yeme�i ��akr�yoruz ki pacman yeni hedefe gitsin
//            foodList.Remove(other.gameObject);
//            closestFood = null;

//            Debug.Log("TestSmall");
//        }
//    }
//}