using UnityEngine;

// Copy meshes from children into the parent's Mesh.
// CombineInstance stores the list of meshes. These are combined
// and assigned to the attached Mesh.

namespace piqey.Utilities
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class CombineMeshes : MonoBehaviour
	{
		void Start()
		{
			MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
			CombineInstance[] combine = new CombineInstance[meshFilters.Length];

			int i = 0;

			while (i < meshFilters.Length)
			{
				combine[i].mesh = meshFilters[i].sharedMesh;
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
				meshFilters[i].gameObject.SetActive(false);

				i++;
			}

			Mesh mesh = new();
			mesh.CombineMeshes(combine);
			transform.GetComponent<MeshFilter>().sharedMesh = mesh;
			transform.gameObject.SetActive(true);
		}
	}
}
