using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FoodManager : MonoBehaviour
{
    public TextMeshProUGUI foodText;


    int foodCountSmall = 0;

    

    public void addSmallFoodScore()
    {
        foodCountSmall++;
        foodText.text = foodCountSmall + "/20";
    }

}
