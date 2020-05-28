using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Net;
using System.Reflection.Emit;
using System.Collections.Generic;

public class MazeLoader : MonoBehaviour {

    #region Editor

    [SerializeField] public int mazeRows;
	[SerializeField] public int mazeColumns;
	[SerializeField] public GameObject wall;
	[SerializeField] public GameObject floor;
	[SerializeField] public GameObject ceiling;
	[SerializeField] public float size;

	[SerializeField] public FirstPersonAIO player;
	[SerializeField] private NavMeshSurface surface;
	[SerializeField] private GameObject agent;
	[SerializeField] private TimeController timeController;
	[SerializeField] public Text fragmentCounter;
	[SerializeField] public Text fragmentInfo;
	[SerializeField] public GameObject fragment;

    #endregion

    #region Variables

    public bool mazeChanged = false;
	public bool isChanging = false;
	public int layoutIndex = 1;
	private string playerSectionTag = "SECTION ONE";
	public int fragments = 0;

	public MazeCell[,] mazeCells;
	public List<GameObject> fragmentList;

	#endregion

	void Start()
	{
		// Initialize the maze by building the cells
		InitializeMaze();

		// Carve a path out of the cells to create a maze
		MazeAlgorithm ma = new HuntAndKillMazeAlgorithm(mazeCells);
		ma.CreateMaze();

		// Update the nav mesh
		mazeChanged = true;

		// Invoke the method that changes the different sections of the maze
		InvokeRepeating("BuildNewMazeSection", 1.0f, 1.0f);
	}

	/**
	 * Reset the map and build a new maze.
	 */
	private void BuildNewMazeSection()
	{
		// Check if the remaining time is divisible by 5 by checking the last digit (using mod causes this to be true longer than we want for some reason, so we do it this way)
		bool correctInterval = timeController.timeLeft.ToString().EndsWith("5") || timeController.timeLeft.ToString().EndsWith("0");

		// Don't build new sections while time is reversing or the time remaining isn't divisible by 5
		if (timeController.isReversing || !correctInterval) return;

		// Copy the maze layout
		CopyCurrentMazeLayout();

		// Randomly choose a section of the maze that the player ISN'T in to change
		int section = UnityEngine.Random.Range(1, 5);
		switch (section)
		{			
			case 1:
				if (playerSectionTag == "SECTION ONE")
				{
					BuildNewMazeSection();
					break;
				}
				GenerateMazeSection(0, 0, 6, 9, "SectionOne");
				break;
			case 2:
				if (playerSectionTag == "SECTION TWO")
				{
					BuildNewMazeSection();
					break;
				}
				GenerateMazeSection(0, 9, 6, 18, "SectionTwo");				
				break;
			case 3:
				if (playerSectionTag == "SECTION THREE")
				{
					BuildNewMazeSection();
					break;
				}
				GenerateMazeSection(6, 0, 12, 9, "SectionThree");
				break;
			case 4:
				if (playerSectionTag == "SECTION FOUR")
				{
					BuildNewMazeSection();
					break;
				}
				GenerateMazeSection(6, 9, 12, 18, "SectionFour");
				break;
		}

		// Carve a path out of the cells to create a maze
		MazeAlgorithm ma = new HuntAndKillMazeAlgorithm(mazeCells);
		ma.CreateMaze();

		// Update the nav mesh
		mazeChanged = true;
	}

	void Update ()
	{
		// Figure out what section of the maze the player is currently in
		GetPlayerSection();

		// If the maze has changed, rebuild the nav mesh
		if (mazeChanged)
		{
			// Update the nav mesh
			surface.BuildNavMesh();
			mazeChanged = false;
			isChanging = false;
		}

		// Update the fragment counter text
		fragmentCounter.text = fragments.ToString();

		LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
		lineRenderer.SetPosition(0, player.transform.position);
		lineRenderer.SetPosition(1, agent.transform.position);
	}

