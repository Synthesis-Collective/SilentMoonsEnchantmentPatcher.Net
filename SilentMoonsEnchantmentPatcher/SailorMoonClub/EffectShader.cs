// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.SkyrimSE
{
    public static partial class SailorMoonClub
    {
        public static class EffectShader
        {
            private static FormLink<IEffectShaderGetter> Construct(uint id) => new FormLink<IEffectShaderGetter>(ModKey.MakeFormKey(id));
            public static FormLink<IEffectShaderGetter> AbsorbMoonFXS => Construct(0x800);
            public static FormLink<IEffectShaderGetter> EnchMoonFXShader => Construct(0x869);
            public static FormLink<IEffectShaderGetter> MoonFireFXShader => Construct(0x86b);
        }
    }
}
