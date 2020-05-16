using UnityEngine;

/**
 * Class that makes up an individual cell in the maze.
 */
public class MazeCell
{
    // Determines whether the generation algorithm has visited this cell
	public bool visited = false;

    // The floor and walls that belong to this cell
	public GameObject northWall, southWall, eastWall, westWall, floor, ceiling;
}
