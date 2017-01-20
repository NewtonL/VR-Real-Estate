using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UI : MonoBehaviour {
	
	bool showUI = true;
	GameObject newObj;
	bool placing = false;

	// Use this for initialization
	void Start () {
		if(SceneManager.GetActiveScene().name == "gearVR")
			this.transform.GetChild (0).gameObject.SetActive (false);


	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown ("escape")) {	//use ESC key to hide or show menu
			this.transform.GetChild (0).gameObject.SetActive (showUI);
			showUI = !showUI;
		}

		if (placing) {
			newObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10;
		}
	}

	public void Button1(){
		if (placing == false) {
			Vector3 v = Camera.main.transform.position + Camera.main.transform.forward * 10;	//position in front of the camera's view
			Quaternion q = new Quaternion (0, 0, 0, 0);
			newObj = (GameObject)Instantiate (Resources.Load ("Wall"), v, q);
			placing = true;
		} else if (placing == true) {
			placing = false;
		}
	}

	public void Button2(){
		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		foreach (GameObject wall in walls) {
			wall.GetComponent<Renderer> ().material.color = Color.red;
		}

	}

	public void StartButton(){
		SceneManager.LoadScene ("gearVR");
	}
}
