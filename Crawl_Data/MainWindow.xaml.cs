using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic.FileIO;

namespace Crawl_Data
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region properties
        ObservableCollection<MenuTreeItem> TreeItems;
        string HomePage = "https://hacom.vn";
        ObservableCollection<MenuTreeItem> TreeItemsProduct;
        HttpClient httpClient;
        HttpClientHandler handler;
        CookieContainer cookie = new CookieContainer();


        #endregion
        public MainWindow()
        {
            InitializeComponent();

            IniHttpClient();

            TreeItems = new ObservableCollection<MenuTreeItem>();
            TreeItemsProduct = new ObservableCollection<MenuTreeItem>();
            treeMain.ItemsSource = TreeItems;
        }

        #region methods
        void IniHttpClient()
        {
            handler = new HttpClientHandler
            {
                CookieContainer = cookie,
                ClientCertificateOptions = ClientCertificateOption.Automatic,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                UseDefaultCredentials = false
            };

            httpClient = new HttpClient(handler);

            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/63.4.154 Chrome/57.4.2987.154 Safari/537.36");
            /*
             * Header:
             * - Origin
             * - Host
             * - Referer
             * - :scheme
             * - accept
             * - Accept-Encoding
             * - Accept-Language
             * - User-Argent
             */


            httpClient.BaseAddress = new Uri(HomePage);
        }
        void AddItemIntoTreeViewItem(ObservableCollection<MenuTreeItem> root, MenuTreeItem node)
        {
            treeMain.Dispatcher.Invoke(new Action(() => {
                root.Add(node);
            }));
        }

        string CrawlDataFromURL(string url)
        {
            string html = "";

            html = WebUtility.HtmlDecode(httpClient.GetStringAsync(url).Result);

            //html = httpClient.PostAsync(url,new StringContent("")).Result.Content.ReadAsStringAsync().Result;

            return html;
        }

        void Crawl(string url)
        {
            List<ProductVariant> listProduct = new List<ProductVariant>();
            string htmlLearn = CrawlDataFromURL(url);
            var CourseList = Regex.Matches(htmlLearn, @"<div class=""p-component item"" data-id=(.*?)</span></span>", RegexOptions.Singleline);
            foreach (var course in CourseList)
            {
                //List<MenuTreeItem> listProduct = new List<MenuTreeItem>();
                var a= Regex.Match(course.ToString(), @"(?=<h3 class=""p-name "">).*?(?=</h3>)").Value;
                var removeHref= Regex.Match(a.ToString(), @"<a href=""/(.*?)"">").Value;
                var test = a.Replace(removeHref,"");
                //string courseName = Regex.Match(a.Replace(removeHref, "").ToString(), @"<h3 class=""p-name "">(.*?)</a>").Value.Replace("<h3 class=\"p-name \">", "").Replace("</a>", "");
                string linkCourse = Regex.Match(removeHref.ToString(), @"<a href=""(.*?)"">", RegexOptions.Singleline).Value.Replace("<a href=\"", "").Replace("\">","");

               // AddItemIntoTreeViewItem(TreeItems, item);

                string htmlCourse = CrawlDataFromURL(linkCourse);
                string sideBar = Regex.Match(htmlCourse, @"<div class=""container-2019 "">(.*?)<div class=""clearfix space2"">", RegexOptions.Singleline).Value;//.Replace(" ","");
                string name = Regex.Match(sideBar, @"<h1(.*?)</h1>", RegexOptions.Singleline).Value.Replace("<h1>","").Replace("</h1>","");
                var testsss= Regex.Match(sideBar, @"<span class=""sku"">(.*?)</span>", RegexOptions.Singleline).Value;
                string skuId = Regex.Match(sideBar, @"<span class=""sku"">(.*?)</span>",RegexOptions.Singleline).Value.Replace("<span class=\"sku\">", "").Replace("</span>", "");
                string price = Regex.Match(sideBar, @"<span class=""gia-km-cu"">(.*?)</span>", RegexOptions.Singleline).Value.Replace("<span class=\"gia-km-cu\">", "").Replace("₫</span>", "");
                
                ProductVariant productVariant = new ProductVariant();
                productVariant.Name = name;
                productVariant.SkuId= skuId;
                productVariant.Price = Convert.ToInt64(price.Replace(".",""));


                var scopedInfomation= Regex.Match(sideBar, @"<div class=""content_scroll_tab_2019""(.*?)</table>", RegexOptions.Singleline).Value;
                var listOPtionValueProduct = Regex.Matches(scopedInfomation, @"<tr>(.*?)</tr>", RegexOptions.Singleline);
                foreach (var lecture in listOPtionValueProduct)
                {
                    string option = Regex.Match(lecture.ToString(), @"<td width=""157"">(.*?)</td>", RegexOptions.Singleline).Value.Replace("</td>", "").Replace("<td width=\"157\">", "");
                    string value = Regex.Match(lecture.ToString(), @"<td width=""371"">(.*?)</td>", RegexOptions.Singleline).Value.Replace("<td width=\"371\">", "").Replace("</td>", "").Replace("&nbsp;","").Replace("<br>","; ");
                    if (!string.IsNullOrEmpty(option) && !string.IsNullOrEmpty(value))
                    {
                        Option_Value Subitem = new Option_Value();
                        Subitem.Option = option;
                        Subitem.Value = value;
                        productVariant.OptionValueColection.Add(Subitem);
                    }
                }


                var scopedImage = Regex.Match(sideBar, @"<ul id=""img_thumb""(.*?)</ul>", RegexOptions.Singleline).Value;
                var listImage = Regex.Matches(scopedImage, @"<li class='owl-thumb-item '(.*?)</li>", RegexOptions.Singleline);
                foreach (var lecture in listImage)
                {
                    string image = Regex.Match(lecture.ToString(), @"data-href=""(.*?)""", RegexOptions.Singleline).Value.Replace("data-href=\"", "").Replace("\"", "");
                    if (!string.IsNullOrEmpty(image))
                        productVariant.ImageCollection.Add(image);
                }
                listProduct.Add(productVariant);
            }
            
        }
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        #endregion
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //treeMain.Dispatcher.Invoke(new Action(()=> {
            //    Crawl("Learn");
            //}));
            Task t = new Task(() => { Crawl("/laptop-acer"); });
            t.Start();
        }

        private void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(wbMain, true);
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string url = HomePage + (sender as TextBlock).Tag.ToString();
            wbMain.Navigate(url);
            //Process.Start(url);
        }
    }
}
