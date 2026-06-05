using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using MonoMod;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/SpecialBumper")]

public class SpecialBumper : Bumper
{
    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    public void Entity_Update() { }

    private bool ignoreCoreMode;

    private bool wobble;

    private bool refillDash;

    private float dashCooldown;

    private float respawnTime;

    private string launchMode;
    public SpecialBumper(EntityData data, Vector2 offset) : base(data, offset)
	{
        ignoreCoreMode = data.Bool("ignoreCoreMode");
        wobble = data.Bool("wobble");
        refillDash = data.Bool("refillDash");
        dashCooldown = data.Float("dashCooldown");
        respawnTime = data.Float("respawnTime");
        launchMode = data.String("launchMode");
        if (ignoreCoreMode)
        {
            Remove(Get<CoreModeListener>());
        }
        Remove(Get<PlayerCollider>());
        Add(new PlayerCollider(OnPlayerCustom));
        
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (!ignoreCoreMode)
        {
            fireMode = SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
        }
        spriteEvil.Visible = fireMode;
        sprite.Visible = !fireMode;
    }
    public override void Update()
    {
        Entity_Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                light.Visible = true;
                bloom.Visible = true;
                sprite.Play("on");
                spriteEvil.Play("on");
                if (!fireMode)
                {
                    Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                }
            }
        }
        else if (base.Scene.OnInterval(0.05f))
        {
            float num = Calc.Random.NextAngle();
            ParticleType type = (fireMode ? P_FireAmbience : P_Ambience);
            float direction = (fireMode ? (-MathF.PI / 2f) : num);
            float length = (fireMode ? 12 : 8);
            SceneAs<Level>().Particles.Emit(type, 1, base.Center + Calc.AngleToVector(num, length), Vector2.One * 2f, direction);
        }
        if (wobble)
        {
            UpdatePosition();
        } else
        {
            NoWiggleUpdatePosition();
        }
    }

    private void NoWiggleUpdatePosition()
    {
        Position = new Vector2((float)(double)anchor.X, (float)(double)anchor.Y);
    }

    private void OnPlayerCustom(Player player)
    {
        if (fireMode)
        {
            if (!SaveData.Instance.Assists.Invincible)
            {
                Vector2 vector = (player.Center - base.Center).SafeNormalize();
                hitDir = -vector;
                hitWiggler.Start();
                Audio.Play("event:/game/09_core/hotpinball_activate", Position);
                respawnTimer = respawnTime;
                player.Die(vector);
                SceneAs<Level>().Particles.Emit(P_FireHit, 12, base.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
            }
        }
        else if (respawnTimer <= 0f)
        {
            if ((base.Scene as Level).Session.Area.ID == 9)
            {
                Audio.Play("event:/game/09_core/pinballbumper_hit", Position);
            }
            else
            {
                Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);
            }
            respawnTimer = respawnTime;
            Vector2 vector2;
            if (launchMode == "ReverseSpeed")
            {
                vector2 = ReverseSpeedLaunch(player, Position);
            } else if (launchMode == "8Way")
            {
                vector2 = EightwayLaunch(player, Position);
            } else
            {
                vector2 = NormalLaunch(player, Position);
            }
            sprite.Play("hit", restart: true);
            spriteEvil.Play("hit", restart: true);
            light.Visible = false;
            bloom.Visible = false;
            SceneAs<Level>().DirectionalShake(vector2, 0.15f);
            SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
            SceneAs<Level>().Particles.Emit(P_Launch, 12, base.Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
        }
    }

    private Vector2 ReverseSpeedLaunch(Player player, Vector2 from)
    {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        global::Celeste.Celeste.Freeze(0.1f);
        Vector2 vector = (player.Center - from).SafeNormalize(-Vector2.UnitY);
        float num = Vector2.Dot(vector, Vector2.UnitY);
        if (num <= 0.65f && num >= -0.55f)
        {
            vector.Y = 0f;
            vector.X = Math.Sign(vector.X);
        }
        SlashFx.Burst(player.Center, player.Speed.Angle());
        player.Speed.X *= -1;
        player.Speed.Y *= -1;
        if (!player.Inventory.NoRefills && refillDash)
        {
            player.RefillDash();
        }
        player.RefillStamina();
        player.dashCooldownTimer = dashCooldown;
        player.StateMachine.State = 0;
        return vector;

    }

    private Vector2 EightwayLaunch(Player player, Vector2 from)
    {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        global::Celeste.Celeste.Freeze(0.1f);
        player.launchApproachX = null;
        Vector2 vector = (player.Center - from).SafeNormalize(-Vector2.UnitY);
        //Logger.Debug($"{nameof(BreadHelperModule)}/{nameof(SpecialBumper)}", vector.Y.ToString());
        float num = vector.Y;
        if (Math.Abs(num) > 0.9f)
        {
            vector.X = 0f;
            vector.Y = Math.Sign(vector.Y);
        } else if (num <= 0.55f && num >= -0.55f)
        {
            vector.Y = 0f;
            vector.X = Math.Sign(vector.X);
        } else
        {
            vector.Y = Math.Sign(vector.Y) * 0.7f;
            vector.X = Math.Sign(vector.X) * 0.7f;
        }
        player.Speed = 280f * vector;
        if (player.Speed.Y <= 50f)
        {
            player.Speed.Y = Math.Min(-150f, player.Speed.Y);
            player.AutoJump = true;
        }
        if (player.Speed.X != 0f)
        {
            if (Input.MoveX.Value == Math.Sign(player.Speed.X))
            {
                player.explodeLaunchBoostTimer = 0f;
                player.Speed.X *= 1.2f;
            }
            else
            {
                player.explodeLaunchBoostTimer = 0.01f;
                player.explodeLaunchBoostSpeed = player.Speed.X * 1.2f;
            }
        }
        SlashFx.Burst(player.Center, player.Speed.Angle());
        if (!player.Inventory.NoRefills && refillDash)
        {
            player.RefillDash();
        }
        player.RefillStamina();
        player.dashCooldownTimer = dashCooldown;
        player.StateMachine.State = 7;
        return vector;
    }
    private Vector2 NormalLaunch(Player player, Vector2 from)
    {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        global::Celeste.Celeste.Freeze(0.1f);
        player.launchApproachX = null;
        Vector2 vector = (player.Center - from).SafeNormalize(-Vector2.UnitY);
        float num = Vector2.Dot(vector, Vector2.UnitY);
        if (num <= 0.65f && num >= -0.55f)
        {
            vector.Y = 0f;
            vector.X = Math.Sign(vector.X);
        }
        player.Speed = 280f * vector;
        if (player.Speed.Y <= 50f)
        {
            player.Speed.Y = Math.Min(-150f, player.Speed.Y);
            player.AutoJump = true;
        }
        if (player.Speed.X != 0f)
        {
            if (Input.MoveX.Value == Math.Sign(player.Speed.X))
            {
                player.explodeLaunchBoostTimer = 0f;
                player.Speed.X *= 1.2f;
            }
            else
            {
                player.explodeLaunchBoostTimer = 0.01f;
                player.explodeLaunchBoostSpeed = player.Speed.X * 1.2f;
            }
        }
        SlashFx.Burst(player.Center, player.Speed.Angle());
        if (!player.Inventory.NoRefills && refillDash)
        {
            player.RefillDash();
        }
        player.RefillStamina();
        player.dashCooldownTimer = dashCooldown;
        player.StateMachine.State = 7;
        return vector;
        
    }


}
