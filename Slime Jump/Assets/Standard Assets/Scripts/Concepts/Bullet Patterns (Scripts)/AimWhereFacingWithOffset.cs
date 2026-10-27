using Extensions;
using UnityEngine;

namespace SlimeJump
{
	[CreateAssetMenu]
	public class AimWhereFacingWithOffset : AimWhereFacing
	{
		public float shootOffset;
		
		public override Vector2 GetShootDirection (Transform spawner)
		{
			return base.GetShootDirection(spawner).Rotate(shootOffset);
		}
	}
}