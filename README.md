# 🎶 Moodify – Emotion-Based Music Recommendation

Moodify is a **.NET 9 Web API** integrated with a **Python FastAPI ML service** for real-time emotion detection.
It uses facial expression recognition to recommend music that matches the user’s mood.

🔗 GitHub Repo: [Moodify](https://github.com/Ahmed-0sama/Moodify)

---

## 🏗 Architecture

* **.NET 9 Web API**

  * Handles authentication (JWT), user management, and email notifications.
  * Stores user data and playlists in **SQL Server**.
  * Connects to the Python ML service for emotion detection.

* **Python FastAPI Service**

  * Runs a YOLO-based model (`best.pt`) to detect facial expressions.
  * Returns JSON responses with the detected emotion.

---

## 🔐 Authentication & Security

* **JWT Authentication** for protected API routes.
* **Email Service** with SMTP (supports Gmail or any SMTP provider).

---

## ⚙️ Installation & Setup

### 1. Clone the Repo

```bash
git clone https://github.com/Ahmed-0sama/Moodify.git
cd Moodify
```

---

### 2. Backend (.NET 9)

1. Open the solution in **Visual Studio 2022** or run from CLI:

   ```bash
   cd backend
   dotnet restore
   dotnet run
   ```

2. Update `appsettings.json` with your own credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MoodifyDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "super_secret_key",
    "Issuer": "Moodify",
    "Audience": "MoodifyUsers"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  },
  "PythonService": {
    "BaseUrl": "http://127.0.0.1:8000"
  }
}
```

3. Run **EF Core migrations** to set up the SQL Server database:

```bash
dotnet ef database update
```

---

### 3. Python ML Service

1. Navigate to the ML folder:

```bash
cd ml-service
pip install -r requirements.txt
uvicorn main:app --reload
```

2. Test health endpoint:

```bash
curl http://127.0.0.1:8000/health
```

---

### 4. Testing with Postman

* Import the provided **Postman collection** (in `postman/` folder).
* Set the environment variables for:

  * `base_url` → `https://localhost:5001` (or your hosted API)
  * `auth_token` → (fetched after login)
* Test endpoints such as:

  * User registration & login
  * Uploading an image to detect emotion
  * Getting music recommendations

---

## 📨 Email Service

* Used for user verification & password reset.
* Configurable in `appsettings.json`.
* Supports Gmail, SendGrid, or custom SMTP.

---

## 🔮 Next Steps

* Integrate Spotify API for real playlists.
* Store music history per user.
* Add frontend (Angular/React).

---

## 👨‍💻 Contributing

1. Fork the repo.
2. Create a feature branch: `git checkout -b feature-name`.
3. Commit changes.
4. Open a PR.

---

## 📜 License

MIT License.
