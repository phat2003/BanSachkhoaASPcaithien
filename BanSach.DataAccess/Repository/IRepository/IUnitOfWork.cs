using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanSach.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork //định nghĩa một giao diện IUnitOfWork để quản lý các repository.
    {
        ICategoryRepository Category { get; }//thuộc tính Category để truy cập các phương thức của CategoryRepository.
        ICoverTypeRepository covertype { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailRepository OrderDetail { get; }
        void Save(); //phương thức Save để lưu các thay đổi vào cơ sở dữ liệu.
    }
}
