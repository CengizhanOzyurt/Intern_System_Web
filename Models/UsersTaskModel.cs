using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ProjectForm.Models
{
    [Table("Users_Task")]
    public class UsersTaskModel
    {
    [Key]
    public int id { get; set; }              
    public int user_id { get; set; }         

    public DateTime? start_date { get; set; }
    public DateTime? end_date { get; set; }
    public string task { get; set; } = string.Empty;
    }
}