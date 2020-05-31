/**
 * Adapted from Richard Hawkes' tutorial found here: https://www.youtube.com/watch?v=IrO4mswO2o4
 */

using UnityEngine;

public class HuntAndKillMazeAlgorithm : MazeAlgorithm
{	

	private int currentRow = 0;
	private int currentColumn = 0;

	private bool courseComplete = false;

	ProceduralNumberGenerator pg = new ProceduralNumberGenerator();

	/**
	 * Constructor.
	 */
	public HuntAndKillMazeAlgorithm(MazeCell[,] mazeCells) : base(mazeCells) { }	

	/**
	 * Entry function into the maze creation.
	 */
	public override void CreateMaze()
	{
		// Generate the random string used to create the maze with		
		pg.Start();

		// Start creating the maze
		HuntAndKill();
	}

	/**
	 * Carves a path out of the cells, creating a maze.
	 */
	private void HuntAndKill()
	{
		// Set the current cell as visited
		mazeCells[currentRow, currentColumn].visited = true;

		// Run until the maze is complete
		while (!courseComplete)
		{
			Kill();
			Hunt();
		}
	}

	/**
	 * Destroys walls adjacent to the current cell. Runs until it hits a dead end.
	 */
	private void Kill()
	{
		while (RouteStillAvailable(currentRow, currentColumn))
		{
			// Grab the next value in the random string
			int direction = pg.GetNextNumber();

			if (direction == 1 && CellIsAvailable(currentRow - 1, currentColumn))
			{
				// North
				DestroyWallIfItExists(mazeCells[currentRow, currentColumn].northWall);
				DestroyWallIfItExists(mazeCells[currentRow - 1, currentColumn].southWall);
				mazeCells[currentRow, currentColumn].northWall = null;
				mazeCells[currentRow - 1, currentColumn].southWall = null;
				currentRow--;
			}
			else if (direction == 2 && CellIsAvailable(currentRow + 1, currentColumn))
			{
				// South
				DestroyWallIfItExists(mazeCells[currentRow, currentColumn].southWall);
				DestroyWallIfItExists(mazeCells[currentRow + 1, currentColumn].northWall);
				mazeCells[currentRow, currentColumn].southWall = null;
				mazeCells[currentRow + 1, currentColumn].northWall = null;
				currentRow++;
			}
			else if (direction == 3 && CellIsAvailable(currentRow, currentColumn + 1))
			{
				// East
				DestroyWallIfItExists(mazeCells[currentRow, currentColumn].eastWall);
				DestroyWallIfItExists(mazeCells[currentRow, currentColumn + 1].westWall);
				mazeCells[currentRow, currentColumn].eastWall = null;
				mazeCells[currentRow, currentColumn + 1].westWall = null;
				currentColumn++;
			}
			else if (direction == 4 && CellIsAvailable(currentRow, currentColumn - 1))
			{
				// West
				DestroyWallIfItExists(mazeCells[currentRow, currentColumn].westWall);
				DestroyWallIfItExists(mazeCells[currentRow, currentColumn - 1].eastWall);
				mazeCells[currentRow, currentColumn].westWall = null;
				mazeCells[currentRow, currentColumn - 1].eastWall = null;
				currentColumn--;
			}

			mazeCells[currentRow, currentColumn].visited = true;
		}
	}
	/**
	 * Finds the next unvisited cell with an adjacent  visited cell. If it can't find any, it sets courseComplete to true.
	 */
	private void Hunt()
	{
		// Set this to true and see if we can prove otherwise below
		courseComplete = true;

		// Iterate through the maze data
		for (int r = 0; r < mazeRows; r++)
		{
			for (int c = 0; c < mazeColumns; c++)
			{
				if (!mazeCells[r, c].visited && CellHasAnAdjacentVisitedCell(r,c))
				{
					// Yep, we found something so definitely do another Kill cycle
					courseComplete = false;
					
					// Set the current row and column to this cell
					currentRow = r;
					currentColumn = c;
					
					// Destroy an adjacent wall and set the cell as visited
					DestroyAdjacentWall(currentRow, currentColumn);
					mazeCells[currentRow, currentColumn].visited = true;

					// Exit the function
					return;
				}
			}
		}
	}

