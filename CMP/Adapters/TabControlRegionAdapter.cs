using System.Windows.Controls;

namespace CMP.Adapters;

public class TabControlRegionAdapter : RegionAdapterBase<TabControl>
{
    public TabControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
        : base(regionBehaviorFactory)
    {
    }

    protected override void Adapt(IRegion region, TabControl regionTarget)
    {
        region.Views.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (item is TabItem)
                    {
                        regionTarget.Items.Add(item);
                    }
                    else
                    {
                        var tabItem = new TabItem();
                        tabItem.Content = item;
                        // You could set header properties from attached properties or metadata
                        regionTarget.Items.Add(tabItem);
                    }

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    if (item is TabItem tabItem)
                        regionTarget.Items.Remove(tabItem);
                    else
                        // Find and remove the wrapper TabItem
                        foreach (TabItem existingItem in regionTarget.Items)
                            if (existingItem.Content == item)
                            {
                                regionTarget.Items.Remove(existingItem);
                                break;
                            }
        };
    }

    protected override IRegion CreateRegion()
    {
        return new AllActiveRegion();
    }
}