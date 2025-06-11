using System;
using UnityEngine;

namespace piqey.Utilities
{
	[Serializable]
	public struct TimeSince : IEquatable<TimeSince>
	{
		[SerializeField]
		private float time;

		public readonly float Absolute => time;

		public readonly float Relative => this;

		public static implicit operator float(TimeSince ts) =>
			Time.time - ts.time;

		public static implicit operator TimeSince(float ts)
		{
			TimeSince result = default;
			result.time = Time.time - ts;

			return result;
		}

		public static bool operator ==(TimeSince left, TimeSince right) =>
			left.Equals(right);

		public static bool operator !=(TimeSince left, TimeSince right) =>
			!(left == right);

		public override readonly bool Equals(object obj) =>
			obj is TimeSince o && Equals(o);

		public readonly bool Equals(TimeSince o) =>
			time == o.time;

		public override readonly int GetHashCode() =>
			time.GetHashCode();
	}
}
