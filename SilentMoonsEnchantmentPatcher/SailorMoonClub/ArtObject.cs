// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.SkyrimSE
{
    public static partial class SailorMoonClub
    {
        public static class ArtObject
        {
            private static FormLink<IArtObjectGetter> Construct(uint id) => new FormLink<IArtObjectGetter>(ModKey.MakeFormKey(id));
            public static FormLink<IArtObjectGetter> AbsorbMoonCastPointFX01 => Construct(0x846);
            public static FormLink<IArtObjectGetter> AbsorbMoonTargetPointFX => Construct(0x847);
        }
    }
}