	/**
	 * Initialize the maze, creating walled cells in the given dimensions.
	 */
	private void InitializeMaze()
	{
		// Initialize a new array of MazeCells with the given dimensions
		mazeCells = new MazeCell[mazeRows, mazeColumns];

		// Iterate over the cells, creating the floor, ceiling, and walls
		for (int r = 0; r < mazeRows; r++) {
			for (int c = 0; c < mazeColumns; c++) {
				
				mazeCells[r, c] = new MazeCell();
				string sectionTag = GetSectionTag(r, c);

				// Floor
				mazeCells[r, c].floor = Instantiate(floor, new Vector3(r*size, -(size/2f), c*size), Quaternion.identity) as GameObject;
				mazeCells[r, c].floor.name = "Floor " + r + "," + c;
				mazeCells[r, c].floor.transform.Rotate(Vector3.right, 90f);

				// Ceiling
				mazeCells[r, c].ceiling = Instantiate(ceiling, new Vector3(r * size, (size / 2f), c * size), Quaternion.identity) as GameObject;
				mazeCells[r, c].ceiling.name = "Ceiling " + r + "," + c;
				mazeCells[r, c].ceiling.transform.Rotate(Vector3.left, 90f);
				mazeCells[r, c].ceiling.layer = 9; // We don't want the mini map camera to render the ceiling

				// West walls
				if (c == 0) {
					mazeCells[r, c].westWall = Instantiate(wall, new Vector3(r*size, 0, (c*size) - (size/2f)), Quaternion.identity) as GameObject;
					mazeCells[r, c].westWall.name = "West Wall " + r + "," + c;
					mazeCells[r, c].westWall.tag = sectionTag;
				}

				// East walls
				mazeCells[r, c].eastWall = Instantiate(wall, new Vector3 (r*size, 0, (c*size) + (size/2f)), Quaternion.identity) as GameObject;
				mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;
				mazeCells[r, c].eastWall.tag = sectionTag;

				// North walls
				if (r == 0) {
					mazeCells[r, c].northWall = Instantiate(wall, new Vector3((r*size) - (size/2f), 0, c*size), Quaternion.identity) as GameObject;
					mazeCells[r, c].northWall.name = "North Wall " + r + "," + c;
					mazeCells[r, c].northWall.transform.Rotate(Vector3.up * 90f);
					mazeCells[r, c].northWall.tag = sectionTag;
				}

				// South walls
				mazeCells[r, c].southWall = Instantiate(wall, new Vector3((r*size) + (size/2f), 0, c*size), Quaternion.identity) as GameObject;
				mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
				mazeCells[r, c].southWall.transform.Rotate(Vector3.down * 90f);
				mazeCells[r, c].southWall.tag = sectionTag;
			}
		}

		// Create and place the fragments in the maze
		PlaceFragments();		
	}

	/**
	 * Place 4 fragments in the maze, each in a random location in one of the four sections.
	 */
	private void PlaceFragments()
	{
		// List to store the fragments for when the player rewinds time
		fragmentList = new List<GameObject>();

		// Variables for creating the fragments
		Vector3 position;
		GameObject go;

		// Place each fragment
		for (int i = 1; i <= 4; i++)
		{
			int row;
			int col;
			switch (i)
			{
				// Section One
				case 1:
					// Get a random cell in section one
					row = UnityEngine.Random.Range(2, 5);
					col = UnityEngine.Random.Range(2, 8);
					position = mazeCells[row, col].floor.transform.position;

					// Create a fragment at the given position
					go = Instantiate(fragment, new Vector3(position.x, -0.75f, position.z), Quaternion.identity) as GameObject;
					go.name = "FRAGMENT " + row + "," + col;
					go.tag = "Fragment";
					fragments++;

					// Add the fragment to the list
					fragmentList.Add(go);

					break;
				// Section Two
				case 2:
					// Get a random cell in section two
					row = UnityEngine.Random.Range(2, 5);
					col = UnityEngine.Random.Range(11, 17);
					position = mazeCells[row, col].floor.transform.position;

					// Create a fragment at the given position
					go = Instantiate(fragment, new Vector3(position.x, -0.75f, position.z), Quaternion.identity) as GameObject;
					go.name = "FRAGMENT " + row + "," + col;
					go.tag = "Fragment";
					fragments++;

					// Add the fragment to the list
					fragmentList.Add(go);

					break;
				// Section Three
				case 3:
					// Get a random cell in section three
					row = UnityEngine.Random.Range(8, 11);
					col = UnityEngine.Random.Range(2, 8);
					position = mazeCells[row, col].floor.transform.position;

					// Create a fragment at the given position
					go = Instantiate(fragment, new Vector3(position.x, -0.75f, position.z), Quaternion.identity) as GameObject;
					go.name = "FRAGMENT " + row + "," + col;
					go.tag = "Fragment";
					fragments++;

					// Add the fragment to the list
					fragmentList.Add(go);

					break;
				// Section Four
				case 4:
					// Get a random cell in section four
					row = UnityEngine.Random.Range(8, 11);
					col = UnityEngine.Random.Range(11, 17);
					position = mazeCells[row, col].floor.transform.position;

					// Create a fragment at the given position
					go = Instantiate(fragment, new Vector3(position.x, -0.75f, position.z), Quaternion.identity) as GameObject;
					go.name = "FRAGMENT " + row + "," + col;
					go.tag = "Fragment";
					fragments++;

					// Add the fragment to the list
					fragmentList.Add(go);

					break;
			}
		}
	}

