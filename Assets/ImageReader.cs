using UnityEngine;
using System.Collections;

public class ImageReader : MonoBehaviour {

	string path = "http://i.imgur.com/CMRCsV1.png";
	WWW www;
	Color32 white = new Color(255,255,255,255);
	Color32 black = new Color(0,0,0,255);
	Color32[] pixels;

	// Use this for initialization
	void Start () {
		
		StartCoroutine (loadImage ());
		int x, y;
		//pixels = www.texture.GetPixels32();
		Build ();


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

	void Build(){
		int x, y;
		for (x = 0; x < www.texture.width; x+=5) {
			for (y = 0; y < www.texture.height; y+=5) {
				Color32 c1 = www.texture.GetPixel (x, y);
				//Color32 c2 = www.texture.GetPixel (x+1, y+1);
				Color32 c3 = www.texture.GetPixel (x+2, y+2);
				//Color32 c4 = www.texture.GetPixel (x+3, y+3);
				Color32 c5 = www.texture.GetPixel (x+4, y+4);

				//if color is black, place a wall
				if ((Color)c1 == (Color)black) {
					Vector3 v = new Vector3 ((x - 40) * 0.2f, 0, (y - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Wall"), v, q);
				} 
				//else if color is grey, place a window
				else if (c1.r < white.r && c1.r > black.r) {
					Vector3 v = new Vector3 ((x - 40) * 0.2f, 0, (y - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Window"), v, q);
				}
				/*
				//if color is black, place a wall
				if ((Color)c2 == (Color)black) {
					Vector3 v = new Vector3 ((x+1 - 40) * 0.2f, 0, (y+1 - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Wall"), v, q);
				} 
				//else if color is grey, place a window
				else if (c2.r < white.r && c2.r > black.r) {
					Vector3 v = new Vector3 ((x+1 - 40) * 0.2f, 0, (y+1 - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Window"), v, q);
				}
				*/
				//if color is black, place a wall
				if ((Color)c3 == (Color)black) {
					Vector3 v = new Vector3 ((x+2 - 40) * 0.2f, 0, (y+2 - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Wall"), v, q);
				} 
				//else if color is grey, place a window
				else if (c3.r < white.r && c3.r > black.r) {
					Vector3 v = new Vector3 ((x+2 - 40) * 0.2f, 0, (y+2 - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Window"), v, q);
				}
				/*
				//if color is black, place a wall
				if ((Color)c4 == (Color)black) {
					Vector3 v = new Vector3 ((x+3 - 40) * 0.2f, 0, (y+3 - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Wall"), v, q);
				}

				//else if color is grey, place a window
				else if (c4.r < white.r && c4.r > black.r) {
					Vector3 v = new Vector3 ((x+3 - 40) * 0.2f, 0, (y+3 - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Window"), v, q);
				}
				*/
				//if color is black, place a wall
				if ((Color)c5 == (Color)black) {
					Vector3 v = new Vector3 ((x+4 - 40) * 0.2f, 0, (y+4 - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Wall"), v, q);
				} 
				//else if color is grey, place a window
				else if (c5.r < white.r && c5.r > black.r) {
					Vector3 v = new Vector3 ((x+4 - 40) * 0.2f, 0, (y+4 - 70) * 0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate (Resources.Load ("Window"), v, q);
				}

			}
		}

	}

}


