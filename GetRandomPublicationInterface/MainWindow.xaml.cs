using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
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

namespace GetRandomPublicationInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        List<Publication> _publications;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _publications = GetPublicationsFromTxtFile();
        }

        private string _publicationDate = "";
        public string PublicationDate
        {
            private set
            {
                try
                {
                    _publicationDate = value;
                    NotifyPropertyChanged("PublicationDate");
                }
                catch (Exception)
                {
                }
            }
            get
            {
                return _publicationDate;
            }
        }

        private string _publicationContent = "";
        public string PublicationContent
        {
            private set
            {
                try
                {
                    _publicationContent = value;
                    NotifyPropertyChanged("PublicationContent");
                }
                catch (Exception)
                {
                }
            }
            get
            {
                return _publicationContent;
            }
        }

        private string _pictureLink = "";
        public string PictureLink
        {
            private set
            {
                try
                {
                    _pictureLink = value;
                    NotifyPropertyChanged("PictureLink");
                }
                catch (Exception)
                {
                }
            }
            get
            {
                return _pictureLink;
            }
        }

        private string _publicationLink = "";
        public string PublicationLink
        {
            private set
            {
                try
                {
                    if (value == "http://facebook.com#")
                    {
                        _publicationLink = "";
                        NotifyPropertyChanged("PublicationLink");
                    }
                    else
                    {
                        _publicationLink = value;
                        NotifyPropertyChanged("PublicationLink");
                    }
                }
                catch (Exception)
                {
                }
                
            }
            get
            {
                return _publicationLink;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            catch (Exception)
            {
            }
        }

        private List<Publication> GetPublicationsFromTxtFile()
        {
            try
            {
                var publications = JsonConvert.DeserializeObject<List<Publication>>(File.ReadAllText("allPublications.json"));
                publications = publications.Where(x =>
                {
                    DateTime limitOldPublications = DateTime.ParseExact("03/02/2015 04:10", "g", new CultureInfo("fr-FR"));
                    DateTime datePublication = DateTime.ParseExact(x.PublicationDate, "g", new CultureInfo("fr-FR"));
                    return DateTime.Compare(datePublication, limitOldPublications) > 0;
                }).ToList();
                return publications;
            }
            catch (Exception)
            {
                return new List<Publication>();
            }

        }

        private void GenerateRandomPublication(object sender, RoutedEventArgs e)
        {
            try
            {
                int rndPublicationIndex = new Random().Next(0, _publications.Count - 1);
                var randomPublication = _publications[rndPublicationIndex];
                PublicationDate = randomPublication.PublicationDate;
                PublicationContent = randomPublication.PublicationContent;
                PictureLink = randomPublication.PictureLink;
                PublicationLink = randomPublication.PublicationLink;
            }
            catch (Exception)
            {
            }
        }

        private void GenerateKeyWord(object sender, RoutedEventArgs e)
        {
            try
            {
                InputBox inputBox = new InputBox();
                if (inputBox.ShowDialog() == true)
                {
                    var keyword = inputBox.Keyword.ToLower();
                    //Filter to get only publications with the specified keyword
                    var publicationsFiltered = _publications.Where(x => x.PublicationContent.ToLower().Contains(keyword)).ToList();
                    if (publicationsFiltered.Count == 0)
                    {
                        PublicationDate = "";
                        PublicationContent = "";
                        PictureLink = "";
                        PublicationLink = "";
                    }
                    else
                    {
                        int rndPublicationIndex = new Random().Next(0, publicationsFiltered.Count - 1);
                        var randomPublication = publicationsFiltered[rndPublicationIndex];
                        PublicationDate = randomPublication.PublicationDate;
                        PublicationContent = randomPublication.PublicationContent;
                        PictureLink = randomPublication.PictureLink;
                        PublicationLink = randomPublication.PublicationLink;
                    }

                }
            }
            catch (Exception)
            {
            }
        }

        private void GeneratePublicationsHtml(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.ShowDialog();
                    path = dialog.SelectedPath;
                }

                if (path == "") return;

                var doc = new HtmlDocument();

                // create html document
                var html = HtmlNode.CreateNode("<html><head><meta charset=\"UTF-8\"></head><body></body></html>");
                doc.DocumentNode.AppendChild(html);
                var body = doc.DocumentNode.SelectSingleNode("/html/body");

                bool isNewPublication = true;
                bool isAutor = false;
                foreach (var currentPublication in _publications)
                {
                    var datePublication = HtmlNode.CreateNode("<p>" + currentPublication.PublicationDate + "</p>");
                    body.AppendChild(datePublication);

                    if (currentPublication.PublicationLink != "")
                    {
                        var publicationLink = HtmlNode.CreateNode("<p>" + currentPublication.PublicationLink + "</p>");
                        body.AppendChild(publicationLink);
                    }

                    if (currentPublication.PictureLink != "")
                    {
                        var pictureLink = HtmlNode.CreateNode("<img src=\"" + currentPublication.PictureLink + "\">");
                        body.AppendChild(pictureLink);
                    }

                    var content = HtmlNode.CreateNode("<p>" + WebUtility.HtmlEncode(currentPublication.PublicationContent) + "</p>");
                    body.AppendChild(content);

                    for (var _ = 0; _ < 5; ++_)
                    {
                        var newLine = HtmlNode.CreateNode("<Br>");
                        body.AppendChild(newLine);
                    }
                }

                const string fileName = "PublicationsJoieEtre.html";
                string fullPath = path + "\\" + fileName;
                File.Delete(fullPath);
                File.WriteAllText(fullPath, html.OuterHtml);

                MessageBoxResult result = MessageBox.Show("Les publications ont été enregistrées dans le fichier html à l'emplacement suivant: " + fullPath,
                                              "Confirmation",
                                              MessageBoxButton.OK);
            }
            catch (Exception)
            {
            }
        }

        private void PublicationHyperLink(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(PublicationLink));
                e.Handled = true;
            }
            catch (Exception)
            {
            }
        }

        private void CopyPublication(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(PublicationContent);
            }
            catch (Exception)
            {
            }
        }
    }
}
