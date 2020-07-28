using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerScript : MonoBehaviour {

	public Transform[] foodModels;
	public bool toShowFood = false;
    public bool orderPlaced = false;

	public void Quit(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#elif UNITY_WEBPLAYER
		Application.OpenURL(webplayerQuitURL);
		#else
		Application.Quit();
		#endif
	}

	public void toggleToShowModel(bool state){
		toShowFood = state;
	}

	public void ViewModel(int i){
		foreach (Transform t in foodModels) {
			t.gameObject.SetActive (false);
		}
		foodModels [i].gameObject.SetActive (true);
	}

	public void disableAllModels(){
		foreach (Transform t in foodModels) {
			t.gameObject.SetActive (false);
		}
	}

    public void toggleOrderPlaced(bool op)
    {
        orderPlaced = op;
    }
}
