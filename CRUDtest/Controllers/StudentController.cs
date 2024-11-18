using CRUDtest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUDtest.Controllers
{
    public class StudentController : Controller
    {
        private ApplicationDbContext _context;
        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }
        public ActionResult Index()
        {
            var students = _context.Student.ToList();
            return View(students);
        }

        public ActionResult Details(Guid id)
        {
            var student = _context.Student.SingleOrDefault(s => s.Id == id);
            if (student == null) {
                return NotFound();
            }
            return View(student);
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student student)
        {
            if (ModelState.IsValid) {
                student.Id = Guid.NewGuid();
                _context.Student.Add(student);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(student);
        }

        public ActionResult Edit(Guid id)
        {
            var student = _context.Student.SingleOrDefault(s => s.Id == id);
            if (student == null) {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(student).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(student);
        }

        public ActionResult Delete(Guid id)
        {
            var student = _context.Student.SingleOrDefault(s => s.Id == id);
            if (student == null) {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            var student = _context.Student.Find(id);
            _context.Student.Remove(student);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
    }
}
