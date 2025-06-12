using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateLightProbes : MonoBehaviour
{
	[MenuItem("Tools/Generate Light Probe Groups/Low Resolution", false, 1)]
	private static void generateLow()
	{

		GameObject lightProbes;
		List<Vector3> probeLocations = new();

		if (GameObject.Find("Light Probe Group") != null)
			DestroyImmediate(GameObject.Find("Light Probe Group"));

		lightProbes = new GameObject("Light Probe Group");
		lightProbes.AddComponent<LightProbeGroup>();
		lightProbes.GetComponent<LightProbeGroup>().probePositions = probeLocations.ToArray();

		GameObject[] objectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject obj in objectsInScene)
			if (obj.isStatic)
				if (obj.GetComponent<Renderer>() != null)
					probeLocations.Add(obj.GetComponent<Renderer>().bounds.max);

		lightProbes.GetComponent<LightProbeGroup>().probePositions = probeLocations.ToArray();
	}

	[MenuItem("Tools/Generate Light Probe Groups/Medium Resolution", false, 2)]
	private static void generateMedium()
	{

		GameObject lightProbes;
		List<Vector3> probeLocations = new();

		if (GameObject.Find("Light Probe Group") != null)
			DestroyImmediate(GameObject.Find("Light Probe Group"));

		lightProbes = new GameObject("Light Probe Group");
		lightProbes.AddComponent<LightProbeGroup>();
		lightProbes.GetComponent<LightProbeGroup>().probePositions = probeLocations.ToArray();

		GameObject[] objectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject obj in objectsInScene)
			if (obj.isStatic)
				if (obj.GetComponent<Renderer>() != null)
				{
					probeLocations.Add(obj.GetComponent<Renderer>().bounds.max);
					probeLocations.Add(obj.GetComponent<Renderer>().bounds.min);
				}

		lightProbes.GetComponent<LightProbeGroup>().probePositions = probeLocations.ToArray();
	}

	[MenuItem("Tools/Generate Light Probe Groups/High Resolution", false, 3)]
	private static void generateHigh()
	{

		GameObject lightProbes;
		List<Vector3> probeLocations = new();

		if (GameObject.Find("Light Probe Group") != null)
		{
			DestroyImmediate(GameObject.Find("Light Probe Group"));
		}

		lightProbes = new GameObject("Light Probe Group");
		lightProbes.AddComponent<LightProbeGroup>();
		lightProbes.GetComponent<LightProbeGroup>().probePositions = probeLocations.ToArray();

		GameObject[] objectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject obj in objectsInScene)
			if (obj.isStatic)
				if (obj.GetComponent<Renderer>() != null)
				{
					probeLocations.Add(obj.GetComponent<Renderer>().bounds.max);
					probeLocations.Add(obj.GetComponent<Renderer>().bounds.min);
				}

		int boundProbes = probeLocations.Count * 2;
		for (int i = 0; i < boundProbes; i++)
			probeLocations.Add(Vector3.Lerp(probeLocations[Random.Range(0, boundProbes / 2)], probeLocations[Random.Range(0, boundProbes / 2)], 0.5f));

		lightProbes.GetComponent<LightProbeGroup>().probePositions = probeLocations.ToArray();
	}

	[MenuItem("Tools/Generate Light Probe Groups/Very High Resolution", false, 4)]
	private static void generateVeryHigh()
	{

		GameObject lightProbes;
		List<Vector3> probeLocations = new();

		if (GameObject.Find("Light Probe Group") != null)
			DestroyImmediate(GameObject.Find("Light Probe Group"));

		lightProbes = new GameObject("Light Probe Group");
		lightProbes.AddComponent<LightProbeGroup>();
		lightProbes.GetComponent<LightProbeGroup>().probePositions = probeLocations.ToArray();

		GameObject[] objectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject obj in objectsInScene)
			if (obj.isStatic)
			{
				if (obj.GetComponent<Renderer>() != null)
				{
					probeLocations.Add(obj.GetComponent<Renderer>().bounds.max);
					probeLocations.Add(obj.GetComponent<Renderer>().bounds.min);
				}
			}

		int boundProbes = probeLocations.Count * 4;

		for (int i = 0; i < boundProbes; i++)
			probeLocations.Add(Vector3.Lerp(probeLocations[Random.Range(0, boundProbes / 4)], probeLocations[Random.Range(0, boundProbes / 4)], 0.5f));

		lightProbes.GetComponent<LightProbeGroup>().probePositions = probeLocations.ToArray();
	}
}
