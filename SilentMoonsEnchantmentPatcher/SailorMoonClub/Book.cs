// Autogenerated by https://github.com/Mutagen-Modding/Mutagen.Bethesda.FormKeys

using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.FormKeys.SkyrimSE
{
    public static partial class SailorMoonClub
    {
        public static class Book
        {
            private static FormLink<IBookGetter> Construct(uint id) => new FormLink<IBookGetter>(ModKey.MakeFormKey(id));
            public static FormLink<IBookGetter> SMC_FoodWarehouseNote => Construct(0x924);
            public static FormLink<IBookGetter> SMC_ParanoiaBook => Construct(0xb3b);
        }
    }
}