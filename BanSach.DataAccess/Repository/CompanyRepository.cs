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
    public class CompanypeRepository : Repository<Company>, ICompanyRepository //thừa kế từ lớp Repository<Category> và triển khai giao diện ICategoryRepository.
    {
        private readonly ApplicationDbContext _db;//biến này chỉ được đọc(không được ghi hay làm gì khác).
        
        public CompanypeRepository(ApplicationDbContext db) : base(db) //gọi đến constructor của lớp cha Repository<T> để khởi tạo DbSet.
        {
            _db = db;// gán giá trị cho biến _db.
        }
        public void Save()
        {
            _db.SaveChanges(); //lưu các thay đổi vào cơ sở dữ liệu.
        }

        public void Update(Company company)
        {
            _db.Companys.Update(company); //cập nhật thông tin của một đối tượng Category trong cơ sở dữ liệu.
        }
    }
}
