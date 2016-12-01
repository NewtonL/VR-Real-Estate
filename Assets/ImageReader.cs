using UnityEngine;
using System.Collections;

public class ImageReader : MonoBehaviour {

	string path = "http://www.colebrook.net/wp-content/uploads/2012/08/RG-3-bdrm-with-balcony.jpg";
	WWW www;
	Color32 white = new Color32(255,255,255,1);
	Color32 black = new Color32(10,10,10,1);
	Color32[] pixels;
	Texture2D scaledDown;

	// Use this for initialization
	void Start () {
		
		StartCoroutine ("loadImage");

		//scale down the image, larger images will be scaled down more
		if (www.texture.width >= 500) {
			int scale = www.texture.width / 300;
			int newWidth = 300;
			int newHeight = www.texture.height / scale;
			scaledDown = Instantiate (www.texture);
			TextureScale.Bilinear (scaledDown, newWidth, newHeight);
		}
		else
			scaledDown = Instantiate (www.texture);


		StartCoroutine ("Build");


	}
	
	// Update is called once per frame
	void Update () {
	}

	IEnumerator loadImage(){
		www = new WWW (path);
		while (www.isDone == false) {
			//wait for image to finish downloading
		}

		yield return www;
	}

	IEnumerator Build(){
		int x, y;
		int width = scaledDown.width;
		int height = scaledDown.height;
		int ratio = width / height;
		int x_incr = width / (200*ratio);
		int y_incr = height / 200;
		Quaternion q = new Quaternion (0, 0, 0, 0);
		for (x = 0; x < width; x+=x_incr) {
			for (y = 0; y < height; y+=y_incr) {
				Vector3 v = new Vector3 ((x - 170) * 0.2f, 0, (y - 150) * 0.2f);
				Color32 c1 = scaledDown.GetPixel (x, y);
				Color32 c3 = scaledDown.GetPixel (x+(x_incr/2), y+(y_incr/2));


				//if color is black, place a wall
				if (Mathf.Abs(c1.r - black.r) <= 10) {
					Instantiate (Resources.Load ("Wall"), v, q);
				} 

				//if color is black, place a wall
				if (Mathf.Abs(c3.r - black.r) <= 10) {
					Instantiate (Resources.Load ("Wall"), v, q);
				} 


			}
			yield return null;
		}

	}

}


