using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace File_Encryption_backend.Service
{
	public class FileEncryptor
	{
		private readonly string key = "Th15IsAVal1dAESK3y1234567890kjhG"; // Make sure the key is 32 bytes long

		public void EncryptFile(IFormFile inputFile, string outputFile)
		{
			if (key.Length != 32)
			{
				throw new ArgumentException("Invalid key length. The key must be 32 bytes long for AES-256 encryption.");
			}

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = Encoding.UTF8.GetBytes(key);
				aesAlg.GenerateIV(); // Generate a random IV for each encryption

				using (var outputStream = new FileStream(outputFile, FileMode.Create))
				{
					// Write the IV to the beginning of the file
					outputStream.Write(aesAlg.IV, 0, aesAlg.IV.Length);

					using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
					using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
					{
						// Copy the input file stream to the CryptoStream
						inputFile.CopyTo(cryptoStream);
					}
				}
			}
		}
		public byte[] DecryptFile(string inputFile)
		{
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = Encoding.UTF8.GetBytes(key);

				byte[] iv = new byte[aesAlg.BlockSize / 8];
				using (var inputFileStream = new FileStream(inputFile, FileMode.Open))
				{
					inputFileStream.Read(iv, 0, iv.Length);
					aesAlg.IV = iv;

					// Seek to the position after IV
					inputFileStream.Seek(iv.Length, SeekOrigin.Begin);

					using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
					using (var memoryStream = new MemoryStream())
					using (var cryptoStream = new CryptoStream(inputFileStream, decryptor, CryptoStreamMode.Read))
					{
						cryptoStream.CopyTo(memoryStream);
						return memoryStream.ToArray();
					}
				}
			}
		}
	}
}
