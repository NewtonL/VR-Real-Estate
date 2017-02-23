using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class UI : MonoBehaviour {
	
	bool showUI = true;
	GameObject newObj;
	bool placing = false;
	float red, green, blue;
	GameObject[] walls;
	GameObject lightObject;
	float distanceFromGround = 0;

	// Use this for initialization
	void Start () {
		if(SceneManager.GetActiveScene().name == "gearVR")
			this.transform.GetChild (0).gameObject.SetActive (false);


	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown ("m")) {	//use ESC key to hide or show menu
			this.transform.GetChild (0).gameObject.SetActive (showUI);
			if(showUI)
				EventSystem.current.SetSelectedGameObject (GameObject.FindGameObjectWithTag("Menu"));
			showUI = !showUI;
		}

		if (placing) {
			

			newObj.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y-distanceFromGround, Camera.main.transform.position.z) + Camera.main.transform.forward * 10;//Camera.main.transform.position - new Vector3(0, distanceFromGround, 0) + Camera.main.transform.forward * 10;
		}
	}

	public void AddWall(){
		showUI = !showUI;
		EventSystem.current.SetSelectedGameObject (GameObject.Find("Rotate"));

		if (placing == false) {
			Vector3 v = Camera.main.transform.position + Camera.main.transform.forward * 10;	//position in front of the camera's view
			newObj = (GameObject)Instantiate (Resources.Load ("sofa2"), v, Quaternion.Euler(0,270,0));

			RaycastHit hitInfo;
			Physics.Raycast (newObj.transform.position, -Vector3.up, out hitInfo);
			distanceFromGround = hitInfo.distance;


			placing = true;
		} else if (placing == true) {
			showUI = !showUI;
			placing = false;
		}
	}

	public void RotateObject(){
		newObj.transform.Rotate (0f, 45f, 0f);
	}

	public void ChangeObject(string s){
		placing = false;
		Destroy (newObj);
		if (s.Equals("Desk")) {
			Vector3 v = Camera.main.transform.position + Camera.main.transform.forward * 10;	//position in front of the camera's view
			newObj = (GameObject)Instantiate (Resources.Load (s), v, Quaternion.Euler (0, 270, 0));

			RaycastHit hitInfo;
			Physics.Raycast (newObj.transform.position, -Vector3.up, out hitInfo);
			distanceFromGround = hitInfo.distance;

			placing = true;
		} else if (s.Equals("sofa")) {
			Vector3 v = Camera.main.transform.position + Camera.main.transform.forward * 10;	//position in front of the camera's view
			newObj = (GameObject)Instantiate (Resources.Load (s), v, Quaternion.Euler (0, 270, 0));

			RaycastHit hitInfo;
			Physics.Raycast (newObj.transform.position, -Vector3.up, out hitInfo);
			distanceFromGround = hitInfo.distance;

			placing = true;
		}
		else if (s.Equals("sofa2")) {
			Vector3 v = Camera.main.transform.position + Camera.main.transform.forward * 10;	//position in front of the camera's view
			newObj = (GameObject)Instantiate (Resources.Load (s), v, Quaternion.Euler (0, 270, 0));

			RaycastHit hitInfo;
			Physics.Raycast (newObj.transform.position, -Vector3.up, out hitInfo);
			distanceFromGround = hitInfo.distance;

			placing = true;
		}
		else if (s.Equals("Chair")) {
			Vector3 v = Camera.main.transform.position + Camera.main.transform.forward * 10;	//position in front of the camera's view
			newObj = (GameObject)Instantiate (Resources.Load (s), v, Quaternion.Euler (0, 270, 0));

			RaycastHit hitInfo;
			Physics.Raycast (newObj.transform.position, -Vector3.up, out hitInfo);
			distanceFromGround = hitInfo.distance;

			placing = true;
		}
	}

	public void ChangeWallColour(){
		showUI = !showUI;
		EventSystem.current.SetSelectedGameObject (GameObject.FindGameObjectWithTag("Slider"));
		walls = GameObject.FindGameObjectsWithTag ("Wall");
	}

	public void ChangeLighting(){
		showUI = !showUI;
		EventSystem.current.SetSelectedGameObject (GameObject.FindGameObjectWithTag("Slider"));
		lightObject = GameObject.FindGameObjectWithTag ("Light");
	}


	public void Back(){
		SceneManager.LoadScene ("Welcome");
	}

	public void StartButton(){
		SceneManager.LoadScene ("gearVR");
	}

	public void GetSliderValue(Slider s){
		Color c1, c2;
		if (s.value <= 100f) {
			c1 = Color.red;
			c2 = Color.blue;
		}
		else if (s.value <= 200f) {
			c1 = Color.blue;
			c2 = Color.green;
		}
		else {
			c1 = Color.green;
			c2 = Color.red;
		}
		foreach (GameObject wall in walls) {
			wall.GetComponent<Renderer> ().material.color = Color.Lerp(c1, c2, s.value/300f);
		}
	}

	public void GetLightSliderValue(Slider s){
		lightObject.GetComponent<Light>().intensity = s.value/10f;
	}


}
