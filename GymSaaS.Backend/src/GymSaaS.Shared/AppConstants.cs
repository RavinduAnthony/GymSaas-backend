namespace GymSaaS.Shared;

public static class AppConstants
{
    public static class ClaimTypes
    {
        public const string UserId = "UserId";
        public const string TenantId = "TenantId";
    }

    public static class Roles
    {
        public const string Owner = "Owner";
        public const string Manager = "Manager";
        public const string Receptionist = "Receptionist";
        public const string Trainer = "Trainer";
    }

    /// <summary>
    /// IDs of the seeded PaymentType master-table rows.
    /// Use these in service logic instead of raw strings.
    /// </summary>
    public static class PaymentTypeIds
    {
        public const int RegistrationFee    = 1;
        public const int MonthlyInitial     = 2;
        public const int RegularMonthly     = 3;
        public const int LateMonthly        = 4; // Paid in Week 2-4 of due month
        public const int FullPackagePayment = 5; // Single upfront payment for full package duration
    }

    public static class MemberTypes
    {
        public const string Monthly = "Monthly"; // Type A - regular monthly billing
        public const string Special = "Special"; // Type B - full upfront / special package
    }
}
