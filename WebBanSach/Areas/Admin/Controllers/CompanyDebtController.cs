using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using BanSach.Models.ViewModel;
using BanSach.Utility;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyDebtController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyDebtController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //public IActionResult Index()
        //{
        //    // Khởi tạo danh sách rỗng để truyền xuống View
        //    IEnumerable<Company> companyList = _unitOfWork.Company.GetAll();

        //    // Lấy danh sách tất cả các công ty
        //    IEnumerable<OrderHeader> orderHeaderslist = _unitOfWork.OrderHeader.GetAll();
        //    IEnumerable<CompanyDebtVM> companydebtVMList = companyList.Select(companydeptVMItem => new CompanyDebtVM()//sử dụng phương thức Select để chuyển đổi mỗi phần tử trong objIdeaList thành một đối tượng IdeaVM mới.
        //    {
        //        company = companydeptVMItem,//gán giá trị của ideaVMItem trong objIdeaList vào thuộc tính idea của IdeaVM.
        //        orderHeader = orderHeaderslist.FirstOrDefault(v => v.Id == companydeptVMItem.Id),//lấy view có IdeaId trùng với Id của ideaVMItem trong objIdeaList. Nếu không tìm thấy sẽ trả về null.

        //    });
        //    foreach (var company in companydebtVMList)
        //    {
        //        // 1. Lọc các đơn hàng của công ty hiện tại VÀ có trạng thái "Trả sau"
        //        var companyOrders = _unitOfWork.OrderHeader.GetAll(u =>
        //            u.ApplicationUser.CompanyId == company.company.Id &&
        //            u.PaymentStatus == SD.PaymentStatusDelayedPayment, includeProperties: "ApplicationUser");

        //        // 2. Tính tổng cột OrderTotal của các đơn hàng vừa lọc
        //        double totalDebt = companyOrders.Sum(u => u.OrderTotal);

        //        if (totalDebt > 0)
        //        {
        //            // 3. Đóng gói dữ liệu và Thêm vào danh sách hiển thị
        //            CompanyDebtVM companydebtView = new CompanyDebtVM()
        //            {
        //                company = company.company,
        //                TotalDebt = totalDebt
        //            };
        //        }
        //    }

        //        return View(companydebtVMList);
        //}

        public IActionResult Index()
        {
            List<CompanyDebtVM> companyDebtList = new List<CompanyDebtVM>();
            var companyList = _unitOfWork.Company.GetAll();

            foreach (var company in companyList)
            {
                // 1. Dùng Include "ApplicationUser" để có thể check được CompanyId của User đặt hàng
                var companyOrders = _unitOfWork.OrderHeader.GetAll(u =>
                    u.ApplicationUser.CompanyId == company.Id &&
                    u.PaymentStatus == SD.PaymentStatusDelayedPayment,
                    includeProperties: "ApplicationUser");

                // 2. Tính tổng nợ
                double totalDebt = companyOrders.Sum(u => u.OrderTotal);

                // 3. (Tùy chọn) Chỉ đưa vào danh sách hiển thị những công ty CÓ NỢ (> 0đ)
                if (totalDebt > 0)
                {
                    companyDebtList.Add(new CompanyDebtVM
                    {
                        company = company,
                        TotalDebt = totalDebt
                    });
                }
            }

            return View(companyDebtList);
        }

        public IActionResult Details(int companyId)
        {
            // Lấy danh sách các đơn hàng nợ
            var orderList = _unitOfWork.OrderHeader.GetAll(
                u => u.ApplicationUser.CompanyId == companyId && u.PaymentStatus == SD.PaymentStatusDelayedPayment,
                includeProperties: "ApplicationUser"
            );

            // Lấy thông tin công ty để hiển thị tiêu đề trang
            var company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == companyId);
            ViewData["CompanyName"] = company?.Name;

            return View(orderList);
        }
    }
}
