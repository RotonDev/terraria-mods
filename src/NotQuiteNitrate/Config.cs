using JetBrains.Annotations;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Tomat.TML.Mod.NotQuiteNitrate;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class Config : ModConfig
{
    public static Config Instance => ModContent.GetInstance<Config>();

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public bool BetterLiquidSlopes { get; set; }
}