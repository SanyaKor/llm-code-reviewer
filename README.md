# 🧠 LLM Code Reviewer

**Code & Text Editor with LLM Support**  
Internal **LLM-based inspection and code quality tool** built with **.NET 9** and **Avalonia UI**.

Integrates with **OpenAI API** for intelligent code review, diff analysis, and automated improvement suggestions.

---

## ✨ Features

- 🪶 **Cross-platform:** Windows, macOS, and Linux  
- ⚙️ **Built on:** .NET 9 + Avalonia UI  
- 🤖 **LLM-powered analysis:** automatic diff inspection and change explanation  
- 🧩 **Syntax highlighting:** C#, Python, JSON, XML, HTML, and more  
- 💾 **Script manager:** add, rename, delete, and inspect scripts  
- 🧠 **Magic commands:**  
  - `analyze diffs` — analyze all modified files  
  - `summarize changes` — quick overview of project diffs  
  - `suggest fixes` — detect issues and propose corrections  
  - `review code` — AI-assisted code review for quality and consistency  

---

## 🏗️ Installation & Build Guide

### ✅ Prerequisites

Before you begin, make sure you have:

- [.NET 9 SDK](https://dotnet.microsoft.com/download) installed  
- (Optional) [Git](https://git-scm.com/downloads) for cloning the repository  
- Internet connection for restoring NuGet packages  
- A valid **OpenAI API key** set in your environment variables (for LLM features)

---

### ⚙️ Clone the Repository

```bash
git clone https://github.com/SanyaKor/llm-code-reviewer.git
cd llm-code-reviewer
```

### Build the Project
```bash
dotnet build -c Release
```

After successful build, you’ll find binaries here:
```bash
bin/Release/net9.0/
```

### Run the Application
```bash
dotnet run -c Release
```
