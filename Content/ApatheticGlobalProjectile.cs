using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace flameout.Content
{
	public class ApatheticGlobalProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public bool ApatheticShot;

		public const int DebuffTime = 60 * 5; 

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (ApatheticShot)
				target.AddBuff(ModContent.BuffType<ApatheticCurse>(), DebuffTime);
		}

		public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
		{
			if (ApatheticShot)
				target.AddBuff(ModContent.BuffType<ApatheticCurse>(), DebuffTime);
		}


		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_Parent parent
				&& parent.Entity is Projectile parentProj
				&& parentProj.GetGlobalProjectile<ApatheticGlobalProjectile>().ApatheticShot)
			{
				ApatheticShot = true;
			}
		}
	}
}