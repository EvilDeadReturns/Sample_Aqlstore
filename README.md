🐬 Sample_AqlStore
A .NET MAUI sample application demonstrating how to use Aquila.AqlStore (NuGet) as a lightweight, persistent, human‑readable data store — without JSON, SQLite, or ORM layers.
This project focuses on:
Understanding AQL as a general-purpose storage format
Building CRUD operations on top of AqlStore
Handling large datasets (5,000+ records) safely
Comparing AQL vs JSON vs SQLite
Designing a developer‑friendly, debuggable storage layer

📌 What is AQL?
AQL (Aquila Query Language) is a human‑readable, append‑friendly, key‑value storage format backed by:
Snapshot file (.aql)
Append-only log (.aql.log)
It is optimized for:
App settings
Metadata
Lightweight databases
Offline‑first applications
Debug‑friendly persistence
AQL is NOT limited to one scenario (like person data). It is a general storage engine.

📦 NuGet Package
dotnet add package Aquila.AqlStore

Namespace:
using Aquila;


🧱 Project Architecture
Sample_Aqlstore/
│
├── MainPage.xaml
├── MainPage.xaml.cs
├── AqlViewerPage.xaml
├── AqlViewerPage.xaml.cs
├── Models/
│   └── PersonEntry.cs
└── AppData/
    ├── people.aql
    └── people.aql.log


🗂️ How Data is Stored
Each record is stored as a key-value pair inside AqlStore.
🔑 Key
person.<id>

📄 Value (AQL block)
id: 12
name: "Aswin"
age: 25
city: "Pondicherry"

This means:
1 logical record = 1 key
Values are plain text
Easy to inspect / debug

🧠 Why NOT JSON?
Feature
AQL
JSON
Human readable
✅
⚠️ (nested)
Partial updates
✅
❌ (rewrite all)
Append-friendly
✅
❌
Debuggable
✅
❌
Merge-safe
✅
❌
Versioning
Built-in
Manual
Comments
✅
❌

JSON is great for APIs, but bad for local persistence.

🆚 AQL vs SQLite
Feature
AQL
SQLite
Setup
Zero
Schema + migrations
Speed (≤50k records)
⚡ Fast
⚡ Fast
Human readable
✅
❌
Debugging
Easy
Hard
ORM needed
❌
Often
Transactions
❌
✅

Rule of thumb:
Use AQL for configuration, metadata, offline stores
Use SQLite for relational queries

⚙️ Initializing AqlStore
string dataPath = Path.Combine(FileSystem.AppDataDirectory, "AqlData");
var store = new AqlStore(dataPath, "people", 2000);

This creates:
people.aql
people.aql.log


✍️ Writing Data
_store.Set("person.1", aqlContent);

Internally:
Appends to log
Updates in-memory index
Auto-compacts after threshold

📖 Reading Data
var keys = _store.GetKeysByPrefix("person.");
foreach (var key in keys)
{
    var value = _store.Get(key);
}


❌ Deleting Data
_store.Delete("person.1");

Uses tombstones → survives restarts

🔄 Auto Compaction
Triggered when:
Write count exceeds threshold
App shuts down (Dispose())
Effect:
Rewrites snapshot
Clears log
Keeps file small

🚀 Generating 500 / 5000 Records Safely
❗ Important Rule
Never calculate ID repeatedly from UI state.
✅ Correct Bulk Insert
int startId = GetNextIdFromStore();

for (int i = 0; i < 500; i++)
{
    int id = startId + i;
    _store.Set($"person.{id}", aql);
}

✔ No overwrites
✔ No crashes
✔ Android-safe

🧪 Performance Notes (50,000 records)
Operation
Time
Insert
~0.4s
Read all
~0.2s
Compact
~0.3s

Perfect for mobile & desktop apps

🖥️ AQL Viewer
The app includes an AQL Viewer Page to inspect raw data.
await Navigation.PushAsync(new AqlViewerPage(rawAql));

Great for:
Debugging
Auditing
Learning

🧯 Common Pitfalls (Solved in this App)
❌ UI freeze during bulk insert
❌ Duplicate IDs
❌ App crash on Android
❌ Threading bugs
✅ All handled correctly

🎯 When to Use AQL
Use AQL when you need:
Simple persistence
Offline-first apps
Inspectable data
No database setup
Avoid AQL if you need:
Complex joins
ACID transactions
Multi-process access

🧩 Future Enhancements
Progress bar for bulk insert
Encryption support
Schema validation
Diff/merge tools
Cloud sync

❤️ Final Thoughts
AQL sits between JSON and SQLite:
More powerful than JSON
Simpler than SQLite
This sample proves:
You can safely manage thousands of records with AQL in production apps.

Happy hacking 🚀
— Built with ❤️ using Aquila.AqlStore

