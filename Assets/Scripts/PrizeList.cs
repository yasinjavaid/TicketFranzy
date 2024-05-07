using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class PrizeList
{
    private static Prize[] AllPrizes => _allPrizes ??= Resources.LoadAll<Prize>("Prizes");
    private static Prize[] _allPrizes;

    public static int Count => AllPrizes.Length;

    public static IEnumerable<Prize> All => AllPrizes;

    public static IEnumerable<Prize> Filtered(PrizeCategory category)
        => AllPrizes.Where(prize => prize.Category == category);

    public static IEnumerable<Prize> Filtered(IntRange priceRange)
        => AllPrizes.Where(prize => prize.Tickets >= priceRange.Min && prize.Tickets <= priceRange.Max);

    public static IEnumerable<Prize> Filtered(PrizeCategory category, IntRange priceRange)
        => AllPrizes.Where(prize => prize.Category == category).Where(prize => prize.Tickets >= priceRange.Min && prize.Tickets <= priceRange.Max);
}