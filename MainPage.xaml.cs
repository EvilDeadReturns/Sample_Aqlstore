using Aquila;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Text;

namespace Sample_Aqlstore;

public partial class MainPage : ContentPage
{
    private readonly AqlStore _store;
    private readonly ObservableCollection<PersonEntry> _items = new();
    private PersonEntry? _selected;

    public MainPage()
    {
        InitializeComponent();
        try
        {
            string dataPath = Path.Combine(FileSystem.AppDataDirectory, "AqlData");
            _store = new AqlStore(dataPath, "people", 2000);
            ListView.ItemsSource = _items;
            LoadData();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error in Constructor", ex.Message, "OK");
        }
    }


    private void LoadData()
    {
        _items.Clear();
        var keys = _store.GetKeysByPrefix("person.");

        foreach (var key in keys)
        {
            string? raw = _store.Get(key);
            if (raw != null)
            {
                // Parse raw AQL into PersonEntry
                var person = ParsePersonAql(raw);
                if (person != null)
                    _items.Add(person);
            }
        }
    }

    private PersonEntry? ParsePersonAql(string aql)
    {
        // Simple parser for our person AQL format
        var lines = aql.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        var p = new PersonEntry();
        foreach (var line in lines)
        {
            if (line.StartsWith("id:")) p.Id = int.Parse(line.Split(':')[1].Trim());
            else if (line.StartsWith("name:")) p.Name = line.Split(':')[1].Trim().Trim('"');
            else if (line.StartsWith("age:")) p.Age = int.Parse(line.Split(':')[1].Trim());
            else if (line.StartsWith("city:")) p.City = line.Split(':')[1].Trim().Trim('"');
        }
        return p.Id > 0 ? p : null;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = e.CurrentSelection.FirstOrDefault() as PersonEntry;
        if (_selected == null) return;

        IdEntry.Text = _selected.Id.ToString();
        NameEntry.Text = _selected.Name;
        AgeEntry.Text = _selected.Age.ToString();
        CityEntry.Text = _selected.City;
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            int id = NextId();
            _selected = new PersonEntry
            {
                Id = id,
                Name = NameEntry.Text ?? "",
                Age = int.Parse(AgeEntry.Text ?? "0"),
                City = CityEntry.Text ?? ""
            };
        }
        else
        {
            _selected.Name = NameEntry.Text ?? "";
            _selected.Age = int.Parse(AgeEntry.Text ?? "0");
            _selected.City = CityEntry.Text ?? "";
        }

        string aql = BuildPersonAql(_selected);
        _store.Set($"person.{_selected.Id}", aql); // store in AqlStore

        ClearInputs();
        LoadData();
    }

    private string BuildPersonAql(PersonEntry p)
    {
        return $"""
        id: {p.Id}
        name: "{p.Name}"
        age: {p.Age}
        city: "{p.City}"
        """;
    }
  


    private void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_selected == null) return;

        _store.Delete($"person.{_selected.Id}");
        ClearInputs();
        LoadData();
    }

    private async void OnViewAqlClicked(object sender, EventArgs e)
    {
        var keys = _store.GetKeysByPrefix(""); // all keys
        var sb = new StringBuilder();

        foreach (var key in keys)
        {
            var value = _store.Get(key);
            sb.AppendLine($"{key} = {value}");
        }

        string display = sb.Length > 0 ? sb.ToString() : "No records available.";
        await Navigation.PushAsync(new AqlViewerPage(display));
    }
    // Async version of LoadData
    private void LoadDataAsync()
    {
        var keys = _store.GetKeysByPrefix("person.");
        var tempList = new List<PersonEntry>();

        foreach (var key in keys)
        {
            string? raw = _store.Get(key);
            if (raw != null)
            {
                var person = ParsePersonAql(raw);
                if (person != null)
                    tempList.Add(person);
            }
        }

        // Update UI on main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _items.Clear();
            foreach (var p in tempList)
                _items.Add(p);
        });
    }



    private void ShowToast(string message)
    {
#if ANDROID
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, Android.Widget.ToastLength.Short).Show();
        });
#else
    MainThread.BeginInvokeOnMainThread(async () =>
    {
        await DisplayAlert("Debug", message, "OK");
    });
#endif
    }





    private int NextId()
    {
        ShowToast("NextId() called"); // Debug

        if (_items.Count == 0)
        {
            ShowToast("No items yet, returning 1");
            return 1;
        }

        int maxId = _items.Max(x => x.Id);
        ShowToast($"Max ID found: {maxId}");

        return maxId + 1;
    }

    private async void OnGenerateTestDataClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Confirm",
            "This will generate 500 test records. Continue?",
            "Yes",
            "No"
        );

        if (!confirm) return;

        // 🔒 Get starting ID ONCE
        int startId = GetNextIdFromStore();

        await Task.Run(() =>
        {
            var random = new Random();

            for (int i = 0; i < 500; i++)
            {
                int id = startId + i;

                string aql = $"""
id: {id}
name: "Person {id}"
age: {random.Next(18, 60)}
city: "City {random.Next(1, 101)}"
""";

                // Safe sequential write
                _store.Set($"person.{id}", aql);

                // Android safety throttle
                if (i % 50 == 0)
                    Thread.Sleep(5);
            }
        });

        LoadData();

        await DisplayAlert(
            "Done",
            "500 records generated successfully.\nTotal count updated correctly.",
            "OK"
        );
    }

    private int GetNextIdFromStore()
    {
        var keys = _store.GetKeysByPrefix("person.");
        if (!keys.Any())
            return 1;

        return keys
            .Select(k => int.Parse(k.Replace("person.", "")))
            .Max() + 1;
    }

    private void GenerateTestData(int count = 5)
    {
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            try
            {
                int id = NextId();
                ShowToast($"Generating Person {id}"); // Debug

                string aql = $"""
id: {id}
name: "Person {id}"
age: {random.Next(18, 60)}
city: "City {random.Next(1, 101)}"
""";

                _store.Set($"person.{id}", aql);
            }
            catch (Exception ex)
            {
                ShowToast($"Error generating record {i}: {ex.Message}");
            }
        }
    }



    private async void OnDownloadAqlClicked(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            await DisplayAlert("Error", "Please select a record first", "OK");
            return;
        }

        string fileName = $"person_{_selected.Id}.aql";
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        string aqlContent = BuildPersonAql(_selected);

        File.WriteAllText(filePath, aqlContent);

        await DisplayAlert("AQL Downloaded", $"File saved at:\n{filePath}", "OK");
    }

    private void ClearInputs()
    {
        _selected = null;
        IdEntry.Text = "";
        NameEntry.Text = "";
        AgeEntry.Text = "";
        CityEntry.Text = "";
        ListView.SelectedItem = null;
    }
}
