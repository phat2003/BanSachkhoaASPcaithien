using BanSach.DataAccess.Data;
using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using Microsoft.AspNetCore.Mvc;


namespace WebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;//biến này chỉ được đọc(không được ghi hay làm gì khác).
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //action đưa index lên html
        public IActionResult Index()
        {
            IEnumerable<CoverType> objCoverTypeList = _unitOfWork.covertype.GetAll();
            return View(objCoverTypeList);
        }
        //action view create
        public IActionResult Create()
        {
            return View();
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult Create(CoverType obj)
        {
            
            if (ModelState.IsValid)
            {
                _unitOfWork.covertype.Add(obj);//thêm đối tượng create
                _unitOfWork.Save();//lưu thay đổi của database
                TempData["Sucess"] = "CoverType Create sucessfully";//hiện thông báo tạm thời là thành công
                return RedirectToAction("index");//trả về action index.
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)//nếu id null hoặc = 0 thì trả về notfound (không tìm thấy).
            {
                return NotFound();
            }
            
            var covertTypefromDbFirst = _unitOfWork.covertype.GetFirstOrDefault(u => u.Id == id);
            
            if (covertTypefromDbFirst == null)//ở đây do đã set categoryfromDb = id nên id null hoặc = 0 thì categoryfromDb cũng null và trả về notfound giống id.
            {
                return NotFound();
            }
            return View(covertTypefromDbFirst);//trả về view dù cho có đáp ứng 2 điều kiện trên hay không.
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.covertype.Update(obj);
                _unitOfWork.Save();
                TempData["Sucess"] = "Cover Type Edit sucessfully";
                return RedirectToAction("index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverTypefromDb = _unitOfWork.covertype.GetFirstOrDefault(u => u.Id == id);
            
            if (coverTypefromDb == null)
            {
                return NotFound();
            }
            return View(coverTypefromDb);
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitOfWork.covertype.GetFirstOrDefault(u => u.Id == id);
            
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.covertype.Remove(obj);
                _unitOfWork.Save();
                TempData["Sucess"] = "Covert Type Delete sucessfully";
                return RedirectToAction("index");
            }
            return View(obj);
        }
    }
}
