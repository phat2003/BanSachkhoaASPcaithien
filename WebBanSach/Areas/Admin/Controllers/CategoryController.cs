using BanSach.DataAccess.Data;
using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using Microsoft.AspNetCore.Mvc;


namespace WebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;//biến này chỉ được đọc(không được ghi hay làm gì khác).
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //action đưa index lên html
        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _unitOfWork.Category.GetAll();
            return View(objCategoryList);
        }
        //action view create
        public IActionResult Create()
        {
            return View();
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("DisplayOrder", "The Name must not same displayorder");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);//thêm đối tượng create
                _unitOfWork.Save();//lưu thay đổi của database
                TempData["Sucess"] = "Category Create sucessfully";//hiện thông báo tạm thời là thành công
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
            //var categoryfromDb = _db.Categories.Find(id);//tạo biến var categoryfromDb và cho = find id để tìm tới id của nó trong database
            var categoryfromDbFirst = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            //var categoryfromDbsingle = _db.Categories.SingleOrDefault(u => u.Id == id);
            if (categoryfromDbFirst == null)//ở đây do đã set categoryfromDb = id nên id null hoặc = 0 thì categoryfromDb cũng null và trả về notfound giống id.
            {
                return NotFound();
            }
            return View(categoryfromDbFirst);//trả về view dù cho có đáp ứng 2 điều kiện trên hay không.
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Name must not same displayorder");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["Sucess"] = "Category Edit sucessfully";
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
            var categoryfromDb = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            //var categoryfromDbFirst = _db.Categories.FirstOrDefault(u=>u.Id==id);
            //var categoryfromDbsingle = _db.Categories.SingleOrDefault(u => u.Id == id);
            if (categoryfromDb == null)
            {
                return NotFound();
            }
            return View(categoryfromDb);
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]//lệnh này dùng để chống giả mạo về method này
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);
            //var categoryfromDbFirst = _db.Categories.FirstOrDefault(u=>u.Id==id);
            //var categoryfromDbsingle = _db.Categories.SingleOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.Category.Remove(obj);
                _unitOfWork.Save();
                TempData["Sucess"] = "Category Delete sucessfully";
                return RedirectToAction("index");
            }
            return View(obj);
        }
    }
}
