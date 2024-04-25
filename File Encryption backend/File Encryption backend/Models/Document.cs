using System;

namespace File_Encryption_backend.Models
{
	public class Document
	{
		public int Id { get; set; } // Unique identifier column with ROWGUIDCOL property
		public Guid DocumentID { get; set; }
		public string FileName { get; set; }
		public string DocumentType { get; set; }
		//public byte[] FileContent { get; set; }
		public DateTime DateInserted { get; set; }
	}
}
