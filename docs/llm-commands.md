# 🤖 LLM AI Bot Guide

## ⚙️ Requirements

To use the **AI Bot**, you must have:
- A valid **OpenAI API key**
- Internet connection
- API key set in your environment:
  ```bash
  export OPENAI_API_KEY="your_api_key_here"
  ```
  or on Windows PowerShell:
  ```powershell
  setx OPENAI_API_KEY "your_api_key_here"
  ```

The bot uses **GPT-5** for intelligent code reasoning and diff analysis.  
No manual copying or extra setup is required — everything happens automatically.

---

## 🧠 How It Works

- The bot **tracks all file changes** during your editing session.  
- On specific “magic commands,” it automatically gathers and analyzes diffs.  
- You can also chat naturally — ask for code explanations, suggestions, or improvements.  

🪶 *You don’t need to copy or paste your code — the bot reads it directly from memory.*

---

## 💬 Magic Commands

Magic commands are prefixed with `%` and trigger advanced AI-powered actions.

| Command | Description |
|----------|--------------|
| `%analyze diffs` | Analyze all current code changes and explain what was modified and why |
| `%suggest fixes` | Suggest possible fixes and improvements for the modified files |
| `%show deleted` | Display a summary of all deleted files or code sections |

Example usage:
```
%analyze diffs
```

---

## 🧩 Example Session

```
> %analyze changes
Bot:
File: Utils.cs  
- Removed old ValidateUser() method  
+ Added async version ValidateUserAsync()  
Summary: Improved async handling and reduced blocking calls.

> %suggest fixes
Bot:
Consider simplifying logic in DataParser.cs → Split nested loops into helper methods.
```

---

## 🖥️ Bot Interface Preview

Here’s how the **LLM Bot** window looks in the app:

<p align="center">
  <img src="/assets/aibot.png" alt="AI Bot Preview" width="50%">
</p>
---

## 📦 Notes

- There is still no batches system, take care of symbols amount

---

✅ **Ready to go!**  
Once your API key is configured, open the **AI Bot** from the top bar and start working —  
no setup, no copying — just pure **AI-assisted coding**.
