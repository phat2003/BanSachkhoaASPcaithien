using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using BanSach.Models.ViewModel;
using BanSach.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Security.Claims;

namespace WebBanSach.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]//chức năng này chỉ dành cho người dùng đăng nhập, đặt ở trên Controller là áp dụng cho toàn bộ controller
    [BindProperties]//tự động ánh xạ dữ liệu từ form đến các thuộc tính của lớp, giúp giảm thiểu mã code và tăng tính bảo mật cho ứng dụng
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration; // Thêm biến này để đọc file cấu hình
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int OrderTotl { get; set; }
        public CartController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        //[AllowAnonymous]//cho phép người dùng chưa đăng nhập cũng có thể truy cập vào trang giỏ hàng
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;//lấy thông tin người dùng đang đăng nhập
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);//lấy id của người dùng đang đăng nhập
            
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, 
                includeProperties: "Product"),//lấy tất cả sản phẩm trong giỏ hàng của người dùng đang đăng nhập
                OrderHeader = new()//khởi tạo một đối tượng OrderHeader mới để lưu thông tin đơn hàng
            };
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.ListPrice, 
                    cart.Product.Price50, cart.Product.Price100);
                //ShoppingCartVM.CartTotal += (cart.Price * cart.Count);//tính tổng tiền của giỏ hàng bằng cách nhân giá của sản phẩm với số lượng sản phẩm trong giỏ hàng và cộng dồn vào biến CartTotal
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;//lấy thông tin người dùng đang đăng nhập
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);//lấy id của người dùng đang đăng nhập

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),//lấy tất cả sản phẩm trong giỏ hàng của người dùng đang đăng nhập
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(
                u => u.Id == claim.Value);
            
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.ListPrice,
                    cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);//tính tổng tiền của giỏ hàng bằng cách nhân giá của sản phẩm với số lượng sản phẩm trong giỏ hàng và cộng dồn vào thuộc tính OrderTotal

            }
            return View(ShoppingCartVM);
        }

        [ActionName("Summary")]
        [HttpPost]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;//lấy thông tin người dùng đang đăng nhập
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);//lấy id của người dùng đang đăng nhập

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product");//lấy tất cả sản phẩm trong giỏ hàng của người dùng đang đăng nhập
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.ListPrice,
                    cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);//tính tổng tiền của giỏ hàng bằng cách nhân giá của sản phẩm với số lượng sản phẩm trong giỏ hàng và cộng dồn vào thuộc tính OrderTotal

            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            _unitOfWork.ShoppingCart.removeRange(ShoppingCartVM.ListCart);//xóa tất cả sản phẩm trong giỏ hàng của người dùng đang đăng nhập sau khi đã tạo đơn hàng
            _unitOfWork.Save();

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
            vnpay.AddRequestData("vnp_Amount", (ShoppingCartVM.OrderHeader.OrderTotal * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + ShoppingCartVM.OrderHeader.Id);
            vnpay.AddRequestData("vnp_OrderType", "other"); // Loại hàng hóa
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", ShoppingCartVM.OrderHeader.Id.ToString()); // Mã tham chiếu (mã đơn hàng của bạn)

            // 4. Tạo đường dẫn thanh toán hoàn chỉnh
            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            // 5. Chuyển hướng người dùng
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
                    if (vnp_ResponseCode == "00")
                    {
                        // VNPAY trả về "00" nghĩa là thanh toán thành công
                        // Gọi cơ sở dữ liệu để tìm đơn hàng này
                        var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId);

                        // Cập nhật trạng thái đơn hàng thành "Đã thanh toán"
                        // (Giả sử bạn có biến SD.PaymentStatusApproved trong file SD.cs)
                        orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                        orderHeader.OrderStatus = SD.StatusApproved;
                        _unitOfWork.Save();

                        // TRẢ VỀ TRANG THÀNH CÔNG TẠI ĐÂY
                        return RedirectToAction("Index", "Home");
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

        public IActionResult Plus(int cartId) 
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);//tăng số lượng sản phẩm trong giỏ hàng lên 1
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);//nếu số lượng sản phẩm trong giỏ hàng nhỏ hơn hoặc bằng 1 thì xóa sản phẩm khỏi giỏ hàng
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }
            
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBaseOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if (quantity <= 100)
                {
                    return price50;
                }

                return price100;

            }
        }
    }
}
