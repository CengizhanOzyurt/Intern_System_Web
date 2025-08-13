using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectForm.Models
{
    [Table("Users")]
    public class UsersModel
    {
        public int id { get; set; }
        public  String name { get; set; }
        public  String surname { get; set; }
        public  String email { get; set; }
        public  String password { get; set; }
        public bool gender { get; set; }

    }
}
