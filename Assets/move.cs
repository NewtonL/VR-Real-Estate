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
		if (Input.GetButtonDown ("Fire1")) {
			moving = 1;
		} else if (Input.GetButtonUp ("Fire1")) {
			moving = 0;
		}

		if (moving == 1) {
			Vector3 v = new Vector3 (cam.forward.x * 0.1f, 0, cam.forward.z * 0.1f);
			rb.MovePosition (transform.position + v);
		}

	}


}