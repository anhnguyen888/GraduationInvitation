using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using GraduationInvitation3.Models;
using GraduationInvitation3.Services;
using SkiaSharp;

namespace GraduationInvitation3.Controllers
{
    public class InvitationController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly IWebHostEnvironment _environment;

        public InvitationController(ITemplateService templateService, IWebHostEnvironment environment)
        {
            _templateService = templateService;
            _environment = environment;
        }

        public IActionResult Create()
        {
            ViewBag.Templates = _templateService.GetAvailableTemplates();
            return View(new InvitationModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(InvitationModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Templates = _templateService.GetAvailableTemplates();
                return View(model);
            }

            try
            {
                var generatedImagePath = await GenerateInvitation(model);
                return RedirectToAction("Preview", new { imagePath = generatedImagePath });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error generating invitation: {ex.Message}");
                ViewBag.Templates = _templateService.GetAvailableTemplates();
                return View(model);
            }
        }

        public IActionResult Preview(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return RedirectToAction("Create");
            }

            var physicalPath = Path.Combine(_environment.WebRootPath, "generated", imagePath);
            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["Error"] = "Generated invitation not found.";
                return RedirectToAction("Create");
            }

            ViewData["ImagePath"] = imagePath; // Pass the image path to the view
            return View(); // Don't pass the model directly
        }


        [HttpGet]
        public IActionResult Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_environment.WebRootPath, "generated", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Use FileStream for larger files
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(fileStream, "image/png")
            {
                FileDownloadName = fileName
            };
        }

        [HttpPost]
        public async Task<IActionResult> UploadTemplate(TemplateModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please select a valid template file.";
                return RedirectToAction("Templates");
            }

            try
            {
                await _templateService.SaveTemplateAsync(model.TemplateFile);
                TempData["Success"] = "Template uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error uploading template: {ex.Message}";
            }

            return RedirectToAction("Templates");
        }

        public IActionResult Templates()
        {
            ViewBag.Templates = _templateService.GetAvailableTemplates();
            ViewBag.DefaultTemplate = _templateService.GetDefaultTemplate();
            return View();
        }

        [HttpPost]
        public IActionResult SetDefaultTemplate(string templateName)
        {
            try
            {
                _templateService.SetDefaultTemplate(templateName);
                TempData["Success"] = "Default template updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error setting default template: {ex.Message}";
            }
            return RedirectToAction("Templates");
        }

        private async Task<string> GenerateInvitation(InvitationModel model)
        {
            var templatePath = Path.Combine(_environment.WebRootPath, "templates",
                model.SelectedTemplate ?? _templateService.GetDefaultTemplate());

            if (!System.IO.File.Exists(templatePath))
            {
                throw new FileNotFoundException("Template file not found.");
            }

            var outputFileName = $"invitation_{DateTime.UtcNow.Ticks}.png";
            var outputPath = Path.Combine(_environment.WebRootPath, "generated", outputFileName);

            // Ensure the generated directory exists
            Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, "generated"));

            using (var bitmap = SKBitmap.Decode(templatePath))
            using (var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height)))
            {
                var canvas = surface.Canvas;
                canvas.DrawBitmap(bitmap, 0, 0);

                using (var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 48.0f,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                })
                {
                    // Draw the name
                    canvas.DrawText(model.FullName, 714, 158, paint);

                    // Draw the message with smaller text
                    paint.TextSize = 32.0f;
                    paint.Typeface = SKTypeface.FromFamilyName("Arial");
                    canvas.DrawText(model.Message, 257,477, paint);
                }

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await data.AsStream().CopyToAsync(stream);
                }
            }

            return outputFileName;
        }
    }
}