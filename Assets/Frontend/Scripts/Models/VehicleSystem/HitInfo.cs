using UnityEngine;

namespace Frontend.Scripts.Models
{
	[System.Serializable]
	public class HitInfo
	{
		public RaycastHit rayHit;
		public Vector3 forwardDir;
		public Vector3 sidewaysDir;

		public Vector3 Point
		{
			get
			{
				return rayHit.point;
			}
		}
		public Vector3 Normal
		{
			get
			{
				return rayHit.normal;
			}
		}
	}
}
