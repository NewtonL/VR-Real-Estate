using UnityEngine;
using System.Collections;

public class ImageReader : MonoBehaviour {

	Texture2D tex;
	string path = "http://i.imgur.com/xK1NKqJ.png";
	WWW www;

	// Use this for initialization
	void Start () {
		print ("IN START\n");
		StartCoroutine (loadImage ());
		int x, y;
		GetComponent<Renderer> ().material.mainTexture = tex;
		for (x = 0; x < 50; x++) {
			for (y = 0; y < 50; y++) {
				print(tex.GetPixel (x, y));
			}
		}

		tex.Apply ();

	}
	
	// Update is called once per frame
	void Update () {
		

	}

	IEnumerator loadImage(){
		print ("HERE\n");
		tex = new Texture2D (50, 50);

		www = new WWW (path);
		yield return www; //wait for download to finish
		www.LoadImageIntoTexture (tex);
	}

}


