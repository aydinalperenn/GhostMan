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


    public Transform player; // oyuncunun pozisyonunu takip etmek için kullanýlacak
    public float minDistanceToPlayer = 10f; // düþmanýn oyuncuya ne kadar yakýn olmasý gerektiði


    private void Awake()
    {
        adjacencyList = new Dictionary<GameObject, List<GameObject>>();

        // Tag'i "Food_Small" olan tüm yiyecekleri bulun
        foods = GameObject.FindGameObjectsWithTag("Food_Small");

        // Her yiyeceði adjacency listesine ekleyin ve komþu yiyecekleri bulun
        foreach (GameObject food in foods)
        {
            adjacencyList[food] = new List<GameObject>();

            // Belli bir mesafeden daha yakýn olan komþu yiyecekleri bulun
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
        // Oluþturulan adjacency listesini konsola yazdýrýn
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
            // Oyuncuya yaklaþtýkça hýzý artýr
            navMeshAgent.speed = 7f;
            //direction = (transform.position - player.transform.position).normalized;

            expandingColliderScript.isExpanding = true;
            
        }
        else
        {
            // Oyuncudan uzaklaþtýkça hýzý düþür
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
            // Yemekten komþularýn olduðu listeyi al
            List<GameObject> neighbors = adjacencyList[other.gameObject];

            // Yönüne yönlendir
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

            // Yemeði adjacency listesinden çýkar
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
        //    if (adjacencyList.Keys.Count != 0)      // komþusu yoksa ve oyun sahnesinde hala yemek varsa
        //    {   // deðiþtirdim, normalde findclosestfood

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

        // adjacencyList'in source kýsmýndaki yemekleri kontrol edin
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


        // closestFood null deðilse ve distance sonsuz deðilse
        if (closestFood != null && closestDistance != Mathf.Infinity)
        {
            return closestFood;
        }
        else
        {
            return GetRandomClosestFood(); // Rastgele bir yemek döndür
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
                distance = Math.Abs(Vector3.Distance(currentPosition, food.transform.position));    // mutlak deðer Abs
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
        // Yemekten komþularýn olduðu listeyi al
        List<GameObject> neighbors = adjacencyList[food];

        // Komþularý adjacencyList'ten çýkar
        foreach (var neighbor in neighbors)
        {
            adjacencyList[neighbor].Remove(food);
        }

        // Yemeði adjacencyList'ten çýkar
        adjacencyList.Remove(food);
    }

    private void PrintGraph()
    {
        foreach (var vertex in adjacencyList)
        {
            Debug.Log("Baðlantýlar " + vertex.Key.name + ": ");
            foreach (var neighbor in vertex.Value)
            {
                Debug.Log(neighbor.name);
            }
            Debug.Log("-----------------------");
        }
    }
}














//// Optimize yarým kod
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


//    public Transform player; // oyuncunun pozisyonunu takip etmek için kullanýlacak
//    public float minDistanceToPlayer = 10f; // düþmanýn oyuncuya ne kadar yakýn olmasý gerektiði


//    private void Awake()
//    {
//        adjacencyList = new Dictionary<GameObject, List<GameObject>>();

//        // Tag'i "Food_Small" olan tüm yiyecekleri bulun
//        foods = GameObject.FindGameObjectsWithTag("Food_Small");

//        // Her yiyeceði adjacency listesine ekleyin ve komþu yiyecekleri bulun
//        foreach (GameObject food in foods)
//        {
//            adjacencyList[food] = new List<GameObject>();

//            // Belli bir mesafeden daha yakýn olan komþu yiyecekleri bulun
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
//        // Oluþturulan adjacency listesini konsola yazdýrýn
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
//            // Burada oyuncuya zýt yönde kendine en yakýn yemeðe yönelecek
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
//            Debug.Log("Baðlantýlar " + vertex.Key.name + ": ");
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
//            // Yemekten komþularýn olduðu listeyi al
//            List<GameObject> neighbors = adjacencyList[other.gameObject];

//            // Yönüne yönlendir
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

//            // Yemeði adjacency listesinden çýkar
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
//        //    if (adjacencyList.Keys.Count != 0)      // komþusu yoksa ve oyun sahnesinde hala yemek varsa
//        //    {   // deðiþtirdim, normalde findclosestfood

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

//        // adjacencyList'in source kýsmýndaki yemekleri kontrol edin
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


//        // closestFood null deðilse ve distance sonsuz deðilse
//        if (closestFood != null && closestDistance != Mathf.Infinity)
//        {
//            return closestFood;
//        }
//        else
//        {
//            return GetRandomClosestFood(); // Rastgele bir yemek döndür
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
//                distance = Math.Abs(Vector3.Distance(currentPosition, food.transform.position));    // mutlak deðer Abs
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
//        // Yemekten komþularýn olduðu listeyi al
//        List<GameObject> neighbors = adjacencyList[food];

//        // Komþularý adjacencyList'ten çýkar
//        foreach (var neighbor in neighbors)
//        {
//            adjacencyList[neighbor].Remove(food);
//        }

//        // Yemeði adjacencyList'ten çýkar
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


//    private List<GameObject> foodList = new List<GameObject>();     // yemekleri içerisine koymak için bir gameobject listesi oluþturdum
//    GameObject closestFood;
//    public Transform player; // oyuncunun pozisyonunu takip etmek için kullanýlacak
//    public float minDistanceToPlayer = 10f; // düþmanýn oyuncuya ne kadar yakýn olmasý gerektiði


//    void Start()
//    {
//        navMeshAgent = GetComponent<NavMeshAgent>();
//        foodManagerScript = FindObjectOfType<FoodManager>().GetComponent<FoodManager>();


//        // Oyundaki tüm yiyeceklerin tag'ini kontrol edip foodList'e ekliyorum
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
//            // Karakterin ve tüm yiyeceklerin konumunu alýp karaktere en yakýn yiyeceði buluyoruz
//            if (closestFood == null)
//            {
//                Vector3 characterPosition = transform.position;
//                float distanceToClosestFood = Mathf.Infinity; // max deðere eþitliyoruz ki daha doðru iþlem yapabilelim.
//                GameObject newClosestFood = null;
//                foreach (GameObject food in foodList)
//                {
//                    Vector3 foodPosition = food.transform.position;     // yemeðin pos
//                    float distanceToFood = Vector3.Distance(characterPosition, foodPosition);       // karakterle yemek arasýndaki pos

//                    if (distanceToFood < distanceToClosestFood)     // en küçük deðerden küçükse
//                    {
//                        distanceToClosestFood = distanceToFood;     // en küçük deðer olsun
//                        newClosestFood = food;     // atama yaptýk
//                    }
//                }
//                closestFood = newClosestFood;
//            }
//            // Karakterin hedefini en yakýn yiyeceðe ayarlayýp ve navMesh'i yönlendiriyorum
//            if (closestFood != null)
//            {
//                Vector3 targetPosition = closestFood.transform.position;
//                navMeshAgent.destination = targetPosition;
//            }
//        }

//    }

//    // Belirtilen konumdan en uzaktaki yiyeceði bulan yardýmcý bir fonksiyon
//    Vector3 FindFurthestFood(Vector3 currentPosition, Vector3 direction)
//    {
//        float furthestDistance = -Mathf.Infinity; // en küçük deðere eþitliyoruz ki daha doðru iþlem yapabilelim.
//        Vector3 furthestFoodPosition = Vector3.zero;

//        foreach (GameObject food in foodList)
//        {
//            Vector3 foodPosition = food.transform.position;
//            Vector3 foodDirection = (foodPosition - currentPosition).normalized;
//            float dotProduct = Vector3.Dot(foodDirection, direction);

//            // Yemeklerin oyuncudan uzaklaþacaklarý yönün dot product'i pozitif olmalý
//            // Yani oyuncuya tam zýt yönde olmalýlar
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

//            // foodlistten yemeði çýakrýyoruz ki pacman yeni hedefe gitsin
//            foodList.Remove(other.gameObject);
//            closestFood = null;

//            Debug.Log("TestBig");
//        }

//        if (other.gameObject.CompareTag("Food_Small"))
//        {
//            Destroy(other.gameObject);
//            foodManagerScript.addSmallFoodScore();


//            // foodlistten yemeði çýakrýyoruz ki pacman yeni hedefe gitsin
//            foodList.Remove(other.gameObject);
//            closestFood = null;

//            Debug.Log("TestSmall");
//        }
//    }
//}