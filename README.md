# 📰 News Summarizer

A modern **Blazor web application** that leverages AI to summarize news articles quickly and efficiently. Just provide a news URL, and the app will generate a concise summary, categorize the news, and highlight key points.

---

## 🚀 Features

- 🔗 Input any news article URL  
- 🤖 AI-powered summarization using Grok API  
- 🏷️ Automatic news categorization  
- 📌 Key points extraction for quick insights  
- ⚡ Fast and responsive UI built with Blazor  

---

## 🛠️ Tech Stack

- **Frontend & Backend:** Blazor (.NET)  
- **Language:** C#  
- **AI Integration:** Grok AI API  
- **HTTP Handling:** HttpClient  

---

## 📸 How It Works

1. User enters a news article URL  
2. Application fetches the article content  
3. Sends content to Grok AI API  
4. Displays:
   - ✍️ Summary  
   - 🏷️ Category  
   - 📌 Key points  


---

## ⚙️ Installation & Setup

### Prerequisites

- .NET SDK (6 or later recommended)  
- Grok AI API Key  

### Steps

```bash
# Clone the repository
git clone https://github.com/your-username/news-summarizer.git

# Navigate to project folder
cd news-summarizer

# Run the project
dotnet run

```
---

## 🔑 Configuration

To run this project, you need to configure the Grok API key.

Update the `appsettings.json` file:

```json
{
  "GrokApi": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```
