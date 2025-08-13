namespace ProjectForm.Models
{
    public class HomePageViewModel
    {
        public UsersTaskModel TaskModel { get; set; } = new UsersTaskModel();
        public List<UsersTaskModel> Tasks { get; set; } = new List<UsersTaskModel>();
    }
}