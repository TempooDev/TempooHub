namespace TempooHub.AuthServer.Pages.Admin
{
    public partial class UsersModel
    {
        public class UserViewModel
        {
            public string Id { get; set; } = "";
            public string Email { get; set; } = "";
            public bool EmailConfirmed { get; set; } = false;
            public IList<string> Roles { get; set; } = new List<string>();
        }
    }
}