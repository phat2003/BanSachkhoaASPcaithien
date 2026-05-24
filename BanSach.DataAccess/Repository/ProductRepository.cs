using BanSach.DataAccess.Data;
using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanSach.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository //thừa kế từ lớp Repository<Category> và triển khai giao diện ICategoryRepository.
    {
        private readonly ApplicationDbContext _db;//biến này chỉ được đọc(không được ghi hay làm gì khác).
        
        public ProductRepository(ApplicationDbContext db) : base(db) //gọi đến constructor của lớp cha Repository<T> để khởi tạo DbSet.
        {
            _db = db;// gán giá trị cho biến _db.
            //_db.Products.Include(u => u.Category).Include(u=>u.CoverType);
        }
        public void Save()
        {
            _db.SaveChanges(); //lưu các thay đổi vào cơ sở dữ liệu.
        }

        public void Update(Product product)
        {
            var objFromDb = _db.Products.SingleOrDefault(a => a.Id == product.Id);//tìm một đối tượng Category trong cơ sở dữ liệu dựa trên Id của nó.
            if (objFromDb != null)//nếu tìm thấy đối tượng có Id trùng(ở đây ám chỉ có giá trị khác null(not null)) với Id của đối tượng product được truyền vào.
            {
                objFromDb.Title = product.Title;//cập nhật các thuộc tính của đối tượng objFromDb với các giá trị từ đối tượng product được truyền vào.
                objFromDb.ISBN = product.ISBN;
                objFromDb.ListPrice = product.ListPrice;
                objFromDb.Price50 = product.Price50;
                objFromDb.Price100 = product.Price100;
                objFromDb.Description = product.Description;
                objFromDb.CategoryId = product.CategoryId;
                objFromDb.Author = product.Author;
                objFromDb.CoverTypeId = product.CoverTypeId;
                if (product.ImageUrl != null)//nếu ImageUrl của đối tượng product không phải là null(thông tin hình ảnh có thay đổi).
                {
                    objFromDb.ImageUrl = product.ImageUrl;//cập nhật ImageUrl của objFromDb với giá trị mới từ product.
                }
            }
            
        }
    }
}
