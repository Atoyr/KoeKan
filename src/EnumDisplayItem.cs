namespace Medoz.KoeKan;

public record EnumDisplayItem<T> (T Value, string DisplayName) where T : Enum;