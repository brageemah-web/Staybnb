# Staybnb: Airbnb Clone

A property rental platform built with **ASP.NET Core MVC**, **Entity Framework Core** and **SQL Server**.
It models the user lifecycle: a user registers as a **Guest**, applies to become a **Host**, and
**Administrators** maintain the quality and security of the marketplace.

---

## 1. Requirements

| Requirement | Notes |
|---|---|
| .NET SDK | The version the project targets (see `staybnb.csproj`; developed on .NET 10) |
| Visual Studio 2022 or later | With the **ASP.NET and web development** workload |
| SQL Server LocalDB | Installed with Visual Studio. Any SQL Server instance also works |

## 2. How to run

1. Extract the zip and open the solution file (`staybnb.sln` or `staybnb.slnx`) in Visual Studio.
2. Check the connection string in `staybnb/appsettings.json`.
3. Press **F5** (or **Ctrl+F5**).

**The database is created automatically.** On startup the app applies all EF Core migrations
(`MigrateAsync()`) and then seeds the roles and the SuperAdmin account, so no SQL script is needed.

To apply migrations manually instead, open **Package Manager Console** and run:

```
Update-Database
```

## 3. Database connection

Default connection string (`appsettings.json`):

```json
"ConnectionStrings": {
  "DefaultConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=StaybnbDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

If you use a different SQL Server instance, change the `Server=` value. The database is named `StaybnbDb`.

## 4. Seeded login (created automatically on startup)

| Role | Email | Password |
|---|---|---|
| SuperAdmin | `superadmin@staybnb.com` | `SuperAdmin@123` |

The four roles **SuperAdmin, Admin, Host, Guest** are also created on startup.

### Test accounts

Register these through the **Register** page (every new registrant becomes a Guest):

| Purpose | Email | Password |
|---|---|---|
| Guest (becomes Host after approval) | _fill in_ | _fill in_ |
| Second Guest (books a property) | _fill in_ | _fill in_ |
| Guest promoted to Admin | _fill in_ | _fill in_ |

## 5. Roles and what each can do

| Role | Access |
|---|---|
| **Guest** | Browse and filter properties, request bookings, complete check-in with a document upload, apply to become a Host, message hosts |
| **Host** | Dashboard, manage listings (activate/deactivate), approve or reject bookings, configure the check-in process, verify guest documents |
| **Admin** | Review (approve/reject) Host applications, view the activity log |
| **SuperAdmin** | Everything an Admin can do, plus promote Guests to Admin |

A user is either a **Guest or a Host, not both**. When an Admin approves a Host application the system adds
the **Host** role, removes the **Guest** role and refreshes the user's security stamp, so the user signs in
again with the new role. Dashboards are protected with `[Authorize(Roles = "...")]`, so a Host cannot open
Guest-only pages and vice versa.

## 6. Suggested walkthrough

1. Register user A (Guest). Open **Become a Host** and submit the form. An image is mandatory.
2. Log in as the SuperAdmin, open **Applications**, and **Approve** it. A is now a Host.
3. Log in again as A and open the Host dashboard. Open **Check-in setup** on the property and save it.
4. Register user B (Guest), open a property, choose dates and **Request to book**.
5. As A, open **Booking Requests** and **Approve** the booking.
6. As B, open **My Bookings**, click **Check in**, accept the rules, upload an ID or passport, and submit.
7. As A, open **Guest Check-ins** and approve the document. The booking becomes **CheckedIn**.
8. Exchange messages between A and B, and check the **notification bell**.
9. As the SuperAdmin, open **Users** to promote a Guest to Admin, and **Logs** to see the audit trail.

## 7. Key features

- **Identity and roles:** registration, login and logout, SuperAdmin seeding, Guest to Host role swap
- **Hosting workflow:** property application with at least one image, admin approval, Host dashboard
- **Booking engine:** `TotalPrice = (PricePerNight x Nights) + CleaningFee + ServiceFee`
  (service fee is 12% of the nightly subtotal; price is always calculated on the server)
- **Booking lifecycle:** Pending, Approved, CheckedIn (also Rejected and Canceled)
- **Check-in:** Host-defined rules and accepted documents, Guest upload, Host verification
- **Messaging:** database-backed inbox with replies and unread counts
- **Notifications:** bell icon with unread count, read/unread status stored in the database
- **Activity log:** records logins, logouts, registrations, role changes, applications, booking and check-in updates

## 8. Project structure

```
staybnb/
├── Controllers/       Account, Admin, Users, Host, Guest, HostApplication, Properties, Messages, Notifications
├── Data/              ApplicationContext, RoleSeeder
├── Migrations/        EF Core migrations
├── Models/            Entities and enums
├── Services/          ActivityLogService, NotificationService, BookingService, FileService
├── ViewComponents/    NotificationBell
├── ViewModels/        Form and display models
├── Views/             Razor views
└── wwwroot/           Static files and uploaded property images
```

## 9. File storage

| Content | Location | Public? |
|---|---|---|
| Property images | `wwwroot/uploads/properties/` | Yes |
| Guest ID/passport documents | `App_Data/documents/` | **No**. Served only to the Host who owns the booking |

Both folders are created automatically when the first file is uploaded.

## 10. Troubleshooting

| Problem | Fix |
|---|---|
| "The ConnectionString property has not been initialized" | The key in `GetConnectionString(...)` in `Program.cs` must match the key in `appsettings.json` (`DefaultConnectionString`) |
| "Pending model changes" error | Run `Add-Migration <Name>` then `Update-Database` after any model change |
| Connection timeout expired | Run `sqllocaldb stop MSSQLLocalDB -k`, then `sqllocaldb start MSSQLLocalDB`, and try again |
| "View not found" after adding a view | Stop the app and rebuild. New views need a rebuild |
| Application blocked by Windows (Application Control policy) | Smart App Control or an organisation policy is blocking the unsigned build, so use a machine that allows it |

## 11. Technologies

ASP.NET Core MVC, Entity Framework Core, ASP.NET Core Identity, SQL Server (LocalDB), Bootstrap, Razor.

## 12. License
Developed for academic evaluation and educational demonstration purposes.