	/**
	 * Generates a section of the maze, prepping it to be changed.
	 */
	private void GenerateMazeSection(int startRow, int startCol, int stopRow, int stopCol, string tag)
	{
		// Grab all objects with the given tag, then destroy them		
		GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
		foreach (GameObject go in objects)
		{
			// Only destroy it if the y value is 0, since we have copies below the maze
			if (go.transform.position.y == 0.0f)
			{
				Destroy(go);
			}			
		}

		// Iterate over the cells associated with the rows and columns, creating the floor and walls
		for (int r = startRow; r < stopRow; r++)
		{
			for (int c = startCol; c < stopCol; c++)
			{
				mazeCells[r, c] = new MazeCell();

				// West walls
				if (c == 0)
				{
					mazeCells[r, c].westWall = Instantiate(wall, new Vector3(r * size, 0, (c * size) - (size / 2f)), Quaternion.identity) as GameObject;
					mazeCells[r, c].westWall.name = "West Wall " + r + "," + c;
					mazeCells[r, c].westWall.tag = tag;
				}

				// East walls
				if ((tag == "SectionOne" || tag == "SectionThree") && c == (stopCol - 1))
				{
					if (UnityEngine.Random.value > 0.5f)
					{
						mazeCells[r, c].eastWall = Instantiate(wall, new Vector3(r * size, 0, (c * size) + (size / 2f)), Quaternion.identity) as GameObject;
						mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;
						mazeCells[r, c].eastWall.tag = tag;
					}
				}
				else
				{
					mazeCells[r, c].eastWall = Instantiate(wall, new Vector3(r * size, 0, (c * size) + (size / 2f)), Quaternion.identity) as GameObject;
					mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;
					mazeCells[r, c].eastWall.tag = tag;
				}

				// North walls
				if (r == 0)
				{
					mazeCells[r, c].northWall = Instantiate(wall, new Vector3((r * size) - (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
					mazeCells[r, c].northWall.name = "North Wall " + r + "," + c;
					mazeCells[r, c].northWall.transform.Rotate(Vector3.up * 90f);
					mazeCells[r, c].northWall.tag = tag;
				}

				// South walls
				if ((tag == "SectionOne" || tag == "SectionTwo") && r == (stopRow - 1))
				{
					if (UnityEngine.Random.value > 0.5f)
					{
						mazeCells[r, c].southWall = Instantiate(wall, new Vector3((r * size) + (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
						mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
						mazeCells[r, c].southWall.transform.Rotate(Vector3.up * 90f);
						mazeCells[r, c].southWall.tag = tag;
					}
				}
				else
				{
					mazeCells[r, c].southWall = Instantiate(wall, new Vector3((r * size) + (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
					mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
					mazeCells[r, c].southWall.transform.Rotate(Vector3.up * 90f);
					mazeCells[r, c].southWall.tag = tag;
				}
			}
		}
	}

	/**
	 * Returns the tag associated with the given row and column.
	 */
	public string GetSectionTag(int row, int col)
	{
		string tag = "";
		if (row < 6 && col < 9) // Section One
		{
			tag = "SectionOne";
		}
		else if (row < 6 && col >= 9) // Section Two
		{
			tag = "SectionTwo";
		}
		else if (row >= 6 && col < 9) // Section Three
		{
			tag = "SectionThree";
		}
		else if (row >= 6 && col >= 9) // Section Four
		{
			tag = "SectionFour";
		}

		return tag;
	}

	/**
	 * Determines what section of the maze the player is currently in by checking their row and column position.
	 */
	private void GetPlayerSection()
	{
		int row = Mathf.Clamp((int)player.transform.position.x / 5, 0, mazeRows);
		int col = Mathf.Clamp((int)player.transform.position.z / 5, 0, mazeColumns);
		if (row < 6 && col < 9)
		{
			playerSectionTag = "SECTION ONE";
			//Debug.Log("SECTION ONE -- " + "ROW: " + row + ", COL: " + col);
		}
		else if (row < 6 && col >= 9)
		{
			playerSectionTag = "SECTION TWO";
			//Debug.Log("SECTION TWO -- " + "ROW: " + row + ", COL: " + col);
		}
		else if (row >= 6 && col < 9)
		{
			playerSectionTag = "SECTION THREE";
			//Debug.Log("SECTION THREE -- " + "ROW: " + row + ", COL: " + col);
		}
		else if (row >= 6 && col >= 9)
		{
			playerSectionTag = "SECTION FOUR";
			//Debug.Log("SECTION FOUR -- " + "ROW: " + row + ", COL: " + col);
		}
	}

	/**
	 * Creates a copy of the current maze layout so we can revert the maze when the player rewinds time.
	 */
	private void CopyCurrentMazeLayout()
	{
		// Initialize an array to store the current layout of the maze
		MazeCell[,] layoutCopy = new MazeCell[mazeRows, mazeColumns];

		// Destroy any walls at the depth we're about to create the copy at
		GameObject[] objects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
		foreach (GameObject go in objects)
		{
			// This iterates over every game object, so make sure we only destroy the ones at the specified depth
			if (go.transform.position.y == (layoutIndex * -10))
			{
				Destroy(go);
			}
		}

		// Iterate over the current maze in order to make a copy of it
		for (int r = 0; r < mazeRows; r++)
		{
			for (int c = 0; c < mazeColumns; c++)
			{
				// Initialize the current cell and determine which section it is in (so we can tag it appropriately)
				layoutCopy[r, c] = new MazeCell();
				layoutCopy[r, c].visited = true;

				string sectionTag = GetSectionTag(r, c);

				// If there's a north wall in this cell, create one in the copy
				if (mazeCells[r, c].northWall != null)
				{
					layoutCopy[r, c].northWall = Instantiate(wall, new Vector3((r * size) - (size / 2f), layoutIndex * -10, c * size), Quaternion.identity) as GameObject;
					layoutCopy[r, c].northWall.name = "North Wall " + r + "," + c;
					layoutCopy[r, c].northWall.transform.Rotate(Vector3.up * 90f);
					layoutCopy[r, c].northWall.tag = sectionTag;
				}

				// If there's a south wall in this cell, create one in the copy
				if (mazeCells[r, c].southWall != null)
				{
					layoutCopy[r, c].southWall = Instantiate(wall, new Vector3((r * size) + (size / 2f), layoutIndex * -10, c * size), Quaternion.identity) as GameObject;
					layoutCopy[r, c].southWall.name = "South Wall " + r + "," + c;
					layoutCopy[r, c].southWall.transform.Rotate(Vector3.up * 90f);
					layoutCopy[r, c].southWall.tag = sectionTag;
				}

				// If there's a east wall in this cell, create one in the copy
				if (mazeCells[r, c].eastWall != null)
				{
					layoutCopy[r, c].eastWall = Instantiate(wall, new Vector3(r * size, layoutIndex * -10, (c * size) + (size / 2f)), Quaternion.identity) as GameObject;
					layoutCopy[r, c].eastWall.name = "East Wall " + r + "," + c;
					layoutCopy[r, c].eastWall.tag = sectionTag;
				}

				// If there's a west wall in this cell, create one in the copy
				if (mazeCells[r, c].westWall != null)
				{
					layoutCopy[r, c].westWall = Instantiate(wall, new Vector3(r * size, layoutIndex * -10, (c * size) - (size / 2f)), Quaternion.identity) as GameObject;
					layoutCopy[r, c].westWall.name = "West Wall " + r + "," + c;
					layoutCopy[r, c].westWall.tag = sectionTag;
				}
			}
		}

		// Add the copy to the list of copies
		timeController.layoutList.Add(layoutCopy);

		// Increment the index counter
		layoutIndex++;

		// If the index is greater than 3 (we only want three copies of the maze), reset it and remove the first copy
		if (layoutIndex > 2)
		{
			layoutIndex = 1;
			timeController.layoutList.RemoveAt(0);			
		}		
	}	
}