using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
	public static class RendererExtensions
	{
		public static bool IsVisibleFromCamera(this Renderer renderer, Camera fromCamera)
		{
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(fromCamera);
			return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
		}
	}
}
