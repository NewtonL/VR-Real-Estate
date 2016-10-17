using UnityEngine;
using System.Collections;

public class ImageReader : MonoBehaviour {

	string path = "http://i.imgur.com/bOMSSlV.png";
	WWW www;

	// Use this for initialization
	void Start () {
		
		StartCoroutine (loadImage ());
		int x, y;
		for (x = 0; x < www.texture.width; x++) {
			for (y = 0; y < www.texture.height; y++) {
				Color32 c = www.texture.GetPixel (x, y);
				//print(c);
				//if colour is not white, place a block
				Color32 white = new Color(255,255,255,255);
				if((Color) c != (Color) white){
					//print ("I'm not  white");
					print(x+","+y);
					Vector3 v = new Vector3((x-30)*0.2f,0,(y-60)*0.2f);
					Quaternion q = new Quaternion (0, 0, 0, 0);
					Instantiate(Resources.Load("cube"), v, q);
				}

			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		

	}

	IEnumerator loadImage(){
		www = new WWW (path);
		while (www.isDone == false) {
			print ("Still downloading\n");
		}
		yield return www;
	}

}


