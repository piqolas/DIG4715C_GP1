using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateLightProbes : MonoBehaviour
{
	private const string _lightProbeGroupObjectName = "Light Probe Group";

	[MenuItem("Tools/Generate Light Probe Groups/Low Resolution", false, 1)]
	private static void GenerateLow()
	{
		GameObject lightProbes;
		List<Vector3> probeLocations = new();

		if (GameObject.Find(_lightProbeGroupObjectName) != null)
			DestroyImmediate(GameObject.Find(_lightProbeGroupObjectName));

		lightProbes = new GameObject(_lightProbeGroupObjectName);
		LightProbeGroup probeGroup = lightProbes.AddComponent<LightProbeGroup>();
		// probeGroup.probePositions = probeLocations.ToArray();

		GameObject[] objectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject obj in objectsInScene)
			if (obj.isStatic)
				if (obj.TryGetComponent(out Renderer renderer))
					probeLocations.Add(renderer.bounds.max);

		probeGroup.probePositions = probeLocations.ToArray();
	}

	[MenuItem("Tools/Generate Light Probe Groups/Medium Resolution", false, 2)]
	private static void GenerateMedium()
	{
		GameObject lightProbes;
		List<Vector3> probeLocations = new();

		if (GameObject.Find(_lightProbeGroupObjectName) != null)
			DestroyImmediate(GameObject.Find(_lightProbeGroupObjectName));

		lightProbes = new GameObject(_lightProbeGroupObjectName);
		LightProbeGroup probeGroup = lightProbes.AddComponent<LightProbeGroup>();
		// probeGroup.probePositions = probeLocations.ToArray();

		GameObject[] objectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject obj in objectsInScene)
			if (obj.isStatic)
				if (obj.TryGetComponent(out Renderer renderer))
				{
					probeLocations.Add(renderer.bounds.max);
					probeLocations.Add(renderer.bounds.min);
				}

		probeGroup.probePositions = probeLocations.ToArray();
	}

	[MenuItem("Tools/Generate Light Probe Groups/High Resolution", false, 3)]
	private static void GenerateHigh()
	{
		GameObject lightProbes;
		List<Vector3> probeLocations = new();

		if (GameObject.Find(_lightProbeGroupObjectName) != null)
		{
			DestroyImmediate(GameObject.Find(_lightProbeGroupObjectName));
		}

		lightProbes = new GameObject(_lightProbeGroupObjectName);
		LightProbeGroup probeGroup = lightProbes.AddComponent<LightProbeGroup>();
		// probeGroup.probePositions = probeLocations.ToArray();

		GameObject[] objectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject obj in objectsInScene)
			if (obj.isStatic)
				if (obj.TryGetComponent(out Renderer renderer))
				{
					probeLocations.Add(renderer.bounds.max);
					probeLocations.Add(renderer.bounds.min);
				}

		int boundProbes = probeLocations.Count * 2;
		for (int i = 0; i < boundProbes; i++)
			probeLocations.Add(Vector3.Lerp(probeLocations[Random.Range(0, boundProbes / 2)], probeLocations[Random.Range(0, boundProbes / 2)], 0.5f));

		probeGroup.probePositions = probeLocations.ToArray();
	}

	[MenuItem("Tools/Generate Light Probe Groups/Very High Resolution", false, 4)]
	private static void GenerateVeryHigh()
	{
		GameObject lightProbes;
		List<Vector3> probeLocations = new();

		if (GameObject.Find(_lightProbeGroupObjectName) != null)
			DestroyImmediate(GameObject.Find(_lightProbeGroupObjectName));

		lightProbes = new GameObject(_lightProbeGroupObjectName);
		LightProbeGroup probeGroup = lightProbes.AddComponent<LightProbeGroup>();
		// probeGroup.probePositions = probeLocations.ToArray();

		GameObject[] objectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject obj in objectsInScene)
			if (obj.isStatic)
			{
				if (obj.TryGetComponent(out Renderer renderer))
				{
					probeLocations.Add(renderer.bounds.max);
					probeLocations.Add(renderer.bounds.min);
				}
			}

		int boundProbes = probeLocations.Count * 4;

		for (int i = 0; i < boundProbes; i++)
			probeLocations.Add(Vector3.Lerp(probeLocations[Random.Range(0, boundProbes / 4)], probeLocations[Random.Range(0, boundProbes / 4)], 0.5f));

		probeGroup.probePositions = probeLocations.ToArray();
	}
}
