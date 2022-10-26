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

namespace Crawl_Data
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region properties
        ObservableCollection<MenuTreeItem> TreeItems;
        string HomePage = "https://www.howkteam.com/";

        HttpClient httpClient;
        HttpClientHandler handler;
        CookieContainer cookie = new CookieContainer();


        #endregion
        public MainWindow()
        {
            InitializeComponent();

            IniHttpClient();

            TreeItems = new ObservableCollection<MenuTreeItem>();
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
            string htmlLearn = CrawlDataFromURL(url);
            var CourseList = Regex.Matches(htmlLearn, @"<div class=""info-course(.*?)</div>", RegexOptions.Singleline);
            foreach (var course in CourseList)
            {
                string courseName = Regex.Match(course.ToString(), @"(?=<h5>).*?(?=</h5>)").Value.Replace("<h5>", "");
                string linkCourse = Regex.Match(course.ToString(), @"'(.*?)'", RegexOptions.Singleline).Value.Replace("'", "");

                MenuTreeItem item = new MenuTreeItem();
                item.Name = courseName;
                item.URL = linkCourse;

                AddItemIntoTreeViewItem(TreeItems, item);

                string htmlCourse = CrawlDataFromURL(linkCourse);
                string sideBar = Regex.Match(htmlCourse, @"<div class=""sidebardetail"">(.*?)</ul>", RegexOptions.Singleline).Value;
                var listLecture = Regex.Matches(sideBar, @"<li(.*?)</li>", RegexOptions.Singleline);
                foreach (var lecture in listLecture)
                {
                    string lectureName = Regex.Match(lecture.ToString(), @"<li title=""(.*?)"">", RegexOptions.Singleline).Value.Replace("\">", "").Replace("<li title=\"", "");
                    string linkLecture = Regex.Match(lecture.ToString(), @"<a href=""(.*?)"">", RegexOptions.Singleline).Value.Replace("<a href=\"", "").Replace("\">", "");

                    MenuTreeItem Subitem = new MenuTreeItem();
                    Subitem.Name = lectureName;
                    Subitem.URL = linkLecture;
                    AddItemIntoTreeViewItem(item.Items, Subitem);
                }
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
            Task t = new Task(() => { Crawl("Learn"); });
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
