using BanSach.DataAccess.Data;
using BanSach.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanSach.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; }// thuộc tính chỉ có thể được nhận(get) và gán(set) giá trị bên trong lớp UnitOfWork này.

        public ICoverTypeRepository covertype { get; private set; }

        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }

        private readonly ApplicationDbContext _db;//biến này chỉ được đọc(không được ghi hay làm gì khác).

        public UnitOfWork(ApplicationDbContext db) //gọi đến constructor của lớp cha Repository<T> để khởi tạo DbSet.
        {
            _db = db;// gán giá trị cho biến _db.
            Category = new CategoryRepository(_db);//khởi tạo CategoryRepository và gán nó cho thuộc tính Category.
            covertype = new CoverTypeRepository(_db);//khởi tạo CoverTypeRepository và gán nó cho thuộc tính covertype.
            Product = new ProductRepository(_db);//khởi tạo ProductRepository và gán nó cho thuộc tính Product.
            Company = new CompanypeRepository(_db);//khởi tạo CompanyRepository và gán nó cho thuộc tính Company.
            ApplicationUser = new ApplicationUserRepository(_db);//khởi tạo ApplicationUserRepository và gán nó cho thuộc tính ApplicationUser.
            ShoppingCart = new ShoppingCartRepository(_db);//khởi tạo ShoppingCartRepository và gán nó cho thuộc tính ShoppingCart.
            OrderHeader = new OrderHeaderRepository(_db);//khởi tạo OrderHeaderRepository và gán nó cho thuộc tính OrderHeader.
            OrderDetail = new OrderDetailRepository(_db);//khởi tạo OrderDetailRepository và gán nó cho thuộc tính OrderDetail.

        }
        public void Save()
        {
            _db.SaveChanges(); //lưu các thay đổi vào cơ sở dữ liệu.
        }
    }
}
