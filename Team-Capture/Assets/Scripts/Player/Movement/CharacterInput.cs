using UnityEngine;

namespace Structs
{
	public struct CharacterInput
	{
		public CharacterInput(Vector2 dirs, Vector2 mouseDirs, bool jump, int inputNum)
		{
			Directions = dirs;
			MouseDirections = mouseDirs;
			Jump = jump;
			InputNum = inputNum;
		}

		public Vector2 Directions;
		public Vector2 MouseDirections;

		public bool Jump;

		public int InputNum;
	}
}