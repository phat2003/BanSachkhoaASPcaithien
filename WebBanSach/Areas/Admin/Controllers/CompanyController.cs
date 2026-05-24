using BanSach.DataAccess.Data;
using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using BanSach.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace WebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;//biến này chỉ được đọc(không được ghi hay làm gì khác).
        private IWebHostEnvironment _webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;//unitOfWork được thêm vào contructor này để ProductController thay đổi và cập nhật dữ liệu cho bảng Product thông qua các thao tác như thêm, xoá, sửa.
            _webHostEnvironment = webHostEnvironment;
        }

        //action của View Index của Product
        public IActionResult Index()//action này được truyền vào các hàm hiển thị danh sách sản phẩm tại view Index của Product
        {
            
            return View();//trả về view với dữ liệu objProductList
        }
        //action view create
        

        public IActionResult Upsert(int? id)
        {
            Company company = new Company();

            if (id == null || id == 0)//nếu id null hoặc = 0 thì trả về notfound (không tìm thấy).
            {
                //Create product
                return View(company);//mỗi lần tạo sản phẩm xong là return về View là product
            }
            else
            {
                //Update product
                company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);//lấy data sản phẩm có sẵn của bảng product đã thêm vào database trước đó có id trùng với id truyền vào.
                //lúc này productVM.product sẽ có dữ liệu của sản phẩm cần update chứ vẫn chưa update vì đây là action get để lấy dữ liệu từ database và show dữ liệu ra thôi.
                //action httppost sẽ làm nhiệm vụ update dữ liệu.

            }


            return View(company);//trả về view dù cho có đáp ứng 2 điều kiện trên hay không.
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id==0)
                {
                    _unitOfWork.Company.Add(obj);
                }
                else
                {
                    _unitOfWork.Company.Update(obj);
                }

                _unitOfWork.Save();
                return RedirectToAction("index");
            }

            // --- THÊM ĐOẠN NÀY ĐỂ FIX LỖI MẤT DROPDOWN KHI CÓ LỖI ---
            //obj.CategoryList = _unitOfWork.Category.GetAll().Select(
            //    u => new SelectListItem
            //    {
            //        Text = u.Name,
            //        Value = u.Id.ToString()
            //    });
            //obj.CoverTypeList = _unitOfWork.covertype.GetAll().Select(
            //    u => new SelectListItem
            //    {
            //        Text = u.Name,
            //        Value = u.Id.ToString()
            //    });
            // --------------------------------------------------------

            return View(obj);
        }

        

        //post
        
        #region API_CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }
        [HttpDelete]
        //[ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                
                _unitOfWork.Company.Remove(obj);
                _unitOfWork.Save();

                return Json(new {sucess=true,message="Delete Successful"});
            }


            return View(obj);
        }
        #endregion
    }
}
