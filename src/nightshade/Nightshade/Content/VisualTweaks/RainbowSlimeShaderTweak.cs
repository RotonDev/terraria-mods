using System.Diagnostics;

using Daybreak.Common.Assets;

using JetBrains.Annotations;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nightshade.Core.Attributes;

using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Nightshade.Content.VisualTweaks;

/// <summary>
///     Modifies the rendering of Rainbow Slimes to have a cooler rainbow effect
///     derived from the Queen Slime shader.
/// </summary>
[Autoload(Side = ModSide.Client)]
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class RainbowSlimeShaderTweak : GlobalNPC
{
    private const int rainbow_slime = NPCID.RainbowSlime;

    private static AssetReplacer.Handle<Texture2D>? rainbowSlimeTextureHandle;

    [InitializedInLoad]
    private static MiscShaderData? rainbowSlimeShaderData;

    public override void Load()
    {
        base.Load();

#pragma warning disable CS0618 // Type or member is obsolete
        rainbowSlimeShaderData = new MiscShaderData(Main.PixelShaderRef, "QueenSlime");
        {
            rainbowSlimeShaderData.UseImage1(Assets.Images.QueenSlimePalettes.RainbowSlime.Asset);
            rainbowSlimeShaderData.UseImage2("Images/Extra_179");
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == rainbow_slime;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Debug.Assert(rainbowSlimeShaderData is not null);

        rainbowSlimeTextureHandle?.Dispose();
        rainbowSlimeTextureHandle = AssetReplacer.Npc(
            rainbow_slime,
            Assets.Images.NPCs.RainbowSlime.Asset.Value
        );

        // Since we override the rendering entirely, just make the NPC white.
        npc.color = Color.White;

        if (!npc.IsABestiaryIconDummy)
        {
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null,
                Main.Transform
            );
        }

        rainbowSlimeShaderData.Apply();

        return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        rainbowSlimeTextureHandle?.Dispose();

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        if (!npc.IsABestiaryIconDummy)
        {
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.Transform
            );
        }

        base.PostDraw(npc, spriteBatch, screenPos, drawColor);
    }
}