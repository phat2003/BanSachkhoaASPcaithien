using BanSach.DataAccess.Data;
using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using BanSach.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace WebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;//biến này chỉ được đọc(không được ghi hay làm gì khác).
        private IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;//unitOfWork được thêm vào contructor này để ProductController thay đổi và cập nhật dữ liệu cho bảng Product thông qua các thao tác như thêm, xoá, sửa.
            _webHostEnvironment = webHostEnvironment;
        }

        //action của View Index của Product
        public IActionResult Index()//action này được truyền vào các hàm hiển thị danh sách sản phẩm tại view Index của Product
        {
            IEnumerable<Product> objProductList = _unitOfWork.Product.GetAll();//lấy tất cả dữ liệu từ bảng Product và gán vào biến objProductList
            return View(objProductList);//trả về view với dữ liệu objProductList
        }
        //action view create
        

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM();
            productVM.product = new Product();
            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(
                u=>new SelectListItem { 
                    Text=u.Name,
                    Value=u.Id.ToString()}
                );
            productVM.CoverTypeList = _unitOfWork.covertype.GetAll().Select(
                u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() }
                );

            if (id == null || id == 0)//nếu id null hoặc = 0 thì trả về notfound (không tìm thấy).
            {
                //Create product
                return View(productVM);//mỗi lần tạo sản phẩm xong là return về View là product
            }
            else
            {
                //Update product
                productVM.product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);//lấy data sản phẩm có sẵn của bảng product đã thêm vào database trước đó có id trùng với id truyền vào.
                //lúc này productVM.product sẽ có dữ liệu của sản phẩm cần update chứ vẫn chưa update vì đây là action get để lấy dữ liệu từ database và show dữ liệu ra thôi.
                //action httppost sẽ làm nhiệm vụ update dữ liệu.

            }


            return View(productVM);//trả về view dù cho có đáp ứng 2 điều kiện trên hay không.
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult Upsert(ProductVM obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                //upload images
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file!=null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);
                    if (obj.product.ImageUrl != null) 
                    { 
                        //this is an edit and we need to remove old image
                        var oldImagePath = Path.Combine(wwwRootPath, obj.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.product.ImageUrl = @"images\products\"+ fileName + extension;

                }
                if (obj.product.Id==0)
                {
                    _unitOfWork.Product.Add(obj.product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.product);
                }

                _unitOfWork.Save();
                TempData["Sucess"] = "Product create sucessfully";
                return RedirectToAction("index");
            }

            // --- THÊM ĐOẠN NÀY ĐỂ FIX LỖI MẤT DROPDOWN KHI CÓ LỖI ---
            obj.CategoryList = _unitOfWork.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            obj.CoverTypeList = _unitOfWork.covertype.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            // --------------------------------------------------------

            return View(obj);
        }

        

        //post
        
        #region API_CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = productList });
        }
        [HttpDelete]
        //[ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                if (obj.ImageUrl != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    //this is an edit and we need to remove old image
                    var oldImagePath = Path.Combine(wwwRootPath, obj.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.Product.Remove(obj);
                _unitOfWork.Save();

                return Json(new {sucess=true,message="Delete Successful"});
            }


            return View(obj);
        }
        #endregion
    }
}
