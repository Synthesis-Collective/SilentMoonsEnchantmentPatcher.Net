// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.SkyrimSE
{
    public static partial class SailorMoonClub
    {
        public static class Global
        {
            private static FormLink<IGlobalGetter> Construct(uint id) => new FormLink<IGlobalGetter>(ModKey.MakeFormKey(id));
            public static FormLink<IGlobalGetter> MoonPhase_WaxingGibbous => Construct(0x87a);
            public static FormLink<IGlobalGetter> MoonPhase_Current => Construct(0x872);
            public static FormLink<IGlobalGetter> MoonPhase_Full => Construct(0x873);
            public static FormLink<IGlobalGetter> MoonPhase_WaningGibbous => Construct(0x874);
            public static FormLink<IGlobalGetter> MoonPhase_SecondQuarter => Construct(0x875);
            public static FormLink<IGlobalGetter> MoonPhase_WaningCrescent => Construct(0x876);
            public static FormLink<IGlobalGetter> MoonPhase_New => Construct(0x877);
            public static FormLink<IGlobalGetter> MoonPhase_WaxingCrescent => Construct(0x878);
            public static FormLink<IGlobalGetter> MoonPhase_FirstQuarter => Construct(0x879);
            public static FormLink<IGlobalGetter> Hour_Moonrise => Construct(0x87e);
            public static FormLink<IGlobalGetter> Hour_Moonset => Construct(0x87f);
            public static FormLink<IGlobalGetter> LunarForgeState => Construct(0x8ac);
        }
    }
}
