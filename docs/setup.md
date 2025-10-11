e# ⚙️ Build & Run Guide

## 🔹 1️⃣ Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Internet connection (for NuGet restore)
- (Optional) [Git](https://git-scm.com/)

Check SDK version:
```bash
dotnet --version
```

---

## 🔹 2️⃣ Clone & Restore

```bash
git clone https://github.com/yourusername/llm-code-reviewer.git
cd llm-code-reviewer
dotnet restore
```

---

## 🔹 3️⃣ Build

Build in Release mode:
```bash
dotnet build -c Release
```

Output:
```
bin/Release/net9.0/
```

---

## 🔹 4️⃣ Run

Run the app directly:
```bash
dotnet run -c Release
```

---

## 🔹 5️⃣ Publish (Standalone Binary)

Windows:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

macOS / Linux:
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

Result:
```
bin/Release/net9.0/<runtime>/publish/
```

---

## 🔹 6️⃣ Clean (Optional)

```bash
dotnet clean
```

---

✅ Done — the app is now built and ready to use.
