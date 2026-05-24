using System.ComponentModel.DataAnnotations;

namespace BanSach.Models
{
    public class Category
    {
        [Key] //set id làm khoá chính
        public int Id { get; set; }
        [Required] //muốn trường name không được phép null thì thêm dòng này
        public string Name { get; set; }

        [Display(Name = "Display Order")]
        [Range(1, 100,
        ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int DisplayOrder { get; set; }
        public DateTime CreateDatetime { get; set; } = DateTime.Now;
    }
}
