// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.SkyrimSE
{
    public static partial class SailorMoonClub
    {
        public static class Flora
        {
            private static FormLink<IFloraGetter> Construct(uint id) => new FormLink<IFloraGetter>(ModKey.MakeFormKey(id));
            public static FormLink<IFloraGetter> SMC_PearlOyster => Construct(0xbb7);
        }
    }
}