using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ImageReader : MonoBehaviour {

	/*****************************Sample floor plans to use **********************************/
	//http://www.colebrook.net/wp-content/uploads/2012/08/RG-3-bdrm-with-balcony.jpg
	//http://www.roomsketcher.com/wp-content/uploads/2014/08/RoomSketcher-2D-Floor-Plan-1.jpg
	//http://www.roomsketcher.com/wp-content/uploads/2015/11/RoomSketcher-House-Floor-Plans-962270.jpg
	//http://headstartonahome.ca/pub/image/floor_plans/TheCayman/Perehudoff-Condo%20Unit%20112_1071sqft_2bed2bath%20(1000x815).jpg



	string path;		//URL path to the floor plan image
	WWW www;
	Color32 black = new Color32(10,10,10,1);
	Color32[] pixels;
	Texture2D scaledDown;
	int wallIndex = 0;

	struct Wall		//basic element that we will detect from the floor plan, can be wall, window, or door
	{
		public int x;
		public int startY;
		public int endY;
		public int type; //0 = solid wall, 1 = window or door
	};  

	Wall[] wallList = new Wall[10000];	//Array that holds all detected elements in the floor plan 


	/*
	 * pasteFromClipboard() is called when a button is pressed on the welcome screen.
	 * The latest copied text is fetched from the clipboard and stored in PlayerPrefs
	 */
	public void pasteFromClipboard(){
		//Get the latest copied text from the clipboard and use it as the path URL
		//Have to use different code for Android and for the Unity Editor
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

		PlayerPrefs.SetString ("path", path);	//saves the path URL in PlayerPrefs. We must do this otherwise the path will be lost when we switch scenes
		SceneManager.LoadScene ("gearVR");		//load the main floor plan scene
	}


	/*
	 * Called from demo1 button on the welcome screen
	 * Hard codes URL to PlayerPrefs
	 */
	public void demo1(){
		PlayerPrefs.SetString ("path", "http://www.roomsketcher.com/wp-content/uploads/2014/08/RoomSketcher-2D-Floor-Plan-1.jpg");
		SceneManager.LoadScene ("gearVR");
	}


	/*
	 * Called from demo2 button on the welcome screen
	 * Hard codes URL to PlayerPrefs
	 */
	public void demo2(){
		PlayerPrefs.SetString ("path", "http://www.roomsketcher.com/wp-content/uploads/2015/11/RoomSketcher-House-Floor-Plans-962270.jpg");
		SceneManager.LoadScene ("gearVR");
	}

	/*
	 * Called from demo3 button on the welcome screen
	 * Enables the demo scene used in the prototype
	 */
	public void demo3(){
		PlayerPrefs.SetString ("path", "demo");
		SceneManager.LoadScene ("gearVR");
	}




	// Use this for initialization
	void Start () {
		if (SceneManager.GetActiveScene ().name != "gearVR")
			return;

		path = PlayerPrefs.GetString ("path");

		if (path != "demo") {
			GameObject demoScene = GameObject.Find ("DemoScene");
			demoScene.transform.GetChild (0).gameObject.SetActive (false);


			//Calls function loadImage and waits until it finishes downloading the image
			StartCoroutine ("loadImage");

			//If the width of the image is over 500, we scale down the image using the TextureScale script
			if (www.texture.width >= 500) {
				float scale = www.texture.width / 300f;
				int newWidth = 300;
				float newHeight = www.texture.height / scale;


				scaledDown = Instantiate (www.texture);
				TextureScale.Bilinear (scaledDown, newWidth, (int)newHeight);
			} else
				scaledDown = Instantiate (www.texture);


			//Scale ground and roof planes according to image size
			GameObject ground = GameObject.Find ("Ground");
			ground.transform.localScale = new Vector3 (10f, 1f, scaledDown.height * 0.035f);

			GameObject roof = GameObject.Find ("Roof");
			roof.transform.localScale = new Vector3 (10f, 1f, scaledDown.height * 0.035f);


			//Calls the function Build and runs it in the background so the main application isn't blocked
			StartCoroutine ("Build");
		} else {
			GameObject demoScene = GameObject.Find ("DemoScene");
			demoScene.transform.GetChild (0).gameObject.SetActive (true);

			GameObject roof = GameObject.Find ("Roof");
			roof.transform.position = new Vector3(roof.transform.position.x, 7f, roof.transform.position.z);
		}

	}

	// Update is called once per frame
	void Update () {
	}




	/*
	 * loadImage downloads the image from the URL in path and saves it to www.texture
	 * A while loop is used to make the application wait until the image is fully downloaded, signified by www.isDone = true
	 */
	IEnumerator loadImage(){
		www = new WWW (path);
		while (!www.isDone) {
			//wait for image to finish downloading
		}

		yield return www;
	}



	/*
	 * Build is the main floor plan scanning algorithm code
	 * Using the downloaded floor plan, we scan along the x and y axis, getting the color of pixels
	 * If a pixel is black, it means we have detected a wall, we remember this wall until reach the endpoint of the wall signified by a non-black pixel
	 * Similarly, we detect other types of objects using different shades of black as well as their relative positions
	 * Windows are detected only if they are along the perimeter edges of the floor plan, edges are saved as vars left, right, bottom, top
	 * 
	 * After we have scanned the entire floorplan and saved all detected objects in wallList array, we go through the array, placing objects in the scene
	 * When placing objects, we filter out any errors in our detection by ignoring objects that are only 1 pixel in size, ignoring overlapping walls, etc.
	 */
	IEnumerator Build(){
		int x, y;
		int width = scaledDown.width;
		int height = scaledDown.height;
		float ratio = width / height;
		int x_incr = (int) (width / (200f*ratio));			//we use x_incr instead of x++ to skip pixels instead of scanning each one
		int y_incr = Mathf.Max(1,height / 200);				//x_incr and y_incr values are larger for larger resolution images
		int top=0, bottom=0, right=0;				//top, bottom, left, right hold the edge x and y values of the perimeter of the floor plan
		int [] left = new int [height];
		int oldTop = 0, oldRight = 0;
		bool[,] wallGrid = new bool [width, height];		//we use wallGrid and windowGrid to remember which x,y coordinates in the floor plan have an object placed there, we use this info to avoid collisions or make other decisions
		bool[,] windowGrid = new bool [width, height];
		Quaternion q = new Quaternion (0, 0, 0, 0);

		//Nested loop to iterate through the entire image stored in www.texture
		for (x = 0; x < width; x+=x_incr) {
			Wall newWall = new Wall();						//create a new Wall, we will set the wall's parameters when we detect a wall
			newWall.x = 0;
			newWall.startY = 0;
			newWall.type = 0;

			Wall newWindow = new Wall ();
			newWindow.x = 0;
			newWindow.startY = 0;
			newWindow.type = 1;

			for (y = 0; y < height; y+=y_incr) {

				//in case we go over the limit because of x_incr or y_incr
				if (x >= width || y >= height || x < 0 || y < 0)
					break;

				Color32 c1 = scaledDown.GetPixel (x, y);


				//if colour is black, we found a wall
				if (Mathf.Abs (c1.r - black.r) <= 20) {
					wallGrid [x, y] = true;

					if(left[y] == 0)
						left[y] = x;							
					
					if (bottom == 0) 
						bottom = y;

					right = x;								//rightmost edge, update every time, last value will be rightmost
					top = y;

					if (newWall.startY == 0) {				//this is the beginning of a wall, since wall's startY has not been set yet
						newWall.x = x;
						newWall.startY = y;
					}

					if (newWindow.startY != 0) { 			//if we found a wall, need to close off the window, this is to avoid overlapping windows and walls
						newWindow.x = x;
						newWindow.endY = y;

						wallList [wallIndex] = newWindow;
						wallIndex++;

						newWindow.x = 0;
						newWindow.startY = 0;
					}

					if (y == height - 1) {					//reached the edge of the image, close off the wall
						newWall.x = x;
						newWall.endY = y;

						wallList [wallIndex] = newWall;		//we add the newly created wall to wallList array, we will later use this array to populate the scene with objects
						wallIndex++;
					}

					//else if colour is greyish, found a window
				} else if (Mathf.Abs (c1.r - black.r) > 20 && Mathf.Abs (c1.r - black.r) <= 200) {
					if(left[y] == 0)
						left[y] = x;

					windowGrid [x, y] = true;

					if (newWindow.startY == 0) {			//this is the beginning of a window, since window's startY has not been set yet
						newWindow.x = x;
						newWindow.startY = y;
					}

					if (y == height - 1) {					//reached the edge of the image, close off the window
						newWindow.x = x;
						newWindow.endY = y;

						wallList [wallIndex] = newWindow;	//windows are also added to wallList, we can differentiate them from walls because type = 1
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
			if (oldTop == 0 || oldTop == top)				//after every y iteration, remember our top and right values, in case the floor plan is not a perfect rectangle
				oldTop = top;
			if (oldRight == 0 || oldRight == right)
				oldRight = right;
			yield return null;
		}













		//After scanning through the entire image, wallList now holds all detected objects
		//Place all the detected objects inside the scene by instantiating a wall, window, door for each object in wallList
		//The length is adjusted based on the start and endpoints

		for (int i = 0; i < 10000; i++) {
			if (wallList[i].startY != 0) {
				int length = wallList [i].endY - wallList [i].startY;
				int center = wallList [i].startY + length / 2;
				Vector3 v = new Vector3 ((wallList [i].x - 170) * 0.2f, 3f, (center - 150) * 0.2f);		//3D vector that holds the 3D position where we will place the new object
				float lengthScale = Mathf.Max (1, length / 5f);

				if (wallList [i].type == 0) {

					//we want walls to be long and continuous, we want to ignore any walls that are randomly placed in the middle of a room, or are only 1 pixel long
					//north, south, east, west checks if there are neighbouring walls, if there are, we should place a wall
					bool north = wallGrid [wallList [i].x, Mathf.Min (height - 1, wallList [i].endY + 1)];// || wallGrid [wallList [i].x, Mathf.Min (height - 1, wallList [i].endY + 2)];
					bool south = wallGrid [wallList [i].x, Mathf.Max (0, wallList [i].startY - 1)];// || wallGrid [wallList [i].x, Mathf.Max (0, wallList [i].startY - 2)];
					bool east = wallGrid [Mathf.Min(width-1, wallList [i].x + 1), wallList [i].startY] || wallGrid [Mathf.Min(width-1, wallList [i].x + 1), wallList [i].endY];
					bool west = wallGrid [Mathf.Max(0, wallList [i].x - 1), wallList [i].startY] || wallGrid [Mathf.Min(0, wallList [i].x - 1), wallList [i].endY];

					//if either we have a neighbouring wall, or the length of the wall is greater than 1, the wall is valid. Otherwise we might have a incorrectly detected wall.
					if (north || south || east || west || (length > 3)) {

						GameObject newWall = (GameObject)Resources.Load ("Wall");
						newWall.transform.localScale = new Vector3 (1f, 6f, lengthScale);


						Instantiate (newWall, v, q);
					} else {
						//if we do not place a wall, we need to set the wallGrid values to false
						for (int m = wallList [i].startY; m < wallList [i].endY; m++)
							wallGrid [wallList [i].x, m] = false;
						/*for (int n = wallList [i].x - 10; n < wallList [i].x + 10; n++) {
								if (n < width && m < height)
									wallGrid [n, m] = false;
							}*/

					}

					//if the wall's type is 1, we either have a window or a door
				} else if (wallList [i].type == 1) {	


					bool rightEdge = Mathf.Abs (wallList [i].x - right) < 3 || Mathf.Abs (wallList [i].endY - oldRight) < 3;
					bool leftEdge = Mathf.Abs (wallList [i].x - left[wallList[i].startY]) < 3;
					bool topEdge = (Mathf.Abs (wallList [i].endY - top) < 3 || Mathf.Abs (wallList [i].endY - oldTop) < 3) && (wallList [i].endY - wallList [i].startY) < 3;
					bool bottomEdge = (Mathf.Abs (wallList [i].endY - bottom) < 3) && (wallList [i].endY - wallList [i].startY) < 3;

					//if any of the 4 bools are true, it means the detected object lies on the perimeter of the floor plan
					//we assume that all the windows are along the perimeter
					if ((rightEdge || leftEdge || topEdge || bottomEdge) && wallList[i].x >=left[wallList[i].startY] && wallList[i].startY >= bottom) {	
						/*
						bool north = !wallGrid [wallList [i].x, Mathf.Min (height - 1, wallList [i].endY + 1)] && !wallGrid [wallList [i].x, Mathf.Min (height - 1, wallList [i].endY + 2)];
						bool south = !wallGrid [wallList [i].x, Mathf.Max (0, wallList [i].startY - 1)] && !wallGrid [wallList [i].x, Mathf.Max (0, wallList [i].startY - 2)];
						bool east = !wallGrid [Mathf.Min (width - 1, wallList [i].x + 1), wallList [i].startY] && !wallGrid [Mathf.Min (width - 1, wallList [i].x + 1), wallList [i].endY];
						bool west = !wallGrid [Mathf.Max (0, wallList [i].x - 1), wallList [i].startY] && !wallGrid [Mathf.Min (0, wallList [i].x - 1), wallList [i].endY];
						*/

						bool north = windowGrid [wallList [i].x, Mathf.Min (height - 1, wallList [i].endY + 1)];
						bool south = windowGrid [wallList [i].x, Mathf.Max (0, wallList [i].startY - 1)];
						bool east = windowGrid [Mathf.Min (width - 1, wallList [i].x + 1), wallList [i].startY] || windowGrid [Mathf.Min (width - 1, wallList [i].x + 1), wallList [i].endY];
						bool west = windowGrid [Mathf.Max (0, wallList [i].x - 1), wallList [i].startY] || windowGrid [Mathf.Min (0, wallList [i].x - 1), wallList [i].endY];

						//get the surrounding neighbours to make sure we aren't overlapping walls
						if (north || south || east || west || (length>3)) {	
							//print ("Window length: " + length + ", x: "+wallList[i].x+", y: "+wallList[i].endY+", north: " + north + ", south: " + south + ", east: " + east + ", west: " + west);
							GameObject newWindow = (GameObject)Resources.Load ("Window");
							newWindow.transform.localScale = new Vector3 (1f, 6f, lengthScale);
							Instantiate (newWindow, v, q);

						} /*else {
							//if we are overlapping neighbour walls, decrease our length until we are not
							while (wallList [i].startY < wallList [i].endY) {	
								wallList [i].startY++;
								wallList [i].endY--;
								north = !wallGrid [wallList [i].x, Mathf.Min (height - 1, wallList [i].endY + 1)];
								south = !wallGrid [wallList [i].x, Mathf.Max (0, wallList [i].startY - 1)];
								east = !wallGrid [Mathf.Min (width - 1, wallList [i].x + 1), wallList [i].startY];
								west = !wallGrid [Mathf.Max (0, wallList [i].x - 1), wallList [i].startY];

								//check neighbours again after decreasing length
								if (north && south && east && west) {
									length = wallList [i].endY - wallList [i].startY;
									if (length <= 1)
										break;
									center = wallList [i].startY + length / 2;
									v = new Vector3 ((wallList [i].x - 170) * 0.3f, 3f, (center - 150) * 0.3f);
									lengthScale = Mathf.Max (1, length / 4);

									GameObject newWindow = (GameObject)Resources.Load ("Window");
									newWindow.transform.localScale = new Vector3 (1f, 6f, lengthScale);

									Instantiate (newWindow, v, q);
								}
							}

						}*/
					} else {	//if the detected object is not on the perimeter, and is not a wall, we treat it as a door
						//doors are detected using a diagonal line, therefore we get NE, SE, NW and SW to detect the presence of a diagonal
						//each boolean value is only true if the diagonal is at least 3 pixels long

						bool northeast = windowGrid [Mathf.Min(width-1, wallList [i].x + 1), Mathf.Min (height - 1, wallList [i].endY + 1)] && windowGrid [Mathf.Min(width-1, wallList [i].x + 2), Mathf.Min (height - 1, wallList [i].endY + 2)] && windowGrid [Mathf.Min(width-1, wallList [i].x + 3), Mathf.Min (height - 1, wallList [i].endY + 3)];
						bool southeast = windowGrid [Mathf.Min(width-1, wallList [i].x + 1), Mathf.Max (0, wallList [i].startY - 1)] && windowGrid [Mathf.Min(width-1, wallList [i].x + 2), Mathf.Max (0, wallList [i].startY - 2)] && windowGrid [Mathf.Min(width-1, wallList [i].x + 3), Mathf.Max (0, wallList [i].startY - 3)];
						bool northwest = windowGrid [Mathf.Max(0, wallList [i].x - 1), Mathf.Min (height - 1, wallList [i].endY + 1)] && windowGrid [Mathf.Max(0, wallList [i].x - 2), Mathf.Min (height - 1, wallList [i].endY + 2)] && windowGrid [Mathf.Max(0, wallList [i].x - 3), Mathf.Min (height - 1, wallList [i].endY + 3)];
						bool southwest = windowGrid [Mathf.Max(0, wallList [i].x - 1), Mathf.Max (0, wallList [i].startY - 1)] && windowGrid [Mathf.Max(0, wallList [i].x - 2), Mathf.Max (0, wallList [i].startY - 2)] && windowGrid [Mathf.Max(0, wallList [i].x - 3), Mathf.Max (0, wallList [i].startY - 3)];

						if (northeast || southeast || northwest || southwest) {
							//once we have detected a door, we need to solve the problem where we detect another diagonal on the same diagonal line
							//we need to set windowGrid to false in the surrounding area to prevent any duplicate doors
							int m = 0, n = 0;
							for (m = wallList [i].x - 10; m < wallList [i].x + 10; m++) {
								for (n = wallList [i].startY - 10; n < wallList [i].endY + 10; n++) {
									if (m >= 0 && m < width && n >= 0 && n < height)
										windowGrid [m, n] = false;
								}
							}

							//want to place the door adjacent to a wall
							//search surrounding area for any presence of a wall
							bool foundWall = false;
							for (m = Mathf.Max(0,wallList [i].x - 20); m < wallList [i].x + 20 && m < width && foundWall == false; m++) {
								for (n = Mathf.Max(0,wallList [i].startY - 20); n < wallList [i].endY + 20 && n < height && foundWall == false; n++) {
									if (wallGrid [m, n]) {
										m += 5;
										n += 1;
										foundWall = true;
									}
								}
							}

							if (foundWall) {
								v = new Vector3 ((m - 170) * 0.3f, 5f, (n - 150) * 0.3f);
								GameObject newWindow = (GameObject)Resources.Load ("Door");
								newWindow.transform.localScale = new Vector3 (0.2f, 0.2f, 0.2f);
								Instantiate (newWindow, v, q);
							}

						}
						else {
							//if we do not place a wall, we need to set the wallGrid values to false
							for (int m = wallList [i].startY-4; m < wallList [i].endY+4; m++) {
								//windowGrid [wallList [i].x, m] = false;
								for (int n = wallList [i].x - 5; n < wallList [i].x + 5; n++) {
									if (n < width && m < height && n > 0 && m > 0)
										windowGrid [n, m] = false;
								}
							}

						}


					}
				} 
			}
		}



		/*

		//place furniture
		int frequency = 0;
		for (x = left; x >= left && (x < right || x < oldRight); x += x_incr * 10) {
			for (y = bottom; y >= bottom && (y < top || y < oldTop); y += y_incr * 10) {
				Collider[] col = Physics.OverlapSphere (new Vector3 ((x - 170) * 0.3f, 5f, (y - 150) * 0.3f), 5f);
				if (col.Length == 0) {
					frequency += 1;
					if (frequency >= 10) {
						frequency = 0;
						int rand = Random.Range (0, 5);
						if (rand < 3) {
							GameObject newFurniture = (GameObject)Resources.Load ("sofa");
							Instantiate (newFurniture, new Vector3 ((x - 170) * 0.3f, 0f, (y - 150) * 0.3f), new Quaternion (0, 0, 0, 0));
						}
					}
				}
			}
		}
		*/

	}


}

