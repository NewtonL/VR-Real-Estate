using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IgnoreMouse : MonoBehaviour {

	GameObject selected;

	// Use this for initialization
	void Start () {
		selected = EventSystem.current.currentSelectedGameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if (EventSystem.current.currentSelectedGameObject == null) {
			EventSystem.current.SetSelectedGameObject (selected);
		}
		selected = EventSystem.current.currentSelectedGameObject;
	}
}
