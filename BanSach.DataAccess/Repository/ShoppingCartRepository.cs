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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository //thừa kế từ lớp Repository<Category> và triển khai giao diện ICategoryRepository.
    {
        private readonly ApplicationDbContext _db;//biến này chỉ được đọc(không được ghi hay làm gì khác).
        
        public ShoppingCartRepository(ApplicationDbContext db) : base(db) //gọi đến constructor của lớp cha Repository<T> để khởi tạo DbSet.
        {
            _db = db;// gán giá trị cho biến _db.
        }

        int IShoppingCartRepository.DecrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count -= count;//giảm số lượng sản phẩm trong giỏ hàng đi count.
            return shoppingCart.Count;//trả về số lượng sản phẩm còn lại trong giỏ hàng sau khi đã giảm.
        }

        int IShoppingCartRepository.IncrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count += count;//tăng số lượng sản phẩm trong giỏ hàng lên count.
            return shoppingCart.Count;//trả về số lượng sản phẩm trong giỏ hàng sau khi đã tăng.
        }
    }
}
