namespace GymSaaS.Shared;

public static class UiPermissionKeys
{
    // Dashboard
    public const string DashboardView = "dashboard.view";

    // Members
    public const string MembersView   = "members.view";
    public const string MembersCreate = "members.create";
    public const string MembersEdit   = "members.edit";
    public const string MembersDelete = "members.delete";

    // Memberships (enrollments)
    public const string MembershipsView   = "memberships.view";
    public const string MembershipsCreate = "memberships.create";
    public const string MembershipsEdit   = "memberships.edit";

    // Packages (pricing tiers)
    public const string PackagesView   = "packages.view";
    public const string PackagesCreate = "packages.create";
    public const string PackagesEdit   = "packages.edit";
    public const string PackagesDelete = "packages.delete";

    // Trainers
    public const string TrainersView   = "trainers.view";
    public const string TrainersCreate = "trainers.create";
    public const string TrainersEdit   = "trainers.edit";
    public const string TrainersDelete = "trainers.delete";

    // Services (classes + PT sessions)
    public const string ServicesView   = "services.view";
    public const string ServicesManage = "services.manage";

    // Payments
    public const string PaymentsView   = "payments.view";
    public const string PaymentsCreate = "payments.create";

    // Attendance
    public const string AttendanceView    = "attendance.view";
    public const string AttendanceCheckIn = "attendance.check_in";

    // Reports
    public const string ReportsView = "reports.view";

    // Settings
    public const string SettingsView        = "settings.view";
    public const string SettingsUsersManage = "settings.users_manage";

    /// <summary>All defined permission keys.</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        DashboardView,
        MembersView, MembersCreate, MembersEdit, MembersDelete,
        MembershipsView, MembershipsCreate, MembershipsEdit,
        PackagesView, PackagesCreate, PackagesEdit, PackagesDelete,
        TrainersView, TrainersCreate, TrainersEdit, TrainersDelete,
        ServicesView, ServicesManage,
        PaymentsView, PaymentsCreate,
        AttendanceView, AttendanceCheckIn,
        ReportsView,
        SettingsView, SettingsUsersManage,
    };

    // ── Default permission sets per system role ──────────────

    public static readonly IReadOnlyList<string> OwnerDefaults = All;

    public static readonly IReadOnlyList<string> ManagerDefaults = new[]
    {
        DashboardView,
        MembersView, MembersCreate, MembersEdit, MembersDelete,
        MembershipsView, MembershipsCreate, MembershipsEdit,
        PackagesView, PackagesCreate, PackagesEdit, PackagesDelete,
        TrainersView, TrainersCreate, TrainersEdit, TrainersDelete,
        ServicesView, ServicesManage,
        PaymentsView, PaymentsCreate,
        AttendanceView, AttendanceCheckIn,
        ReportsView,
        SettingsView,
        // settings.users_manage intentionally excluded
    };

    public static readonly IReadOnlyList<string> ReceptionistDefaults = new[]
    {
        MembersView, MembersCreate, MembersEdit,
        MembershipsView, MembershipsCreate, MembershipsEdit,
        PackagesView,
        PaymentsView, PaymentsCreate,
        AttendanceView, AttendanceCheckIn,
    };

    public static readonly IReadOnlyList<string> TrainerDefaults = new[]
    {
        AttendanceView,
        ServicesView,
    };

    public static IReadOnlyList<string> DefaultsFor(string? roleName) => roleName switch
    {
        "Owner"        => OwnerDefaults,
        "Manager"      => ManagerDefaults,
        "Receptionist" => ReceptionistDefaults,
        "Trainer"      => TrainerDefaults,
        _              => Array.Empty<string>(),
    };
}
