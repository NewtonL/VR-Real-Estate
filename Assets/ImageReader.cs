using UnityEngine;
using System.Collections;

public class ImageReader : MonoBehaviour {

	string path = "http://www.roomsketcher.com/wp-content/uploads/2014/08/RoomSketcher-2D-Floor-Plan-1.jpg";
	WWW www;
	Color32 white = new Color32(255,255,255,1);
	Color32 black = new Color32(10,10,10,1);
	Color32[] pixels;
	Texture2D scaledDown;

	struct Wall
	{
		public int x;
		public int startY;
		public int endY;
	};  

	Wall[] wallList = new Wall[1000];
	Wall[] windowList = new Wall[10000];

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
		int wallIndex = 0;
		int windowIndex = 0;
		int x, y;
		int width = scaledDown.width;
		int height = scaledDown.height;
		int ratio = width / height;
		int x_incr = width / (200*ratio);
		int y_incr = height / 200;
		int top=0, bottom=0, left=0, right=0; //4 outermost edges of the property

		Quaternion q = new Quaternion (0, 0, 0, 0);
		for (x = 0; x < width; x+=x_incr) {
			Wall newWall = new Wall();
			newWall.x = 0;
			newWall.startY = 0;

			Wall newWindow = new Wall ();
			newWindow.x = 0;
			newWindow.startY = 0;

			for (y = 0; y < height; y+=y_incr) {
				Color32 c1 = scaledDown.GetPixel (x, y);

				//if color is black, we found a wall
				if (Mathf.Abs (c1.r - black.r) <= 10) {
					if(left == 0){
						left = x;	//leftmost edge of the property, only set once
					}
					if (top == 0) {
						top = y;
					}
					right = x;		//rightmost edge, update every time, last value will be rightmost
					bottom = y;

					if (newWall.startY == 0) {
						newWall.x = x;
						newWall.startY = y;
					}
				} else if (Mathf.Abs (c1.r - black.r) > 10 && Mathf.Abs (c1.r - black.r) <= 200) {
					if (newWindow.startY == 0) {
						newWindow.x = x;
						newWindow.startY = y;
					}
				}
					
				//we have reached the endpoint of a wall, add it to the array wallList
				if(Mathf.Abs (c1.r - black.r) > 10){
					if (newWall.startY != 0) {
						newWall.x = x;
						newWall.endY = y;

						wallList [wallIndex] = newWall;
						wallIndex++;

						newWall.x = 0;
						newWall.startY = 0;
					} else if (Mathf.Abs (c1.r - black.r) > 200 && newWindow.startY != 0) {
						newWindow.x = x;
						newWindow.endY = y;

						windowList [windowIndex] = newWindow;
						windowIndex++;

						newWindow.x = 0;
						newWindow.startY = 0;
					}
				}
					
			}
			yield return null;
		}


		//Place all the walls inside the scene by instantiating a wall for each wall in wallList
		//One object is made for each wall, the length is adjusted based on the start and endpoints
		for (int i = 0; i < 1000; i++) {
			if (wallList [i].startY != 0) {
				int length = wallList [i].endY - wallList [i].startY;
				int center = wallList[i].startY + length/ 2;
				Vector3 v = new Vector3 ((wallList [i].x - 170) * 0.2f, 0, (center - 150) * 0.2f);
				GameObject newWall = (GameObject)Resources.Load ("Wall");
				float lengthScale = Mathf.Max (1, length / 5);
				newWall.transform.localScale = new Vector3 (1f, 5f, lengthScale);
				Instantiate (newWall, v, q);
			}

			if (windowList [i].startY != 0) {
				bool rightEdge = Mathf.Abs (windowList [i].x - right) < 3;
				bool leftEdge = Mathf.Abs (windowList [i].x - left) < 3;
				bool topEdge = (Mathf.Abs (windowList [i].endY - top) < 3) && (windowList[i].endY - windowList[i].startY)<3;
				bool bottomEdge = (Mathf.Abs (windowList [i].endY - bottom) < 3) && (windowList[i].endY - windowList[i].startY)<3;
				if (rightEdge || leftEdge || topEdge || bottomEdge) {
					int length = windowList [i].endY - windowList [i].startY;
					int center = windowList [i].startY + length / 2;
					Vector3 v = new Vector3 ((windowList [i].x - 170) * 0.2f, 0, (center - 150) * 0.2f);
					GameObject newWindow = (GameObject)Resources.Load ("Window");
					float lengthScale = Mathf.Max (1, length / 5);
					newWindow.transform.localScale = new Vector3 (1f, 5f, lengthScale);
					Instantiate (newWindow, v, q);
				}
			}
		}
		print ("top is: " + top + " bottom is: " + bottom + " right is: " + right + " left is: " + left);
	}

}


