# 🔧 Configuration — OpenAI API Key & Model Setup

## 🧠 API Key Setup

To use the **AI Bot** and LLM-powered features, you need a valid **OpenAI API key**.

### 🪟 Windows (PowerShell)
```powershell
setx OPEN_AI_API_KEY "your_api_key_here"
```

### 🍎 macOS / 🐧 Linux
```bash
export OPEN_AI_API_KEY="your_api_key_here"
```

Verify:
```bash
echo $Env:OPEN_AI_API_KEY   # Windows
echo $OPEN_AI_API_KEY       # macOS / Linux
```

---

## ⚙️ Supported Models

By default, the app uses **GPT-5** through the OpenAI API.  
However, you can easily switch to **any other model** (e.g. GPT-4, Claude, Gemini, Mistral, etc.)  
by modifying just a few lines in your LLM initialization code.

---

## 🧩 Custom LLM Setup

If you want to use another provider or model, simply adjust your initialization.

You can also adapt this to work with custom APIs  
(e.g. local inference endpoints or self-hosted open models).

---

✅ Once your API key is set, restart the app — the **LLM features** will work automatically.
