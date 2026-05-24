using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanSach.Models
{
    public class ShoppingCart
    {

        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        [Range(1,1000)]
        public int Count { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [NotMapped]//thuộc tính này sẽ không được ánh xạ vào cơ sở dữ liệu, nó chỉ tồn tại trong lớp ShoppingCart và không có cột tương ứng trong bảng ShoppingCart của cơ sở dữ liệu.
        public double Price { get; set; }
    }
}
