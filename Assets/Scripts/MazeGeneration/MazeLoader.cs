using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using UnityEngine.UI;

public class MazeLoader : MonoBehaviour {

	[SerializeField] private int mazeRows;
	[SerializeField] private int mazeColumns;
	[SerializeField] private GameObject wall;
	[SerializeField] private GameObject floor;
	[SerializeField] private GameObject ceiling;
	[SerializeField] private float size;

	[SerializeField] private FirstPersonAIO player;
	[SerializeField] private NavMeshSurface surface;
	[SerializeField] private GameObject agent;

/*	[SerializeField] private Text timeLabel;
	private DateTime startTime;
	public int timeLimit;
	private int reduceLimitBy;*/

	private bool mazeChanged = false;

	/*	private const int sectionOneStartRow = 0;
		private const int sectionOneStopRow = 6;
		private const int sectionOneStartCol = 0;
		private const int sectionOneStopCol = 9;

		private const int sectionTwoStartRow = 0;
		private const int sectionTwoStopRow = 6;
		private const int sectionTwoStartCol = 9;
		private const int sectionTwoStopCol = 18;

		private const int sectionThreeStartRow = 6;
		private const int sectionThreeStopRow = 12;
		private const int sectionThreeStartCol = 0;
		private const int sectionThreeStopCol = 9;

		private const int sectionFourStartRow = 6;
		private const int sectionFourStopRow = 12;
		private const int sectionFourStartCol = 9;
		private const int sectionFourStopCol = 18;*/

	public MazeCell[,] mazeCells;

	private TimeController timeController;

	// Use this for initialization
	void Start()
	{
		timeController = FindObjectOfType(typeof(TimeController)) as TimeController;

		/*// Initialize the timer
		timeLimit = 80;
		reduceLimitBy = 5;
		startTime = DateTime.Now;*/

		// Initialize the maze by building the cells
		InitializeMaze();		

		// Carve a path out of the cells to create a maze
		MazeAlgorithm ma = new HuntAndKillMazeAlgorithm(mazeCells);
		ma.CreateMaze();

		// Spawn some AI
		//SpawnAI();

		// Update the nav mesh
		mazeChanged = true;

		// Invoke the method that changes the different sections of the maze
		InvokeRepeating("BuildNewMazeSection", 5.0f, 5.0f);
	}

	/**
	 * Reset the map and build a new maze.
	 */
	private void BuildNewMazeSection()
	{
		timeController.mazeCells = mazeCells;
		// Randomly choose a section of the maze to change
		int section = UnityEngine.Random.Range(1, 5);
		switch (section)
		{
			case 1:
				GenerateMazeSection(0, 0, 6, 9, "SectionOne");
				break;
			case 2:
				GenerateMazeSection(0, 9, 6, 18, "SectionTwo");
				break;
			case 3:
				GenerateMazeSection(6, 0, 12, 9, "SectionThree");
				break;
			case 4:
				GenerateMazeSection(6, 9, 12, 18, "SectionFour");
				break;
		}

		// Carve a path out of the cells to create a maze
		MazeAlgorithm ma = new HuntAndKillMazeAlgorithm(mazeCells);
		ma.CreateMaze();

		// Update the nav mesh
		mazeChanged = true;
	}

	// Update is called once per frame
	void Update ()
	{
		/*// Reduce the remaining time
		int timeUsed = (int)(DateTime.Now - startTime).TotalSeconds;
		int timeLeft = timeLimit - timeUsed;

		// If there is still time remaining, display the updated value
		if (timeLeft > 0)
		{
			timeLabel.text = timeLeft.ToString();
		}
		// Time has run out
		else
		{
			timeLabel.text = "FIND THE EXIT";
		}*/

		// If the maze has changed, rebuild the nav mesh
		if (mazeChanged)
		{
			// Update the nav mesh
			surface.BuildNavMesh();
			mazeChanged = false;
		}
	}

