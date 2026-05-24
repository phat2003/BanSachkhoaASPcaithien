using BanSach.DataAccess.Repository.IRepository;
using BanSach.Models;
using BanSach.Models.ViewModel;
using BanSach.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy danh sách tất cả người dùng
            var users = _userManager.Users.ToList();

            // 2. Tạo một danh sách rỗng để chứa dữ liệu hiển thị (ViewModel)
            var userRoleVMs = new List<UserRoleVM>();

            // 3. Duyệt qua từng người dùng để lấy thêm Role
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userRoleVMs.Add(new UserRoleVM
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    // Một người có thể có nhiều role, ta ghép chúng lại bằng dấu phẩy
                    Role = string.Join(", ", roles)
                });
            }

            // 4. Trả danh sách này về cho View
            return View(userRoleVMs);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string userId)
        {
            // Lấy thông tin user hiện tại
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == userId);

            // 1. Kiểm tra sự tồn tại của Admin
            var admins = await _userManager.GetUsersInRoleAsync(SD.Role_User_Admin);
            bool adminExists = admins.Any();

            // 2. Lọc danh sách Role
            //var roleList = _roleManager.Roles
            //    .Where(u => !adminExists || u.Name != SD.Role_User_Admin)
            //    .Select(x => new SelectListItem
            //    {
            //        Text = x.Name,
            //        Value = x.Name
            //    });

            // Nạp dữ liệu vào ViewModel
            RoleManagementVM roleVM = new RoleManagementVM()
            {
                ApplicationUser = user,
                RoleList = _roleManager.Roles.Where(u => !adminExists || u.Name != SD.Role_User_Admin).Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            

            // Lấy Role hiện tại của User (bạn có thể đang dùng cách truy vấn trực tiếp vào Db)
            roleVM.ApplicationUser.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

            return View(roleVM);
        }

        [HttpPost]
        public IActionResult EditRole(RoleManagementVM roleVM)
        {
            // Lấy thông tin user cũ từ database
            var applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == roleVM.ApplicationUser.Id);
            var oldRole = _userManager.GetRolesAsync(applicationUser).GetAwaiter().GetResult().FirstOrDefault();



            // Nếu có sự thay đổi về Role
            if (!(roleVM.ApplicationUser.Role == oldRole))
            {
                if (roleVM.ApplicationUser.Role == "Company") // (Bạn có thể dùng SD.Role_User_Comp nếu có file SD)
                {
                    // Nếu phân quyền mới là Company, lưu lại CompanyId từ thẻ select
                    applicationUser.CompanyId = roleVM.ApplicationUser.CompanyId;
                }
                if (oldRole == "Company")
                {
                    // Nếu role cũ là Company mà chuyển sang role khác, hủy bỏ CompanyId
                    applicationUser.CompanyId = null;
                }

                // Cập nhật Database
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                // Xóa Role cũ, thêm Role mới cho Identity
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                // Nếu Role không đổi, nhưng họ đổi Công ty khác
                if (oldRole == "Company" && applicationUser.CompanyId != roleVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            TempData["success"] = "Cập nhật quyền thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string userId)
        {
            // 1. Tìm người dùng
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // 2. Lấy Role hiện tại (Giả sử dự án của bạn mỗi người chỉ có 1 Role tại một thời điểm)
            var oldRoles = await _userManager.GetRolesAsync(user);
            var oldRole = oldRoles.FirstOrDefault();

            // 3. Lấy tất cả Roles trong hệ thống và chuyển đổi thành dạng danh sách thả xuống (SelectListItem)
            var roles = _roleManager.Roles.ToList();
            var roleList = roles.Select(x => new SelectListItem
            {
                Text = x.Name, // Chữ hiển thị cho Admin thấy (VD: "Staff")
                Value = x.Name // Giá trị thực sự gửi về Server
            });

            // 4. Đóng gói tất cả vào ViewModel
            var roleVM = new RoleManagementVM
            {
                User = user,
                OldRole = oldRole,
                RoleList = roleList
            };

            // 5. Gửi ra View
            return View(roleVM);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index");
            return View();
        }

    }
}
