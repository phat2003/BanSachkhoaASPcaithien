using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using BanSach.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace WebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")] // Chỉ định Controller này thuộc khu vực dành cho Admin
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // Tiêm phụ thuộc (Dependency Injection) để gọi cơ sở dữ liệu
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // Lấy danh sách toàn bộ đơn hàng
            // includeProperties: "ApplicationUser" giúp lấy kèm dữ liệu của người dùng đặt hàng
            IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            return View(objOrderHeaders); // Gửi dữ liệu sang View
        }
        //public IActionResult Details(int id)
        //{
        //    // Lấy thông tin đơn hàng có Id khớp với tham số truyền vào
        //    // Kèm theo thông tin ApplicationUser
        //    OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");

        //    return View(orderHeader);
        //}

        public IActionResult Details(int id)
        {
            // Khởi tạo ViewModel
            OrderVM orderVM = new OrderVM()
            {
                // Lấy thông tin đơn hàng chính
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser"),

                // Lấy danh sách chi tiết các cuốn sách trong đơn hàng này
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == id, includeProperties: "Product")
            };

            return View(orderVM); // Truyền ViewModel sang View
        }

        [HttpPost]
        public IActionResult StartProcessing(OrderVM orderVM)
        {
            // Cập nhật trạng thái thành "Processing"
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, "Processing");

            // Lưu vào cơ sở dữ liệu
            _unitOfWork.Save();

            // Tạo một thông báo ngắn gọn để hiện trên màn hình (nếu bạn có cấu hình TempData)
            TempData["Success"] = "Đơn hàng đang được xử lý!";

            // Quay lại trang chi tiết
            return RedirectToAction(nameof(Details), new { id = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        public IActionResult ShipOrder(OrderVM orderVM)
        {
            // 1. Lấy thông tin đơn hàng hiện tại từ cơ sở dữ liệu để đảm bảo dữ liệu chính xác
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);

            // 2. Cập nhật các thông tin vận chuyển mà người quản trị đã nhập từ giao diện
            orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;

            // 3. Thay đổi trạng thái sang "Shipped" (Đã giao) và ghi nhận thời gian
            orderHeaderFromDb.OrderStatus = "Shipped";
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            // 4. Gọi phương thức Update để đánh dấu đối tượng đã thay đổi
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);

            // 5. Lưu tất cả thay đổi xuống cơ sở dữ liệu
            _unitOfWork.Save();

            TempData["Success"] = "Đơn hàng đã được cập nhật trạng thái giao hàng thành công! 🚚";

            return RedirectToAction(nameof(Details), new { id = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);

            // Kiểm tra xem khách hàng đã thanh toán hay chưa
            if (orderHeader.PaymentStatus == "Approved")
            {
                // ... (Code gọi API hoàn tiền của Stripe hoặc VNPay sẽ viết ở đây sau) ...

                // Cập nhật trạng thái thanh toán sau khi hoàn tiền
                orderHeader.PaymentStatus = "Refunded";
            }
            else
            {
                // Nếu chưa thanh toán thì gán trạng thái thanh toán là Cancelled
                orderHeader.PaymentStatus = "Cancelled";
            }

            // Đổi trạng thái đơn hàng chính thành Hủy
            orderHeader.OrderStatus = "Cancelled";

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            TempData["Success"] = "Đơn hàng đã được hủy thành công.";
            return RedirectToAction(nameof(Details), new { id = orderVM.OrderHeader.Id });
        }
    }
}
