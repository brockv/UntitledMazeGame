/**
 * Adapted from Richard Hawkes' tutorial found here: https://www.youtube.com/watch?v=IrO4mswO2o4
 */

using UnityEngine;

public class ProceduralNumberGenerator
{
	public static string key = "";
	private const string glyphs = "1234";
	public static int currentPosition;

	public void Start()
	{
		currentPosition = 0;
		key = GenerateRandomString(40);
	}

	/**
	 * Generates a random string of the given length using the characters in the glyph.
	 */
	private string GenerateRandomString(int length)
	{
		// Build the string and return it
		string randomString = "";
		for (int i = 0; i < length; i++)
		{
			randomString += glyphs[Random.Range(0, glyphs.Length)];
		}

		return randomString;
	}

	/**
	 * Gets the next number in the string.
	 */
	public int GetNextNumber()
	{
		// Grab the next number in the string and increment the index
		string currentNum = key.Substring(currentPosition++ % key.Length, 1);

		// If the index has reached the end of the string somehow, reset it
		if (currentPosition >= key.Length) currentPosition = 0; 

		return int.Parse(currentNum);
	}
}