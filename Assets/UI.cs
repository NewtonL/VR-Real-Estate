using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI : MonoBehaviour {
	
	bool showUI = true;

	// Use this for initialization
	void Start () {
		this.transform.GetChild (0).gameObject.SetActive (false);

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("escape")) {
			this.transform.GetChild (0).gameObject.SetActive (showUI);
			showUI = !showUI;
		}
	}

	public void Button1(){
		Vector3 v = new Vector3 (0, 0, 0);
		Quaternion q = new Quaternion (0, 0, 0, 0);
		Instantiate (Resources.Load("Wall"), v, q);
	}

	public void Button2(){
		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		foreach (GameObject wall in walls) {
			wall.GetComponent<Renderer> ().material.color = Color.red;
		}

	}
}
