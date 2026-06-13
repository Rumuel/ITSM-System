namespace Domain.Constants
{
    public static class AppRoles
    {
        public const string Administrator = "Administrador";
        public const string Technician = "Tecnico";
        public const string User = "Utilizador";

        public static readonly string[] All =
        {
            Administrator,
            Technician,
            User
        };
    }
}
