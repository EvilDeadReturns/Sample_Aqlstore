using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sample_Aqlstore.Services;

public class PersonStore
{
    private readonly string _filePath;

    public PersonStore()
    {
        string dir = Path.Combine(FileSystem.AppDataDirectory, "AqlData");
        Directory.CreateDirectory(dir); // Ensure folder exists

        _filePath = Path.Combine(dir, "people.aql");

        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, Header());
    }

    private string Header() =>
        """
        aql_version: 1.0
        entity: person
        ---
        """;

    // READ
    public List<PersonEntry> GetAll()
    {
        var list = new List<PersonEntry>();
        if (!File.Exists(_filePath)) return list;

        var lines = File.ReadAllLines(_filePath);
        PersonEntry? current = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("- id:"))
            {
                current = new PersonEntry
                {
                    Id = int.Parse(line.Split(':')[1].Trim())
                };
                list.Add(current);
            }
            else if (current != null)
            {
                if (line.StartsWith("  name:"))
                    current.Name = line.Split(':')[1].Trim().Trim('"');
                else if (line.StartsWith("  age:"))
                    current.Age = int.Parse(line.Split(':')[1].Trim());
                else if (line.StartsWith("  city:"))
                    current.City = line.Split(':')[1].Trim().Trim('"');
            }
        }

        return list;
    }

    // CREATE
    public void Add(PersonEntry p)
    {
        p.Id = NextId();

        File.AppendAllText(_filePath, $"""
        - id: {p.Id}
          name: "{p.Name}"
          age: {p.Age}
          city: "{p.City}"

        """);
    }

    // UPDATE
    public void Update(PersonEntry p)
    {
        var all = GetAll();
        var index = all.FindIndex(x => x.Id == p.Id);
        if (index == -1) return;

        all[index] = p;
        Rewrite(all);
    }

    // DELETE
    public void Delete(int id)
    {
        var all = GetAll();
        all.RemoveAll(x => x.Id == id);
        Rewrite(all);
    }

    // Read entire AQL file as text
    public string ReadRawAql() =>
        File.Exists(_filePath)
            ? File.ReadAllText(_filePath)
            : "AQL store is empty. No records yet.";

    private int NextId()
    {
        var all = GetAll();
        return all.Count == 0 ? 1 : all.Max(x => x.Id) + 1;
    }

    private void Rewrite(List<PersonEntry> items)
    {
        var lines = new List<string> { Header() };

        foreach (var p in items)
        {
            lines.Add($"""
            - id: {p.Id}
              name: "{p.Name}"
              age: {p.Age}
              city: "{p.City}"
            """);
        }

        File.WriteAllText(_filePath, string.Join(Environment.NewLine, lines));
    }

}
