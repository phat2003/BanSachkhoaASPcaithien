using BanSach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanSach.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);//phương thức để cập nhật thông tin của một đối tượng Category trong cơ sở dữ liệu.
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
    }
}
