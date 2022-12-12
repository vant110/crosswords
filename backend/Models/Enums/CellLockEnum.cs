namespace Crosswords.Models.Enums
{
    [Flags]
    public enum CellLockEnum : byte
    {
        None,
        Horizontally,
        Vertically

    }
}
