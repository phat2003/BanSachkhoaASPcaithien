using System.ComponentModel.DataAnnotations;

namespace BanSach.Models
{
    public class CoverType
    {
        [Key] //set id làm khoá chính
        public int Id { get; set; }
        [Required] //muốn trường name không được phép null thì thêm dòng này
        public string Name { get; set; }

    }
}
