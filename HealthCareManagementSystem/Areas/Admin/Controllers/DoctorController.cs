using HealthCareManagementSystem.Repositories;
using HealthCareManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static HealthCareManagementSystem.Repositories.IRepository;

namespace HealthCareManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.ADMIN_AREA)]
    public class DoctorController : Controller
    {
        private readonly IRepository<Clinic> _clinicRepository;
        private readonly IRepository<Doctor> _doctorRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DoctorController(IRepository<Clinic> clinicRepository, IRepository<Doctor> doctorRepository, IWebHostEnvironment webHostEnvironment)
        {
            _clinicRepository = clinicRepository;
            _doctorRepository = doctorRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var doctors = await _doctorRepository.GetAsync(
                includes: [d => d.Clinic]);

            return View(doctors);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Clinics = new SelectList(
                await _clinicRepository.GetAsync(),
                "Id",
                "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorVM doctorVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clinics = new SelectList(
                    await _clinicRepository.GetAsync(),
                    "Id",
                    "Name");

                return View(doctorVM);
            }

            string? imageName = null;

            if (doctorVM.Image != null)
            {
                string folder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images",
                    "Doctors");

                Directory.CreateDirectory(folder);

                imageName = Guid.NewGuid().ToString()
                            + Path.GetExtension(doctorVM.Image.FileName);

                string imagePath = Path.Combine(folder, imageName);

                using FileStream stream = new(imagePath, FileMode.Create);

                await doctorVM.Image.CopyToAsync(stream);
            }

            Doctor doctor = new()
            {
                Name = doctorVM.Name,
                Specialization = doctorVM.Specialization,
                ClinicId = doctorVM.ClinicId,
                ImageName = imageName

                // هتضيفها لما تضيف ImageName فى الموديل
                // ImageName = imageName
            };

            await _doctorRepository.AddAsync(doctor);

            await _doctorRepository.CommitAsync();

            TempData["Success-Notification"] = "Doctor Added Successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var doctor = await _doctorRepository.GetOneAsync(
                d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            ViewBag.Clinics = new SelectList(
                await _clinicRepository.GetAsync(),
                "Id",
                "Name",
                doctor.ClinicId);

            DoctorVM doctorVM = new()
            {
                Name = doctor.Name,
                Specialization = doctor.Specialization,
                ClinicId = doctor.ClinicId
            };

            return View(doctorVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, DoctorVM doctorVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clinics = new SelectList(
                    await _clinicRepository.GetAsync(),
                    "Id",
                    "Name",
                    doctorVM.ClinicId);

                return View(doctorVM);
            }

            var doctor = await _doctorRepository.GetOneAsync(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            doctor.Name = doctorVM.Name;
            doctor.Specialization = doctorVM.Specialization;
            doctor.ClinicId = doctorVM.ClinicId;

            if (doctorVM.Image != null)
            {
                string folder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images",
                    "Doctors");

                Directory.CreateDirectory(folder);

                if (!string.IsNullOrEmpty(doctor.ImageName))
                {
                    string oldImage = Path.Combine(folder, doctor.ImageName);

                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }

                string imageName = Guid.NewGuid().ToString()
                                 + Path.GetExtension(doctorVM.Image.FileName);

                string imagePath = Path.Combine(folder, imageName);

                using FileStream stream = new(imagePath, FileMode.Create);

                await doctorVM.Image.CopyToAsync(stream);

                doctor.ImageName = imageName;
            }

            _doctorRepository.Update(doctor);

            await _doctorRepository.CommitAsync();

            TempData["Success-Notification"] = "Doctor Updated Successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _doctorRepository.GetOneAsync(
                d => d.Id == id,
                includes: [d => d.Clinic]);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, Doctor doctor)
        {
            var doctorFromDb = await _doctorRepository.GetOneAsync(
                d => d.Id == id);

            if (doctorFromDb == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(doctorFromDb.ImageName))
            {
                string imagePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images",
                    "Doctors",
                    doctorFromDb.ImageName);

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _doctorRepository.Delete(doctorFromDb);

            await _doctorRepository.CommitAsync();

            TempData["Success-Notification"] = "Doctor Deleted Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
