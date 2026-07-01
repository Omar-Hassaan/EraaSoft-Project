using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static HealthCareManagementSystem.Repositories.IRepository;

namespace HealthCareManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.ADMIN_AREA)]

    public class ClinicController : Controller
    {
        private readonly IRepository<Clinic> _clinicRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ClinicController(
            IRepository<Clinic> clinicRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _clinicRepository = clinicRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Index

        public async Task<IActionResult> Index()
        {
            var clinics =await _clinicRepository.GetAsync();

            return View(clinics);
        }

        #endregion

        #region Create

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClinicVM clinicVM)
        {
            if (!ModelState.IsValid)
                return View(clinicVM);

            // Check if clinic already exists
            var clinicExists =await _clinicRepository
                .GetOneAsync(c => c.Name == clinicVM.Name);

            if (clinicExists != null)
            {
                ModelState.AddModelError("Name", "Clinic already exists.");
                return View(clinicVM);
            }

            string? imageName = null;

            if (clinicVM.Image != null)
            {
                imageName = Guid.NewGuid() + Path.GetExtension(clinicVM.Image.FileName);

                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Clinics");

                Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, imageName);

                using FileStream stream = new(path, FileMode.Create);

                await clinicVM.Image.CopyToAsync(stream);
            }

            Clinic clinic = new()
            {
                Name = clinicVM.Name,
                Address = clinicVM.Address,
                PhoneNumber = clinicVM.PhoneNumber,
                Description = clinicVM.Description,
                ImageName = imageName
            };

            await _clinicRepository.AddAsync(clinic);
            await _clinicRepository.CommitAsync();

            TempData["Success-Notification"] = "Clinic created successfully.";

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Updete
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var clinic =await _clinicRepository.GetOneAsync(c => c.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            ClinicVM clinicVM = new()
            {
                Name = clinic.Name,
                Address = clinic.Address,
                PhoneNumber = clinic.PhoneNumber,
                Description = clinic.Description
            };

            return View(clinicVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, ClinicVM clinicVM)
        {
            if (!ModelState.IsValid)
            {
                return View(clinicVM);
            }

            var clinic =await _clinicRepository.GetOneAsync(c => c.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            clinic.Name = clinicVM.Name;
            clinic.Address = clinicVM.Address;
            clinic.PhoneNumber = clinicVM.PhoneNumber;
            clinic.Description = clinicVM.Description;

            if (clinicVM.Image != null)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Clinics");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                if (!string.IsNullOrEmpty(clinic.ImageName))
                {
                    string oldImage = Path.Combine(folder, clinic.ImageName);

                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }

                string imageName = Guid.NewGuid().ToString() + Path.GetExtension(clinicVM.Image.FileName);

                string imagePath = Path.Combine(folder, imageName);

                using FileStream stream = new(imagePath, FileMode.Create);

                await clinicVM.Image.CopyToAsync(stream);

                clinic.ImageName = imageName;
            }

            _clinicRepository.Update(clinic);
            await _clinicRepository.CommitAsync();

            TempData["Success-Notification"] = "Clinic updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _clinicRepository.GetOneAsync(c => c.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            return View(clinic);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, Clinic clinic)
        {
            var clinicFromDb =await _clinicRepository.GetOneAsync(c => c.Id == id);

            if (clinicFromDb == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(clinicFromDb.ImageName))
            {
                string imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "Images",
                    "Clinics",
                    clinicFromDb.ImageName);

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _clinicRepository.Delete(clinicFromDb);

            await _clinicRepository.CommitAsync();

            TempData["Success-Notification"] = "Clinic deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}