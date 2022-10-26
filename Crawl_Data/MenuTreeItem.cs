using System.Collections.ObjectModel;

namespace Crawl_Data;

public class MenuTreeItem
{
    public string Name { get; set; }
    public string URL { get; set; }
    public ObservableCollection<ProductVariant> Items { get; set; }
    public MenuTreeItem()
    {
        this.Items = new ObservableCollection<ProductVariant>();
    }
}