using UnityEngine;

public partial class GuardAIController : MonoBehaviour
{
	public enum GuardState
	{
		Spawn,
		Search,
		Patrol,
		Chase
	}
}
