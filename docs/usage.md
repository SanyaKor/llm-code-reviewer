# 💡 Usage Guide

## 🗂️ File Management

The **LLM Code Reviewer** editor allows you to manage your scripts directly inside the app.

You can:
- ➕ **Add** new files  
- ✏️ **Edit** existing ones  
- 🧾 **Rename** scripts  
- ❌ **Delete** unnecessary files  

Each script is stored locally and automatically saved to disk.

---

## 🧠 Syntax Highlighting

When a file has a recognized extension, the editor automatically applies syntax highlighting.

Supported formats:
- `.cs` — C#  
- `.py` — Python  
- `.json` — JSON  
- `.xml` — XML  
- `.html`, `.css`, `.js`, `.md` — common text formats

Unknown extensions are opened as plain text.

---

## 💾 Saving and Storage

All scripts are saved automatically to your user profile folder.

| OS | Location |
|----|-----------|
| 🪟 **Windows** | `C:\Users\<username>\AppData\Roaming\LLMCodeReviewer\scripts.json` |
| 🍎 **macOS** | `/Users/<username>/Library/Application Support/LLMCodeReviewer/scripts.json` |
| 🐧 **Linux** | `/home/<username>/.config/LLMCodeReviewer/scripts.json` |

The editor will create the folder automatically if it doesn’t exist.  
Your data is persistent between sessions.

---


## 🤖 LLM AI Bot Integration

You can launch the built-in **AI Assistant** directly from the editor.  
It provides:
- Code analysis based on recent changes  
- Diff summarization and issue detection  
- Context-aware insights for refactoring  

📄 See more: [LLM Commands & Bot Usage →](docs/llm-commands.md)


## 🖥️ Editor Preview

Below is an example of the code editor interface with syntax highlighting enabled:

![Editor Preview](/assets/editor.png)

---

✅ **That’s it!**  
You’re ready to use **LLM Code Reviewer** as your AI-powered, cross-platform code editor.