using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models.ViewModel;
using BanSach.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BanSach.Utility;

namespace WebBanSach.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public OrderController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim.Value;

            //IEnumerable<OrderHeader> orderHeaderList = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").OrderByDescending(u => u.Id);
            IEnumerable<OrderHeader> orderHeaderlist = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId);
            IEnumerable<OrderDetail> orderDetailslist = _unitOfWork.OrderDetail.GetAll(includeProperties: "Product");
            IEnumerable<OrderVM> orderVMList = orderHeaderlist.Select(orderVMItem => new OrderVM()//sử dụng phương thức Select để chuyển đổi mỗi phần tử trong objIdeaList thành một đối tượng IdeaVM mới.
            {
                OrderHeader = orderVMItem,//gán giá trị của ideaVMItem trong objIdeaList vào thuộc tính idea của IdeaVM.
                //OrderDetail = (IEnumerable<OrderDetail>)orderDetailslist.FirstOrDefault(o => o.OrderId == orderVMItem.Id)//lấy view có IdeaId trùng với Id của ideaVMItem trong objIdeaList. Nếu không tìm thấy sẽ trả về null.
                OrderDetail = orderDetailslist.Where(o => o.OrderId == orderVMItem.Id)


            });

            return View(orderVMList);
        }

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
        [ValidateAntiForgeryToken]
        public IActionResult PayNow(int orderId)
        {
            // 1. Lấy thông tin đơn hàng từ Database
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId);

            if (orderHeader == null)
            {
                return NotFound();
            }

            // 1. Đọc các thông tin cấu hình từ appsettings.json
            string vnp_Returnurl = _configuration["Vnpay:ReturnUrl"];
            string vnp_Url = _configuration["Vnpay:BaseUrl"];
            string vnp_TmnCode = _configuration["Vnpay:TmnCode"];
            string vnp_HashSecret = _configuration["Vnpay:HashSecret"];

            // 2. Khởi tạo lớp tiện ích VnPayLibrary
            VnPayLibrary vnpay = new VnPayLibrary();

            // 3. Nạp các tham số bắt buộc để gửi sang VNPAY
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            // VNPAY yêu cầu số tiền phải nhân lên 100 lần (ví dụ: 10,000 VND thì gửi là 1000000)
            vnpay.AddRequestData("vnp_Amount", (orderHeader.OrderTotal * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + orderHeader.Id);
            vnpay.AddRequestData("vnp_OrderType", "other"); // Loại hàng hóa
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", orderHeader.Id.ToString()); // Mã tham chiếu (mã đơn hàng của bạn)

            // 3. Tạo URL và chuyển hướng người dùng sang VNPay
            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret); // Thay bằng biến config

            return Redirect(paymentUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallback()
        {
            // 1. Kiểm tra xem có dữ liệu do VNPAY gửi về không
            if (Request.Query.Count > 0)
            {
                string vnp_HashSecret = _configuration["Vnpay:HashSecret"]; // Lấy chuỗi bí mật
                var vnpayData = Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();

                // Đọc toàn bộ dữ liệu VNPAY trả về và nạp vào thư viện
                foreach (var (key, value) in vnpayData)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(key, value.ToString());
                    }
                }

                // Lấy mã đơn hàng và mã phản hồi từ VNPAY
                int orderId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_SecureHash = Request.Query["vnp_SecureHash"];

                // 2. Kiểm tra chữ ký (đảm bảo dữ liệu không bị hacker thay đổi trên đường truyền)
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    // Đoạn logic gợi ý nằm trong Action xử lý Callback của VNPay
                    if (vnp_ResponseCode == "00") // 00 nghĩa là thanh toán thành công
                    {
                        var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId);

                        // Cập nhật trạng thái thanh toán nếu trước đó là Pending hoặc DelayedPayment
                        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment ||
                            orderHeader.PaymentStatus == SD.PaymentStatusPending)
                        {
                            orderHeader.PaymentStatus = SD.PaymentStatusApproved; // Thanh toán thành công
                            _unitOfWork.Save();
                        }
                    }
                    else
                    {
                        // Thanh toán thất bại (khách hàng hủy hoặc không đủ tiền)
                        return RedirectToAction("Index", "Home"); // Tạm thời đưa về trang chủ
                    }
                }
                else
                {
                    // Lỗi bảo mật: Chữ ký không khớp!
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
