# LoginForm

![.NET 10 MVC](https://img.shields.io/badge/.NET-10-purple)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Student%20DB-blue)
![Dockerized](https://img.shields.io/badge/docker-ready-2496ed)
![License](https://img.shields.io/badge/license-MIT-green)

A modern, feature-rich Student Management System built with .NET 10 MVC and PostgreSQL.  
Manage student data efficiently with a responsive web interface, robust authentication, role-based access, and more.

---

## 🚀 Features

- **User Authentication**: Secure login & registration with hashed passwords
- **Role-Based Access**: Admin & Student privileges, assignable roles
- **Student Record Management**: Create, update, delete, and view student profiles
- **Responsive UI**: Clean HTML5/CSS3 interface with seamless navigation
- **Database Integration**: PostgreSQL backend for data persistence
- **API Endpoints**: For integration with other systems/applications
- **Dockerized Setup**: Easy deployment & environment replication

---

## 📦 Tech Stack

- **Backend**: C# (.NET 10 MVC)
- **Database**: PostgreSQL
- **Frontend**: HTML5, CSS3, JavaScript
- **Containerization**: Docker


---

## ⚙️ Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/) *(Optional, for containerized setup)*

### 1. Clone the repository

```bash
git clone https://github.com/satyop7/LoginForm.git
cd LoginForm
```

### 2. Configure Database

- Edit `appsettings.json` with your PostgreSQL credentials:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=studentdb;Username=yourusername;Password=yourpassword"
}
```

### 3. Apply Migrations

```bash
dotnet ef database update
```

### 4. Run the application

```bash
dotnet run
```

Visit [http://localhost:5000](http://localhost:5000) in your browser.

---

### 🚢 Docker (Optional)

```bash
docker-compose up --build
```
> This will spin up both the application and PostgreSQL database containers.

---

## 📂 Project Structure

```
LoginForm/
├── Controllers/
├── Models/
├── Views/
├── wwwroot/
├── appsettings.json
├── Dockerfile
├── docker-compose.yml
└── ...
```

---

## 🤝 Contributing

Contributions are welcome!  
Feel free to [open an issue](https://github.com/satyop7/LoginForm/issues) or submit a pull request.

---

## 📝 License

This project is licensed under the [MIT License](LICENSE).

---

## 📧 Contact

Questions, suggestions?  
Open an issue or email: [your.email@example.com](mailto:your.email@example.com)
