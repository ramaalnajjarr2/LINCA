namespace LINCA_v1.ViewModels
{
    public class UserRole
    {

        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public List<string> Roles { get; set; } = new();

        public string SelectedRole { get; set; }
        public List<string> AllRoles { get; set; } = new();
    }
}
