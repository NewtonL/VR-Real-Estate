using UnityEngine;
using System.Collections;

public class move : MonoBehaviour {
	Rigidbody rb;
	Transform cam;
	float speed = 0.15f;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		cam = Camera.main.transform;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
			Vector3 v = new Vector3 (cam.forward.x * speed, 0, cam.forward.z * speed);
			rb.MovePosition (transform.position + v);
		} 
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
			Vector3 v = new Vector3 (cam.right.x * speed, 0, cam.right.z * speed);
			rb.MovePosition (transform.position - v);
		} 
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
			Vector3 v = new Vector3 (cam.forward.x * speed, 0, cam.forward.z * speed);
			rb.MovePosition (transform.position - v);
		} 
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
			Vector3 v = new Vector3 (cam.right.x * speed, 0, cam.right.z * speed);
			rb.MovePosition (transform.position + v);
		}


	}


}