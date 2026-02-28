# Student Personal Web Pages & Portfolio Platform 🎓

A comprehensive, full-stack web application built with **ASP.NET Core MVC**. This platform serves as a centralized hub for college students to create, manage, and share their personal profiles, CVs, and academic posts. It also features a robust **Admin Portal** equipped with strict content moderation, email notifications, and an approval workflow for all student activities.

## ✨ Key Features

### 🛡️ Strict Moderation & Approval Workflow
* **Account Approval System:** Student registrations are not instantly active. Upon signing up, students receive an automated **Email Notification** stating their account is in a `Pending` state.
* **Content Moderation:** Every action taken by a student—whether it's creating a profile, publishing a new post, or submitting a complaint—is initially marked as `Pending` and requires manual Admin approval before going live.

### 🎓 Student Portal
* **Profile & CV Management:** Once approved, students can edit personal details, upload images, and generate dynamic CVs (`CreateForm`, `EditProfile`, `CVDetails`).
* **Content Publishing:** Students can submit posts that go through the college's approval pipeline (`PendingApproval`, `Rejected`, `StudentDashboard`).
* **Secure Authentication:** User registration and secure login powered by ASP.NET Core Identity.

### 👨‍💼 Admin Portal
* **Admin Dashboard:** Centralized view of platform activity, pending requests, and active students (`Dashboard`, `ActiveStudents`).
* **Comprehensive Review System:** Admins can review, approve, or reject student accounts, profiles, and posts (`PendingPosts`, `ReportedPosts`, `ReviewRequests`, `Moderation`).
* **Complaint Management:** A dedicated system to handle user reports and complaints (`Complaints`).
* **Automated Filtering:** Built-in banned words dictionary to automatically flag or restrict inappropriate content (`BannedWord.cs`).

## 🛠️ Technologies Used

* **Framework:** ASP.NET Core MVC (.NET)
* **Language:** C#
* **Database:** Microsoft SQL Server
* **ORM:** Entity Framework Core (EF Core)
* **Email Service:** SMTP / Email Sending Integration for user notifications.
* **Frontend:** Razor Views (`.cshtml`), HTML5, CSS3, Vanilla JavaScript
* **Architecture:** Model-View-Controller (MVC)

## 📂 Project Structure

```text
Student-Personal-Web-Pages-Application/
│
├── Student_Profile_Pro.sln          # Main Visual Studio Solution File
├── Student_Profile/                 # Core ASP.NET Core MVC Project
│   ├── Models/
│   │   ├── ApplicationUser.cs       # Identity User Model
│   │   ├── StudentProfile.cs        # Student Data Model
│   │   ├── Post.cs                  # Student Posts Model
│   │   ├── Complaint.cs             # Complaints Handling
│   │   └── BannedWord.cs            # Content Moderation Dictionary
│   │
│   ├── Views/                       # Razor Views (Admin, Student, Account, etc.)
│   └── Controllers/                 # Application Logic 
│
├── admin/                           # Raw Admin Frontend Assets (HTML/CSS)
├── student/                         # Raw Student Frontend Assets (HTML/CSS)
└── home_page/                       # Landing Page Assets
