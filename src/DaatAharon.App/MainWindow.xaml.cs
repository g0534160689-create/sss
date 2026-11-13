using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DaatAharon.Core.Links;
using DaatAharon.Infrastructure.Links;

namespace DaatAharon.App;

public partial class MainWindow : Window
{
    private readonly SqliteLinkRepository _repository;
    public ObservableCollection<LinkItem> Links { get; } = [];
    public LinkItem? SelectedLink { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DaatAharon");
        _repository = new SqliteLinkRepository(Path.Combine(dataDirectory, "links.db"));
        Loaded += MainWindow_Loaded;
        Closed += (_, _) => Browser.Dispose();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _repository.InitializeAsync();
            foreach (var link in await _repository.GetAllAsync()) Links.Add(link);
            await Browser.EnsureCoreWebView2Async();
            Browser.CoreWebView2.NavigationStarting += (sender, args) =>
            {
                if (!LinkAddress.TryNormalize(args.Uri, out _))
                {
                    args.Cancel = true;
                    BrowserStatus.Text = "הניווט נחסם: כתובת לא תקינה או פרוטוקול לא מאושר";
                }
            };
            Browser.CoreWebView2.NewWindowRequested += (sender, args) => args.Handled = true;
            BrowserStatus.Text = "מוכן — הקישורים נשמרים במחשב זה";
        }
        catch (Exception ex)
        {
            BrowserStatus.Text = "שגיאה באתחול. פרטים נרשמו בחלון ההודעה.";
            MessageBox.Show(ex.Message, "שגיאה באתחול", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void AddLink_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadForm(out var title, out var url, out var category)) return;
        try
        {
            var link = new LinkItem(Guid.NewGuid(), title, url, category, DateTimeOffset.UtcNow);
            await _repository.AddAsync(link);
            Links.Add(link);
            LinksList.SelectedItem = link;
            BrowserStatus.Text = "הקישור נשמר בהצלחה";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "לא ניתן לשמור את הקישור", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void UpdateLink_Click(object sender, RoutedEventArgs e)
    {
        if (LinksList.SelectedItem is not LinkItem selected)
        {
            MessageBox.Show("יש לבחור קישור לעדכון.", "עדכון קישור", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (!TryReadForm(out var title, out var url, out var category)) return;
        try
        {
            var updated = selected with { Title = title, Url = url, Category = category };
            await _repository.UpdateAsync(updated);
            var index = Links.IndexOf(selected);
            if (index >= 0) Links[index] = updated;
            LinksList.SelectedItem = updated;
            BrowserStatus.Text = "פרטי הקישור עודכנו ונשמרו";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "לא ניתן לעדכן את הקישור", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool TryReadForm(out string title, out string url, out string category)
    {
        title = TitleInput.Text.Trim();
        category = string.IsNullOrWhiteSpace(CategoryInput.Text) ? "כללי" : CategoryInput.Text.Trim();
        if (title.Length is 0 or > 120 || category.Length > 80 ||
            !LinkAddress.TryNormalize(UrlInput.Text, out url))
        {
            MessageBox.Show("יש להזין שם (1–120 תווים), קטגוריה (עד 80 תווים) וכתובת HTTP או HTTPS תקינה.", "בדיקת פרטים", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private async void DeleteLink_Click(object sender, RoutedEventArgs e)
    {
        if (LinksList.SelectedItem is not LinkItem selected) return;
        if (MessageBox.Show($"למחוק את '{selected.Title}'?", "אישור מחיקה", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            await _repository.DeleteAsync(selected.Id);
            Links.Remove(selected);
            TitleInput.Clear();
            UrlInput.Clear();
            BrowserStatus.Text = "הקישור נמחק";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "שגיאה במחיקה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void LinksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LinksList.SelectedItem is not LinkItem selected) return;
        TitleInput.Text = selected.Title;
        UrlInput.Text = selected.Url;
        CategoryInput.Text = selected.Category;
        if (Browser.CoreWebView2 is null) return;
        if (!LinkAddress.TryNormalize(selected.Url, out var url))
        {
            BrowserStatus.Text = "הקישור אינו תקין ולכן לא נפתח";
            return;
        }
        try
        {
            BrowserStatus.Text = $"טוען: {selected.Title}";
            Browser.Source = new Uri(url);
        }
        catch (Exception ex)
        {
            BrowserStatus.Text = "לא ניתן לפתוח את הקישור";
            MessageBox.Show(ex.Message, "שגיאה בפתיחת קישור", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
