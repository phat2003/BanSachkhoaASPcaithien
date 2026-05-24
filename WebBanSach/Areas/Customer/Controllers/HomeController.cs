using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;


namespace WebBanSach.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;


        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(objProductList);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart cartObj = new()
            {
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType"),
                Count = 1,//m?c ??nh s? l??ng là 1 khi vào trang details
                ProductId=id
            };
            return View(cartObj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]//chức năng này chỉ dành cho người dùng đăng nhập
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;//lấy thông tin người dùng đang đăng nhập
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);//lấy id của người dùng đang đăng nhập
            shoppingCart.ApplicationUserId = claim.Value;//gán id của người dùng đang đăng nhập vào thuộc tính ApplicationUserId của shoppingCart

            ShoppingCart cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);//kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng của người dùng chưa

            if (cartObj == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);//nếu sản phẩm chưa tồn tại trong giỏ hàng của người dùng thì thêm sản phẩm vào giỏ hàng
            }
            else
            {
                _unitOfWork.ShoppingCart.IncrementCount(cartObj, shoppingCart.Count);//nếu sản phẩm đã tồn tại trong giỏ hàng của người dùng thì tăng số lượng sản phẩm trong giỏ hàng lên shoppingCart.Count

            }

            
            _unitOfWork.Save();//lưu thay đổi vào database
            
            return RedirectToAction(nameof(Index));//chuyển hướng về trang chủ sau khi thêm sản phẩm vào giỏ hàng

            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
