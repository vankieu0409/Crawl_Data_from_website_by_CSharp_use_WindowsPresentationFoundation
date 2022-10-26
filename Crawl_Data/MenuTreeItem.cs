using System.Collections.ObjectModel;

namespace Crawl_Data;

public class MenuTreeItem
{
    public string Name { get; set; }
    public string URL { get; set; }
    public ObservableCollection<MenuTreeItem> Items { get; set; }
    public MenuTreeItem()
    {
        this.Items = new ObservableCollection<MenuTreeItem>();
    }
}