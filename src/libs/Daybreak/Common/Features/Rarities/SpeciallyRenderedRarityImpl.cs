using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoMod.Cil;

using ReLogic.Graphics;

using Terraria;
using Terraria.ModLoader;

namespace Daybreak.Common.Features.Rarities;

internal sealed class SpeciallyRenderedRarityImpl : GlobalItem
{
    public override void Load()
    {
        base.Load();

        IL_Main.DrawItemTextPopups += RenderSpecialRarities;
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (line is not { Mod: "Terraria", Name: "ItemName" })
        {
            return true;
        }

        if (RarityLoader.GetRarity(item.rare) is not ISpeciallyRenderedRarity rarity)
        {
            return true;
        }

        rarity.RenderRarityText(
            Main.spriteBatch,
            line.Font,
            line.Text,
            new Vector2(line.X, line.Y),
            line.Color,
            line.Rotation,
            line.Origin,
            line.BaseScale,
            SpriteEffects.None,
            line.MaxWidth,
            line.Spread,
            true
        );
        return false;
    }

    private static void RenderSpecialRarities(ILContext il)
    {
        var c = new ILCursor(il);

        // Tomat: This IL edit is awesome.  Match into the second loop that
        // renders the text and its outline, modify it to only run the last
        // operation to render the colored text of the popup text's rarity
        // is of our specially-rendered kind, and override the relevant
        // draw operations to use our custom API instead.

        // Get popup text.
        var popupTextLoc = -1;
        c.GotoNext(x => x.MatchLdsfld<Main>(nameof(Main.popupText)));
        c.GotoNext(x => x.MatchStloc(out popupTextLoc));

        // Jump to the end of the method.
        c.Next = null;

        ILLabel? secondLoopBegin = null;
        c.GotoPrev(x => x.MatchBlt(out _)); // Skip outer loop.
        c.GotoPrev(x => x.MatchBlt(out secondLoopBegin)); // Find inner loop.
        if (secondLoopBegin is null)
        {
            // Shouldn't be possible to reach here, but oh well.
            throw new InvalidOperationException("Failed to find second loop begin");
        }

        // Jump to the start of the body of the loop (convenience).
        c.GotoLabel(secondLoopBegin);

        // Here we can modify the loop to run only once.
        c.GotoPrev(MoveType.After, x => x.MatchLdcI4(0));

        c.EmitPop();
        c.EmitLdloc(popupTextLoc);
        c.EmitDelegate((PopupText text) => RarityLoader.GetRarity(text.rarity) is ISpeciallyRenderedRarity ? 4 : 0);

        // Go to the end again (could also manually match DrawString three
        // times).
        c.Next = null;

        // Actually take control of rendering by intercepting the call.
        c.GotoPrev(MoveType.Before, x => x.MatchCall(typeof(DynamicSpriteFontExtensionMethods), nameof(DynamicSpriteFontExtensionMethods.DrawString)));
        c.Remove();
        c.EmitLdloc(popupTextLoc); // So we can check the rarity.
        c.EmitDelegate((SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, PopupText popupText) =>
            {
                if (RarityLoader.GetRarity(popupText.rarity) is not ISpeciallyRenderedRarity rarity)
                {
                    spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, effects, layerDepth);
                    return;
                }

                rarity.RenderRarityText(spriteBatch, font, text, position, color, rotation, origin, new Vector2(scale), effects, -1, 2, false);
            }
        );
    }
}