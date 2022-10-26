using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Crawl_Data;

public class ProductVariant
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string SkuId { get; set; }
    public Int64 Price { get; set; }

    public ICollection<Option_Value> OptionValueColection { get; set; }
    public ICollection<string> ImageCollection { get; set; }

    public ProductVariant()
    {
        OptionValueColection = new Collection<Option_Value>();
        ImageCollection = new Collection<string>();
    }

}