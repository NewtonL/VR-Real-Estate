using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IgnoreMouse : MonoBehaviour {

	GameObject selected;

	// Use this for initialization
	void Start () {
		selected = EventSystem.current.currentSelectedGameObject;
		OVRTouchpad.Create ();
		OVRTouchpad.TouchHandler += HandleTouchHandler;
	}

	void HandleTouchHandler(object sender, System.EventArgs e){
		Button b = selected.GetComponent<Button> ();
		OVRTouchpad.TouchArgs touch = (OVRTouchpad.TouchArgs)e;
		if (touch.TouchType == OVRTouchpad.TouchEvent.Left) {
			EventSystem.current.SetSelectedGameObject (b.FindSelectableOnLeft().gameObject);
			print ("Left");
		}
		if (touch.TouchType == OVRTouchpad.TouchEvent.Right) {
			EventSystem.current.SetSelectedGameObject (b.FindSelectableOnRight().gameObject);
			print ("Right");
		}
		if (touch.TouchType == OVRTouchpad.TouchEvent.Up) {
			EventSystem.current.SetSelectedGameObject (b.FindSelectableOnUp().gameObject);
			print ("Up");
		}
		if (touch.TouchType == OVRTouchpad.TouchEvent.Down) {
			EventSystem.current.SetSelectedGameObject (b.FindSelectableOnDown().gameObject);
			print ("Down");
		}
		if (touch.TouchType == OVRTouchpad.TouchEvent.SingleTap) {
			b.onClick.Invoke ();
			print ("Tap");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (EventSystem.current.currentSelectedGameObject == null) {
			EventSystem.current.SetSelectedGameObject (selected);
		}
		selected = EventSystem.current.currentSelectedGameObject;
	}
}
