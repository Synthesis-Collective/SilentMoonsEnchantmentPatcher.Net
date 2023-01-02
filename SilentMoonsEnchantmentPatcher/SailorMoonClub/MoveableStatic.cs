// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.SkyrimSE
{
    public static partial class SailorMoonClub
    {
        public static class MoveableStatic
        {
            private static FormLink<IMoveableStaticGetter> Construct(uint id) => new FormLink<IMoveableStaticGetter>(ModKey.MakeFormKey(id));
            public static FormLink<IMoveableStaticGetter> SilentMoonLightPillar => Construct(0x8ad);
        }
    }
}