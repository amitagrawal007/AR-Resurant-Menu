using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodQtyScript : MonoBehaviour {

    public Text valText;
    public DatabaseHandler databaseHandler;
    public int foodItemId;
    public Dropdown typeChoice;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void incVal()
    {
        int prVal = int.Parse(valText.text);
        prVal+=1;
        valText.text = prVal.ToString();
    }

    public void decVal()
    {
        int prVal = int.Parse(valText.text);
        if(prVal!=0)
            prVal-=1;
        valText.text = prVal.ToString();
    }

    public void updateOrder()
    {
        databaseHandler.handleOrder(foodItemId.ToString() + ":" +typeChoice.value.ToString()+":"+valText.text);
    }
}
