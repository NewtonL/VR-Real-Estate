using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IgnoreMouse : MonoBehaviour {

	static GameObject selected;
	static bool alreadyClicked = false;
	// Use this for initialization
	void Start () {
		selected = EventSystem.current.currentSelectedGameObject;
		OVRTouchpad.Create ();
		OVRTouchpad.TouchHandler += HandleTouchHandler;
	}

	void HandleTouchHandler(object sender, System.EventArgs e){
		OVRTouchpad.TouchArgs touch = (OVRTouchpad.TouchArgs)e;
		if (selected.GetComponent<Button> ()) {
			Button b = selected.GetComponent<Button> ();
			if (touch.TouchType == OVRTouchpad.TouchEvent.Right) {
				EventSystem.current.SetSelectedGameObject (b.FindSelectableOnLeft ().gameObject);
				alreadyClicked = false;
			}
			if (touch.TouchType == OVRTouchpad.TouchEvent.Left) {
				EventSystem.current.SetSelectedGameObject (b.FindSelectableOnRight ().gameObject);
				alreadyClicked = false;
			}
			if (touch.TouchType == OVRTouchpad.TouchEvent.Up) {
				EventSystem.current.SetSelectedGameObject (b.FindSelectableOnUp ().gameObject);
				alreadyClicked = false;
			}
			if (touch.TouchType == OVRTouchpad.TouchEvent.Down) {
				EventSystem.current.SetSelectedGameObject (b.FindSelectableOnDown ().gameObject);
				alreadyClicked = false;
			}
			if (touch.TouchType == OVRTouchpad.TouchEvent.SingleTap) {
				//There is currently a bug with Oculus Utilities 1.11 where click events are executed twice (same with slider)
				//https://forums.oculus.com/developer/discussion/47588/click-events-fired-twice-on-gear-vr
				//Apparently will be fixed in 1.12 soon...
				if (!alreadyClicked) {
					b.onClick.Invoke ();
					alreadyClicked = true;
				}
			}
		} else if (selected.GetComponent<Slider> ()) {
			Slider s = selected.GetComponent<Slider> ();
			if (touch.TouchType == OVRTouchpad.TouchEvent.Right) {
				s.value -= 15;
			}
			if (touch.TouchType == OVRTouchpad.TouchEvent.Left) {
				s.value += 15;
			}
			if (touch.TouchType == OVRTouchpad.TouchEvent.Up) {
				EventSystem.current.SetSelectedGameObject (s.FindSelectableOnUp ().gameObject);
			}
			if (touch.TouchType == OVRTouchpad.TouchEvent.Down) {
				EventSystem.current.SetSelectedGameObject (s.FindSelectableOnDown ().gameObject);
			}
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