	/**
	 * Initialize the maze, creating walled cells in the given dimensions.
	 */
	private void InitializeMaze()
	{
		// Initialize a new array of MazeCells with the given dimensions
		mazeCells = new MazeCell[mazeRows, mazeColumns];		

		// Iterate over the cells, creating the floor and walls
		for (int r = 0; r < mazeRows; r++) {
			for (int c = 0; c < mazeColumns; c++) {
				mazeCells[r, c] = new MazeCell();
				string tag = GetSectionTag(r, c);

				// Floor
				mazeCells[r, c].floor = Instantiate(floor, new Vector3(r*size, -(size/2f), c*size), Quaternion.identity) as GameObject;
				mazeCells[r, c].floor.name = "Floor " + r + "," + c;
				mazeCells[r, c].floor.transform.Rotate(Vector3.right, 90f);

				// Ceiling
				mazeCells[r, c].ceiling = Instantiate(ceiling, new Vector3(r * size, (size / 2f), c * size), Quaternion.identity) as GameObject;
				mazeCells[r, c].ceiling.name = "Ceiling " + r + "," + c;
				mazeCells[r, c].ceiling.transform.Rotate(Vector3.left, 90f);
				mazeCells[r, c].ceiling.layer = 9;

				// West walls
				if (c == 0) {
					mazeCells[r, c].westWall = Instantiate(wall, new Vector3(r*size, 0, (c*size) - (size/2f)), Quaternion.identity) as GameObject;
					mazeCells[r, c].westWall.name = "West Wall " + r + "," + c;
					mazeCells[r, c].westWall.tag = tag;
					//mazeCells[r, c].westWall.layer = 8;
				}

				// East walls
				mazeCells[r, c].eastWall = Instantiate(wall, new Vector3 (r*size, 0, (c*size) + (size/2f)), Quaternion.identity) as GameObject;
				mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;
				mazeCells[r, c].eastWall.tag = tag;
				//mazeCells[r, c].eastWall.layer = 8;

				// North walls
				if (r == 0) {
					mazeCells[r, c].northWall = Instantiate(wall, new Vector3((r*size) - (size/2f), 0, c*size), Quaternion.identity) as GameObject;
					mazeCells[r, c].northWall.name = "North Wall " + r + "," + c;
					mazeCells[r, c].northWall.transform.Rotate(Vector3.up * 90f);
					mazeCells[r, c].northWall.tag = tag;
					//mazeCells[r, c].northWall.layer = 8;
				}

				// South walls
				mazeCells[r, c].southWall = Instantiate(wall, new Vector3((r*size) + (size/2f), 0, c*size), Quaternion.identity) as GameObject;
				mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
				mazeCells[r, c].southWall.transform.Rotate(Vector3.up * 90f);
				mazeCells[r, c].southWall.tag = tag;
				//mazeCells[r, c].southWall.layer = 8;				
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
			Destroy(go);
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
					//mazeCells[r, c].westWall.layer = 8;
				}

				// East walls
				if ((tag == "SectionOne" || tag == "SectionThree") && c == stopCol - 1)
				{
					if (UnityEngine.Random.value > 0.3f)
					{
						mazeCells[r, c].eastWall = Instantiate(wall, new Vector3(r * size, 0, (c * size) + (size / 2f)), Quaternion.identity) as GameObject;
						mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;
						mazeCells[r, c].eastWall.tag = tag;
						//mazeCells[r, c].eastWall.layer = 8;
					}
				}
				else
				{
					mazeCells[r, c].eastWall = Instantiate(wall, new Vector3(r * size, 0, (c * size) + (size / 2f)), Quaternion.identity) as GameObject;
					mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;
					mazeCells[r, c].eastWall.tag = tag;
					//mazeCells[r, c].eastWall.layer = 8;
				}

				// North walls
				if (r == 0)
				{
					mazeCells[r, c].northWall = Instantiate(wall, new Vector3((r * size) - (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
					mazeCells[r, c].northWall.name = "North Wall " + r + "," + c;
					mazeCells[r, c].northWall.transform.Rotate(Vector3.up * 90f);
					mazeCells[r, c].northWall.tag = tag;
					//mazeCells[r, c].northWall.layer = 8;
				}

				// South walls
				if ((tag == "SectionOne" || tag == "SectionTwo") && r == stopRow - 1)
				{
					if (UnityEngine.Random.value > 0.3f)
					{
						mazeCells[r, c].southWall = Instantiate(wall, new Vector3((r * size) + (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
						mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
						mazeCells[r, c].southWall.transform.Rotate(Vector3.up * 90f);
						mazeCells[r, c].southWall.tag = tag;
						//mazeCells[r, c].southWall.layer = 8;
					}
				}
				else
				{
					mazeCells[r, c].southWall = Instantiate(wall, new Vector3((r * size) + (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
					mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
					mazeCells[r, c].southWall.transform.Rotate(Vector3.up * 90f);
					mazeCells[r, c].southWall.tag = tag;
					//mazeCells[r, c].southWall.layer = 8;
				}
			}
		}
	}

	/**
	 * Returns the tag associated with the given row and column.
	 */
	private string GetSectionTag(int row, int col)
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

	private void SpawnAI()
	{
		int numEnemies = 0;
		for (int r = 6; r < mazeRows; r++)
		{
			for (int c = 9; c < mazeColumns; c++)
			{
				// Should we spawn an enemy here?
				if (UnityEngine.Random.value > 0.8f && numEnemies < 3)
				{
					Vector3 position = new Vector3(r, 1.25f, c);
					Instantiate(agent, position, Quaternion.identity);
					numEnemies++;
				}
			}
		}
	}
}