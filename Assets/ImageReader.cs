using UnityEngine;
using System.Collections;

public class ImageReader : MonoBehaviour {

	string path = "http://i.imgur.com/WxrwOla.png";
	WWW www;

	// Use this for initialization
	void Start () {
		
		StartCoroutine (loadImage ());
		int x, y;
		//GetComponent<Renderer> ().material.mainTexture = tex;
		for (x = 0; x < www.texture.width; x++) {
			for (y = 0; y < www.texture.height; y++) {
				Color32 c = www.texture.GetPixel (x, y);
				print(c);
			}
		}

		//tex.Apply ();



	}
	
	// Update is called once per frame
	void Update () {
		

	}

	IEnumerator loadImage(){
		www = new WWW (path);
		yield return www; //wait for download to finish
	}

}


