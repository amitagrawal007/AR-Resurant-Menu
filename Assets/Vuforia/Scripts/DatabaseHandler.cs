using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public class Variant
{
    public string name;
    public int price;
}

[Serializable]
public class Food
{
    public string name;
    public Variant[] var;
}

[Serializable]
public class OrderItem
{
    public Food food;
    public Variant variant;
    public int qty;
}

public class DatabaseHandler : MonoBehaviour
{

    public float guiListItemOffset;

    public GameObject foodMenuHolder;
    public Text loaderText;
    public GameObject sampleListItem;
    public bool toUpdateList;
    public string[] foodnames;
    public Food[] foods;
    public GameObject UIManagerScript;
    public List<OrderItem> orders;
    public GameObject PlaceOrderpage;
    public GameObject OrderPlacedMsg;

    void Start()
    {

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp, i.e.
                //   app = Firebase.FirebaseApp.DefaultInstance;
                // where app is a Firebase.FirebaseApp property of your application class.

                // Set a flag here indicating that Firebase is ready to use by your
                // application.

                // Set up the Editor before calling into the realtime database.
                FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://form-651aa.firebaseio.com/");

                // Get the root reference location of the database.
                DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

                orders = new List<OrderItem>();
                toUpdateList = false;
                guiListItemOffset = 510f;
                getFoods();
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

    }

    void Update()
    {
        if (toUpdateList)
        {
            int i = 1;
            foreach(Food f in foods)
            {
                Debug.Log("from the list: " + f.name);
                createListItem(f,i);
                i++;
            }
            toUpdateList = false;
            loaderText.gameObject.SetActive(false);
        }
    }

    public void getFoods()
    {
        
        FirebaseDatabase.DefaultInstance.GetReference("foods/12345").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                if (loaderText)
                    loaderText.text = task.Exception.Message;
                else
                    Debug.LogError(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // Do something with snapshot...
                Debug.Log(snapshot.GetRawJsonValue());
                int i = 0;
                foodnames = new string[snapshot.ChildrenCount];
                foods = new Food[snapshot.ChildrenCount];
                foreach (DataSnapshot ds in snapshot.Children)
                {
                    Debug.Log(ds.GetRawJsonValue());
                    foods[i] = JsonUtility.FromJson<Food>(ds.GetRawJsonValue());
                    Debug.Log(foods[i].name);
                    foodnames[i] = foods[i].name;
                    Debug.Log(foods[i].var);
                    i++;
                }
                toUpdateList = true;
                
            }
        });
        
    }

    private void createListItem(Food f, int i)
    {

        //Instantiate the list item
        GameObject newListItem = Instantiate(sampleListItem) as GameObject;
        newListItem.transform.SetParent(foodMenuHolder.transform, false);
        Vector3 pos = sampleListItem.transform.localPosition;
        pos.y -= i * guiListItemOffset;
        newListItem.GetComponent<RectTransform>().localPosition = pos;

        //Setup attributes
        //View button
        Button viewButton = newListItem.gameObject.transform.Find("ViewBtn").GetComponent<Button>();
        viewButton.GetComponentInChildren<Text>().text = f.name;
        UIManagerScript uiManagerScript = UIManagerScript.GetComponent<UIManagerScript>();
        Debug.Log("Food list length: " + uiManagerScript.foodModels.Length);
        newListItem.gameObject.transform.Find("ViewBtn").GetComponent<Button>().onClick.AddListener(delegate { uiManagerScript.ViewModel(i - 1); });

        //set up spinner
        Debug.Log(f.name + " : " + f.var.Length);
        if (f.var != null) {
            if (f.var.Length > 1) {
                foreach (Variant v in f.var) {
                    newListItem.gameObject.transform.Find("VariantChooser").GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(v.name));
                }
            } else
            {
                newListItem.gameObject.transform.Find("VariantChooser").gameObject.SetActive(false);
            }
        }
        else
        {
            newListItem.gameObject.transform.Find("VariantChooser").gameObject.SetActive(false);
        }

        //setup price
        newListItem.gameObject.transform.Find("PriceTxt").GetComponent<Text>().text = "₹" + f.var[0].price.ToString()+".00";

        //setup script
        newListItem.GetComponent<FoodQtyScript>().databaseHandler = this;
        newListItem.GetComponent<FoodQtyScript>().foodItemId = i - 1;
        newListItem.GetComponent<FoodQtyScript>().typeChoice = newListItem.gameObject.transform.Find("VariantChooser").GetComponent<Dropdown>();
        newListItem.GetComponent<FoodQtyScript>().valText = newListItem.gameObject.transform.Find("QtyPanel").Find("qtyTxt").GetComponent<Text>();
        

        newListItem.SetActive(true);
    }



    public void handleOrder(string orderString)
    {
        Debug.Log("recieved order: " + orderString);
        Food f = foods[int.Parse(orderString.Split(':')[0])];
        Variant v = f.var[int.Parse(orderString.Split(':')[1])];
        int qty = int.Parse(orderString.Split(':')[2]);

        OrderItem oi = new OrderItem();
        oi.food = f;
        oi.qty = qty;
        oi.variant = v;

        Debug.Log(JsonUtility.ToJson(oi));

        if (qty != 0)
        {
            bool found = false;

            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i].food == oi.food && orders[i].variant == oi.variant)
                {
                    orders.RemoveAt(i);
                    orders.Add(oi);
                    found = true;
                    break;
                }
            }

            if (!found)
                orders.Add(oi);
        }else
        {
            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i].food == oi.food && orders[i].variant == oi.variant)
                {
                    orders.RemoveAt(i);
                    break;
                }
            }
        }


        Debug.Log("cart size: " + orders.Count);

        
    }

    public void uploadOrder(string id)
    {
        Debug.Log(id);
        if (!id.Contains("table"))
            return;

        Debug.Log(orders);
        Debug.Log(orders.ToArray());
        string json = JsonUtility.ToJson(orders.ToArray());
        Debug.Log(json);

        int idno = int.Parse(id.Split('_')[1]);

        FirebaseDatabase.DefaultInstance.GetReference("orders/" + idno.ToString()).SetRawJsonValueAsync(json);

        PlaceOrderpage.gameObject.SetActive(false);
        OrderPlacedMsg.gameObject.SetActive(true);
    }

        //public void varChanged(int index)
        //{
        //    Debug.Log(index);
        //}
}