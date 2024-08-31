namespace Questripag;

public class Page<ItemModel>
{
    public IEnumerable<ItemModel> Items { get; private set; }
    public int TotalItemsCount { get; private set; }

    public Page(IEnumerable<ItemModel> items, int totalItemsCount)
    {
        Items = items;
        TotalItemsCount = totalItemsCount;
    }
}
