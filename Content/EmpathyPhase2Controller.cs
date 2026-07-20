using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace flameout.Content
{
    public class EmpathyPhase2Controller
    {
        public static bool Active = false; 

        private readonly NPC _npc;
        private int _spawnTimer = 0;
        private int _healTimer = 0;
        private const int SPAWN_INTERVAL = 240; 
        private const int CIRCLES_PER_WAVE = 3;
        private const float SPAWN_SCATTER = 450f; 

        public EmpathyPhase2Controller(NPC npc) { _npc = npc; }

        public void Update()
        {
            if (!Active)
            {
                Active = true;
                
                if (!Main.dedServ && !SkyManager.Instance["flameout:EmpathySky"].IsActive())
                    SkyManager.Instance.Activate("flameout:EmpathySky");
            }

            
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                _healTimer++;
                if (_healTimer >= 60)
                {
                    _healTimer = 0;
                    _npc.life = System.Math.Min(_npc.life + 1000, _npc.lifeMax);
                    _npc.netUpdate = true;
                }

                
                _spawnTimer++;
                if (_spawnTimer >= SPAWN_INTERVAL)
                {
                    _spawnTimer = 0;
                    foreach (Player p in Main.player)
                    {
                        if (!p.active || p.dead) continue;
                        for (int i = 0; i < CIRCLES_PER_WAVE; i++)
                        {
                            Vector2 pos = p.Center + Main.rand.NextVector2Circular(SPAWN_SCATTER, SPAWN_SCATTER);
                            Projectile.NewProjectile(_npc.GetSource_FromAI(), pos, Vector2.Zero,
                                                     ModContent.ProjectileType<EmpathyPulseCircle>(), 0, 0f, Main.myPlayer);
                        }
                    }
                }
            }
        }

        
        public void End()
        {
            Active = false;
            _spawnTimer = 0;
            _healTimer = 0;

            if (!Main.dedServ && SkyManager.Instance["flameout:EmpathySky"].IsActive())
                SkyManager.Instance.Deactivate("flameout:EmpathySky");

            
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int type = ModContent.ProjectileType<EmpathyPulseCircle>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile pr = Main.projectile[i];
                    if (pr.active && pr.type == type) pr.Kill();
                }
            }
        }
    }
}
