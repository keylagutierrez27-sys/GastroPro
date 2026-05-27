using Microsoft.AspNetCore.Mvc;
using GastroPro.Domain.Interfaces;
using GastroPro.Domain.Entities;

namespace GastroPro.Web.Controllers
{
    public class PlatosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PlatosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Platos
        public async Task<IActionResult> Index()
        {
            var platos = await _unitOfWork.GetPlatosAsync();
            return View(platos);
        }

        // GET: /Platos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Platos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plato plato)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.AddPlatoAsync(plato);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(plato);
        }

        // GET: /Platos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var plato = await _unitOfWork.GetPlatoByIdAsync(id);
            if (plato == null) return NotFound();
            return View(plato);
        }

        // POST: /Platos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Plato plato)
        {
            if (id != plato.PlatoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.UpdatePlato(plato);
                    await _unitOfWork.CompleteAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "No se pudo actualizar el precio en SQL Server.");
                }
            }
            return View(plato);
        }

        // GET: /Platos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var plato = await _unitOfWork.GetPlatoByIdAsync(id);
            if (plato == null) return NotFound();
            return View(plato);
        }

        // POST: /Platos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plato = await _unitOfWork.GetPlatoByIdAsync(id);
            if (plato != null)
            {
                _unitOfWork.RemovePlato(plato);
                await _unitOfWork.CompleteAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}