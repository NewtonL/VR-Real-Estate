using UnityEngine;
using System.Collections;

public class ImageReader : MonoBehaviour {

	//http://www.colebrook.net/wp-content/uploads/2012/08/RG-3-bdrm-with-balcony.jpg
	//http://www.roomsketcher.com/wp-content/uploads/2014/08/RoomSketcher-2D-Floor-Plan-1.jpg
	//http://www.roomsketcher.com/wp-content/uploads/2015/11/RoomSketcher-House-Floor-Plans-962270.jpg
	string path;
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

		#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"); 
			AndroidJavaObject clipboard = jo.Call<AndroidJavaObject>("getSystemService","clipboard");

			AndroidJavaObject clipdata = clipboard.Call<AndroidJavaObject>("getPrimaryClip");
			AndroidJavaObject item = clipdata.Call<AndroidJavaObject>("getItemAt", 0);
			AndroidJavaObject text = item.Call<AndroidJavaObject>("getText");
			path = text.Call<string>("toString");

		#endif

		#if UNITY_EDITOR
			path = GUIUtility.systemCopyBuffer;
		#endif
		print ("Path is: " + path);

		StartCoroutine ("loadImage");

		//scale down the image, larger images will be scaled down more
		if (www.texture.width >= 500) {
			float scale = www.texture.width / 300f;
			int newWidth = 300;
			float newHeight = www.texture.height / scale;


			scaledDown = Instantiate (www.texture);
			TextureScale.Bilinear (scaledDown, newWidth, (int) newHeight);
		}
		else
			scaledDown = Instantiate (www.texture);

		//scale ground and roof planes according to image size
		GameObject ground = GameObject.Find ("Ground");
		ground.transform.localScale = new Vector3 (10f, 1f, scaledDown.height * 0.035f);

		GameObject roof = GameObject.Find ("Roof");
		roof.transform.localScale = new Vector3 (10f, 1f, scaledDown.height * 0.035f);

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
		int oldTop = 0, oldRight = 0;
		bool[,] wallGrid = new bool [width, height];

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
					wallGrid [x, y] = true;

					if(left == 0){
						left = x;	//leftmost edge of the property, only set once
					}
					if (bottom == 0) {
						bottom = y;
					}
					right = x;		//rightmost edge, update every time, last value will be rightmost
					top = y;



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
			if (oldTop == 0 || oldTop == top)		//after every y iteration, remember our top and right values, in case the floor plan is not a perfect rectangle
				oldTop = top;
			if (oldRight == 0 || oldRight == right)
				oldRight = right;
			yield return null;
		}
			

		//Place all the walls inside the scene by instantiating a wall for each wall in wallList
		//One object is made for each wall, the length is adjusted based on the start and endpoints
		for (int i = 0; i < 10000; i++) {
			if (wallList[i].startY != 0) {
				int length = wallList [i].endY - wallList [i].startY;
				int center = wallList [i].startY + length / 2;
				Vector3 v = new Vector3 ((wallList [i].x - 170) * 0.3f, 3f, (center - 150) * 0.3f);
				float lengthScale = Mathf.Max (1, length / 4);
				if (wallList [i].type == 0) {

					//print("Wall x = " + wallList[i].x + ", startY = " + wallList[i].startY + ", endY = " + wallList[i].endY);

					GameObject newWall = (GameObject)Resources.Load ("Wall");
					newWall.transform.localScale = new Vector3 (1f, 6f, lengthScale);

					newWall.gameObject.tag = "Wall";

					Instantiate (newWall, v, q);

				} else if (wallList [i].type == 1) {
					
					bool rightEdge = Mathf.Abs (wallList [i].x - right) < 3 || Mathf.Abs (wallList [i].endY - oldRight) < 3;
					bool leftEdge = Mathf.Abs (wallList [i].x - left) < 3;
					bool topEdge = (Mathf.Abs (wallList [i].endY - top) < 3 || Mathf.Abs (wallList [i].endY - oldTop) < 3) && (wallList [i].endY - wallList [i].startY) < 3;
					bool bottomEdge = (Mathf.Abs (wallList [i].endY - bottom) < 3) && (wallList [i].endY - wallList [i].startY) < 3;
					if (rightEdge || leftEdge || topEdge || bottomEdge) {
						
						
						bool north = !wallGrid [wallList [i].x, Mathf.Min(height-1, wallList [i].endY + 1)] && !wallGrid [wallList [i].x, Mathf.Min(height-1, wallList [i].endY + 2)];
						bool south = !wallGrid [wallList [i].x, Mathf.Max(0, wallList [i].startY - 1)] && !wallGrid [wallList [i].x, Mathf.Max(0, wallList [i].startY - 2)];
						bool east = !wallGrid [Mathf.Min(width-1, wallList [i].x + 1), wallList [i].startY] && !wallGrid [Mathf.Min(width-1, wallList [i].x + 1), wallList [i].endY];
						bool west = !wallGrid [Mathf.Max(0, wallList [i].x - 1), wallList [i].startY] && !wallGrid [Mathf.Min(0, wallList [i].x - 1), wallList [i].endY];

						if (north && south && east && west) {

							GameObject newWindow = (GameObject)Resources.Load ("Window");
							newWindow.transform.localScale = new Vector3 (1f, 6f, lengthScale);
							//print("Window1 x = " + wallList[i].x + ", startY = " + wallList[i].startY + ", endY = " + wallList[i].endY);
							Instantiate (newWindow, v, q);
						}
						else {
							while (wallList [i].startY < wallList [i].endY) {
								wallList [i].startY++;
								wallList [i].endY--;
								north = !wallGrid [wallList [i].x, Mathf.Min(height-1, wallList [i].endY + 1)];
								south = !wallGrid [wallList [i].x, Mathf.Max(0, wallList [i].startY - 1)];
								east = !wallGrid [Mathf.Min(width-1, wallList [i].x + 1), wallList [i].startY];
								west = !wallGrid [Mathf.Max(0, wallList [i].x - 1), wallList [i].startY];

								if (north && south && east && west) {
									length = wallList [i].endY - wallList [i].startY;
									if (length <= 1)
										break;
									center = wallList [i].startY + length / 2;
									v = new Vector3 ((wallList [i].x - 170) * 0.3f, 3f, (center - 150) * 0.3f);
									lengthScale = Mathf.Max (1, length / 4);

									//print("Window2 x = " + wallList[i].x + ", startY = " + wallList[i].startY + ", endY = " + wallList[i].endY);

									GameObject newWindow = (GameObject)Resources.Load ("Window");
									newWindow.transform.localScale = new Vector3 (1f, 6f, lengthScale);

									Instantiate (newWindow, v, q);
									}
								}

							}
						}
					} 
				}
			}
		}


}


