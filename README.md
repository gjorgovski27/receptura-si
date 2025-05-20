# 🧑‍🍳 Receptura.si — Cooking Assistant

**Receptura.si** is a modern web application that helps users manage, rate, and share their favorite recipes — with features like user authentication, personal profiles, pantry-based search, and more. Built using ASP.NET Core Razor Pages and powered by SQLite, this project is designed with simplicity and functionality in mind.

---

## 🌐 Live Demo

> 🚀 [Hosted on Azure](https://recepturasi.azurewebsites.net/)  

---

## 📸 Features

- 🔐 User Sign-Up and Login (with hashed passwords)
- 👨‍🍳 Personal Profile Page with saved data
- 📜 Create, View, Edit & Delete Recipes
- ⭐ Rate recipes (1–5 stars)
- ❤️ Add/Remove Recipes from Favourites
- 🔍 Live Search with Suggestions
- 🧺 Pantry-Based Ingredient Search (coming soon)
- 📰 News Article Section (Create & Display)
- 💬 Comment system with delete support
- 📦 Fully functional API backend (RESTful)

---

## 🛠️ Tech Stack

| Technology     | Purpose                            |
|----------------|------------------------------------|
| ASP.NET Core   | Backend + Razor Pages frontend     |
| Entity Framework Core | Database ORM (SQLite3)      |
| SQLite         | Lightweight embedded DB            |
| Bootstrap 5    | Styling and responsive layout      |
| JavaScript (vanilla) | Frontend interactivity      |
| Azure App Service | Deployment platform             |
| GitHub         | Version Control & CI/CD            |

---

## ⚙️ Local Setup

1. **Clone the Repo**

   ```bash
   git clone https://github.com/gjorgovski27/receptura-si.git
   cd receptura-si

   
2. **Run the App**

  dotnet build
  
  dotnet ef database update
  
  dotnet run

3. **Access**
   Navigate to: https://localhost:5204
  (adjust the port based on your config)

## 📁 Folder Structure
CookingAssistantAPI/
├── Controllers/          → API controllers (Users, Recipes, etc.)

├── Data/                 → EF Core models and DbContext

├── Models/               → View models and data transfer objects

├── Pages/                → Razor Pages (.cshtml and code-behind)

├── wwwroot/              → Static assets (CSS, images)

├── appsettings.json      → Configurations (DB, JWT, etc.)

└── Program.cs            → App entry point and middleware config


## 🔒 Security
- JWT-based Authentication
- Passwords hashed using PasswordHasher
- CORS enabled with policy
- Logging to Azure Diagnostics

## 🚀 Azure Deployment
1. Use Deployment Center in App Service to connect your GitHub repo.
2. Push changes to the master branch.
3. Azure automatically builds and deploys the app using your .csproj and published files. 

## 👨‍💻 Author
Mihail Gjorgovski (63230413)
GitHub: @gjorgovski27


## 📄 License
MIT License © 2025
You are free to use, modify, and distribute this app.

