/**
 * Adapted from Richard Hawkes' tutorial found here: https://www.youtube.com/watch?v=IrO4mswO2o4
 */

public abstract class MazeAlgorithm
{

	protected MazeCell[,] mazeCells;
	protected int mazeRows, mazeColumns;

	protected MazeAlgorithm(MazeCell[,] mazeCells) : base()
	{
		this.mazeCells = mazeCells;
		mazeRows = mazeCells.GetLength(0);
		mazeColumns = mazeCells.GetLength(1);
	}

	public abstract void CreateMaze();
}
