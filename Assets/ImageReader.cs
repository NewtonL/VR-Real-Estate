using UnityEngine;
using System.Collections;

public class ImageReader : MonoBehaviour {

	//http://www.colebrook.net/wp-content/uploads/2012/08/RG-3-bdrm-with-balcony.jpg
	//http://www.roomsketcher.com/wp-content/uploads/2014/08/RoomSketcher-2D-Floor-Plan-1.jpg
	string path = "http://www.roomsketcher.com/wp-content/uploads/2015/11/RoomSketcher-House-Floor-Plans-962270.jpg";
	WWW www;
	Color32 white = new Color32(255,255,255,1);
	Color32 black = new Color32(10,10,10,1);
	Color32[] pixels;
	Texture2D scaledDown;
	int wallIndex = 0;

	struct Wall
	{
		public int x;
		public int startY;
		public int endY;
		public int type; //0 = solid wall, 1 = window
	};  
		
	Wall[] wallList = new Wall[10000];


	// Use this for initialization
	void Start () {

		StartCoroutine ("loadImage");

		//scale down the image, larger images will be scaled down more
		if (www.texture.width >= 500) {
			float scale = www.texture.width / 300f;
			int newWidth = 300;
			float newHeight = www.texture.height / scale;

			//print (" Width is: " + www.texture.width + ", Height is: " + www.texture.height + ", scale is: " + scale + ", newWidth is: " + newWidth + ", newHeight is: " + (int) newHeight);

			scaledDown = Instantiate (www.texture);
			TextureScale.Bilinear (scaledDown, newWidth, (int) newHeight);
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
		int y_incr = Mathf.Max(1,height / 200);
		int top=0, bottom=0, left=0, right=0; //4 outermost edges of the property

		Quaternion q = new Quaternion (0, 0, 0, 0);
		for (x = 0; x < width; x+=x_incr) {
			Wall newWall = new Wall();
			newWall.x = 0;
			newWall.startY = 0;
			newWall.type = 0;

			Wall newWindow = new Wall ();
			newWindow.x = 0;
			newWindow.startY = 0;
			newWindow.type = 1;

			for (y = 0; y < height; y+=y_incr) {
				Color32 c1 = scaledDown.GetPixel (x, y);

				//if color is black, we found a wall
				if (Mathf.Abs (c1.r - black.r) <= 20) {
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

					//if we found a wall, need to close off the window
					if (newWindow.startY != 0) {
						newWindow.x = x;
						newWindow.endY = y;

						wallList [wallIndex] = newWindow;
						wallIndex++;

						newWindow.x = 0;
						newWindow.startY = 0;
					}

					if (y == height - 1) {	//reached the edge of the image, close off the wall
						newWall.x = x;
						newWall.endY = y;

						wallList [wallIndex] = newWall;
						wallIndex++;
					}
				//found a window
				} else if (Mathf.Abs (c1.r - black.r) > 20 && Mathf.Abs (c1.r - black.r) <= 200) {
					if (newWindow.startY == 0) {
						newWindow.x = x;
						newWindow.startY = y;
					}

					if (y == height - 1) {	//reached the edge of the image, close off the window
						newWindow.x = x;
						newWindow.endY = y;

						wallList [wallIndex] = newWindow;
						wallIndex++;
					}
				}
					
				//we have reached the endpoint of a wall, add it to the array wallList
				if(Mathf.Abs (c1.r - black.r) > 20){
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

						wallList [wallIndex] = newWindow;
						wallIndex++;

						newWindow.x = 0;
						newWindow.startY = 0;
					}
				}
					
			}
			yield return null;
		}


		//print ("Right is " + right + ", Left is " + left + ", Top is " + top + ", Bottom is " + bottom);

		//Place all the walls inside the scene by instantiating a wall for each wall in wallList
		//One object is made for each wall, the length is adjusted based on the start and endpoints
		for (int i = 0; i < 10000; i++) {
			if (wallList[i].startY != 0) {
				int length = wallList [i].endY - wallList [i].startY;
				int center = wallList [i].startY + length / 2;
				Vector3 v = new Vector3 ((wallList [i].x - 170) * 0.2f, 2f, (center - 150) * 0.2f);
				float lengthScale = Mathf.Max (1, length / 5);
				if (wallList [i].type == 0) {

					print("Wall x = " + wallList[i].x + ", startY = " + wallList[i].startY + ", endY = " + wallList[i].endY);

					GameObject newWall = (GameObject)Resources.Load ("Wall");
					newWall.transform.localScale = new Vector3 (1f, 5f, lengthScale);

					newWall.gameObject.tag = "Wall";

					Instantiate (newWall, v, q);

				} else if (wallList [i].type == 1) {

					bool rightEdge = Mathf.Abs (wallList [i].x - right) < 3;
					bool leftEdge = Mathf.Abs (wallList [i].x - left) < 3;
					bool topEdge = (Mathf.Abs (wallList [i].endY - top) < 3) && (wallList [i].endY - wallList [i].startY) < 3;
					bool bottomEdge = (Mathf.Abs (wallList [i].endY - bottom) < 3) && (wallList [i].endY - wallList [i].startY) < 3;
					if (rightEdge || leftEdge || topEdge || bottomEdge) {
						//print("Window x = " + wallList[i].x + ", startY = " + wallList[i].startY + ", endY = " + wallList[i].endY);

						GameObject newWindow = (GameObject)Resources.Load ("Window");
						newWindow.transform.localScale = new Vector3 (1f, 5f, lengthScale);

						Instantiate (newWindow, v, q);
					}

				}
			}
		}
	}

}