	/**
	 * Checks if there are still routes available.
	 */
	private bool RouteStillAvailable(int row, int column)
	{
		// Initialize a counter
		int availableRoutes = 0;

		// North
		if (row > 0 && !mazeCells[row-1,column].visited)
		{
			availableRoutes++;
		}

		// South
		if (row < mazeRows - 1 && !mazeCells [row + 1, column].visited)
		{
			availableRoutes++;
		}

		// West
		if (column > 0 && !mazeCells[row,column-1].visited)
		{
			availableRoutes++;
		}

		// East
		if (column < mazeColumns-1 && !mazeCells[row,column+1].visited)
		{
			availableRoutes++;
		}

		// Return true if there are routes available, false otherwise
		return availableRoutes > 0;
	}

	private bool CellIsAvailable(int row, int column)
	{
		if (row >= 0 && row < mazeRows && column >= 0 && column < mazeColumns && !mazeCells[row, column].visited)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/**
	 * Destroys the given wall object if it exists.
	 */
	private void DestroyWallIfItExists(GameObject wall)
	{
		if (wall != null)
		{
			GameObject.Destroy(wall);
		}
	}

	private bool CellHasAnAdjacentVisitedCell(int row, int column)
	{
		int visitedCells = 0;

		// Look 1 row up (north) if we're on row 1 or greater
		if (row > 0 && mazeCells[row - 1, column].visited)
		{
			visitedCells++;
		}

		// Look one row down (south) if we're the second-to-last row (or less)
		if (row < (mazeRows-2) && mazeCells [row + 1, column].visited)
		{
			visitedCells++;
		}

		// Look one row left (west) if we're column 1 or greater
		if (column > 0 && mazeCells[row, column - 1].visited)
		{
			visitedCells++;
		}

		// Look one row right (east) if we're the second-to-last column (or less)
		if (column < (mazeColumns-2) && mazeCells[row, column + 1].visited)
		{
			visitedCells++;
		}

		// return true if there are any adjacent visited cells to this one
		return visitedCells > 0;
	}

	/**
	 * Destroys walls adjacent to the current cell, determined by the row and column.
	 */
	private void DestroyAdjacentWall(int row, int column)
	{
		// Run until we've destroyed walls next to the current cell
		bool wallDestroyed = false;
		while (!wallDestroyed)
		{
			// Grab the next value in the random string
			int direction = pg.GetNextNumber();

			// North
			if (direction == 1 && row > 0 && mazeCells[row - 1, column].visited)
			{
				DestroyWallIfItExists(mazeCells[row, column].northWall);
				DestroyWallIfItExists(mazeCells[row - 1, column].southWall);
				mazeCells[row, column].northWall = null;
				mazeCells[row - 1, column].southWall = null;
				wallDestroyed = true;
			}
			// South
			else if (direction == 2 && row < (mazeRows-2) && mazeCells[row + 1, column].visited)
			{
				DestroyWallIfItExists(mazeCells[row, column].southWall);
				DestroyWallIfItExists(mazeCells[row + 1, column].northWall);
				mazeCells[row, column].southWall = null;
				mazeCells[row + 1, column].northWall = null;
				wallDestroyed = true;
			}
			// West
			else if (direction == 3 && column > 0 && mazeCells[row, column-1].visited)
			{
				DestroyWallIfItExists(mazeCells[row, column].westWall);
				DestroyWallIfItExists(mazeCells[row, column-1].eastWall);
				mazeCells[row, column].westWall = null;
				mazeCells[row, column - 1].eastWall = null;
				wallDestroyed = true;
			}
			// East
			else if (direction == 4 && column < (mazeColumns-2) && mazeCells[row, column+1].visited)
			{
				DestroyWallIfItExists(mazeCells[row, column].eastWall);
				DestroyWallIfItExists(mazeCells[row, column+1].westWall);
				mazeCells[row, column].eastWall = null;
				mazeCells[row, column + 1].westWall = null;
				wallDestroyed = true;
			}
		}
	}
}
