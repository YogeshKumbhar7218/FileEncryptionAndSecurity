using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;
using File_Encryption_backend.Data;
using File_Encryption_backend.Service;
using static System.Net.WebRequestMethods;
using System.Linq;
using File_Encryption_backend.Models;

namespace File_Encryption_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly FileEncryptor _fileEncryptor;

		public HomeController(ApplicationDbContext context, FileEncryptor fileEncryptor)
		{
			_context = context;
			_fileEncryptor = fileEncryptor;
		}
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> UploadFile(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("File not selected");

			var fileName = file.FileName;
			var documentType = Path.GetExtension(file.FileName);
			var documentId = Guid.NewGuid(); // Or get this value from request or any other source

			//using (var memoryStream = new MemoryStream())
			//{
			//	await file.CopyToAsync(memoryStream);

			var fileModel = new Models.Document
			{
				DocumentID = documentId,
				DocumentType = documentType,
				FileName = fileName,
				//FileContent = memoryStream.ToArray(),
				DateInserted = DateTime.Now
			};

			_context.documents.Add(fileModel);
			await _context.SaveChangesAsync();



			string outputFolder = @"Y:\Temp\File Encryption\encryptedFiles";
			string outputFileName = documentId.ToString() + ".enc";
			string outputFile = Path.Combine(outputFolder, outputFileName);
			try
			{
				_fileEncryptor.EncryptFile(file, outputFile);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}

			//}

			return Ok();
		}

		//[HttpGet("download/{id}")]
		//public IActionResult DownloadDecryptedFile(Guid id)
		//{
		//	string inputFile = @"Y:\Temp\File Encryption\encryptedFiles"; // Change this to the path of your encrypted file
		//	Document doc = _context.documents.Where(x => x.DocumentID == id).FirstOrDefault();
		//	inputFile = Path.Combine(inputFile, (doc.DocumentID.ToString() + ".enc"));

		//	byte[] decryptedFileBytes = _fileEncryptor.DecryptFile(inputFile);

		//	return File(decryptedFileBytes, "image/png", doc.FileName);
		//}

		[HttpGet("download/{id}")]
		public IActionResult DownloadDecryptedFile(Guid id)
		{
			string inputFileDirectory = @"Y:\Temp\File Encryption\encryptedFiles";
			Document doc = _context.documents.FirstOrDefault(x => x.DocumentID == id);

			if (doc == null)
			{
				return NotFound(); // Handle if the document is not found
			}

			string inputFile = Path.Combine(inputFileDirectory, (doc.DocumentID.ToString() + ".enc"));

			byte[] decryptedFileBytes = _fileEncryptor.DecryptFile(inputFile);

			// Write decrypted file to disk
			string outputFileDirectory = @"Y:\Temp\File Encryption\decryptedFiles";
			string outputFile = Path.Combine(outputFileDirectory, doc.FileName);

			try
			{
				Directory.CreateDirectory(outputFileDirectory); // Create directory if it doesn't exist
				System.IO.File.WriteAllBytes(outputFile, decryptedFileBytes);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"An error occurred while writing the decrypted file: {ex.Message}");
			}

			// Return the decrypted file
			return PhysicalFile(outputFile, "application/octet-stream", doc.FileName);
		}
	}

}
