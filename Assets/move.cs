using UnityEngine;
using System.Collections;

public class move : MonoBehaviour {
	Rigidbody rb;
	Transform cam;
	int moving;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		cam = Camera.main.transform;
		moving = 0;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
			Vector3 v = new Vector3 (cam.forward.x * 0.1f, 0, cam.forward.z * 0.1f);
			rb.MovePosition (transform.position + v);
		} 
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
			Vector3 v = new Vector3 (cam.right.x * 0.1f, 0, cam.right.z * 0.1f);
			rb.MovePosition (transform.position - v);
		} 
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
			Vector3 v = new Vector3 (cam.forward.x * 0.1f, 0, cam.forward.z * 0.1f);
			rb.MovePosition (transform.position - v);
		} 
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
			Vector3 v = new Vector3 (cam.right.x * 0.1f, 0, cam.right.z * 0.1f);
			rb.MovePosition (transform.position + v);
		}


	}


}