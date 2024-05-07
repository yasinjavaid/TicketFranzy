using System;
using System.Linq;

using TMPro;

public static class TextMeshProExtensions
{

    public static void AddOptionsFromEnum<T>(this TMP_Dropdown dropdown, bool clearPreviousOptions) where T : Enum
    {
        if (clearPreviousOptions) dropdown.ClearOptions();
        dropdown.AddOptions(Enum.GetNames(typeof(T)).ToList());
    }
}
