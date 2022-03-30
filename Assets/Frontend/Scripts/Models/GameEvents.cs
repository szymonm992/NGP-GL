using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Models
{
	public static class GameEvents
	{
		public delegate void CameraBound(GameObjectContext context, Vector3 startingEulerAngles);

	}

}
