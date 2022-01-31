namespace ValueOptimizer;

public readonly record struct ItemInfo(int Id, int Value, int Count)
{
    public int Sum => Value * Count;
}

public class ItemInfoLookup
{
    private readonly Dictionary<int, ItemInfo> _lookup;

    public ItemInfoLookup(IReadOnlyDictionary<int, int> itemValues, IReadOnlyDictionary<int, int> itemCounts)
    {
        _lookup = itemValues.ToDictionary(
            values => values.Key,
            values => new ItemInfo(values.Key, values.Value, itemCounts.TryGetValue(values.Key, out var count) ? count : 0));
    }

    public bool Lookup(int id, out ItemInfo itemInfo) => _lookup.TryGetValue(id, out itemInfo);

    public int Sum => _lookup.Values.Sum(item => item.Sum);
}