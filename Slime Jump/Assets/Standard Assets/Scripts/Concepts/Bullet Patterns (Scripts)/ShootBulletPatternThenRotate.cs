using SlimeJump;
using Extensions;
using UnityEngine;

[CreateAssetMenu]
public class ShootBulletPatternThenRotate : BulletPattern
{
	public BulletPattern bulletPattern;
	[MakeConfigurable]
	public float rotate;
	
	public override Bullet[] Shoot (Transform spawner, Bullet bulletPrefab)
	{
		Bullet[] output = bulletPattern.Shoot(spawner, bulletPrefab);
		spawner.up = spawner.up.Rotate(rotate);
		return output;
	}
}