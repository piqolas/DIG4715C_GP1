using UnityEngine;

namespace piqey.Utilities.Extensions
{
	public static class Vector3Extensions
	{
		/// <summary>
		/// Returns the input <see cref="Vector3" />, but only after running
		/// <paramref name="vec"/>'s Y component through <see cref="Mathf.Abs" />
		/// </summary>
		public static Vector3 WithAbsY(this Vector3 vec) =>
			new(vec.x, Mathf.Abs(vec.y), vec.z);
	}
}
