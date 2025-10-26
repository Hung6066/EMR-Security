// MedicalDocumentService.cs
namespace EMRSystem.Application.Services
{
    public class MedicalDocumentService : IMedicalDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public MedicalDocumentService(
            ApplicationDbContext context,
            IFileService fileService,
            IMapper mapper)
        {
            _context = context;
            _fileService = fileService;
            _mapper = mapper;
        }

        public async Task<MedicalDocumentDto> UploadDocumentAsync(
            int medicalRecordId, 
            IFormFile file, 
            string description, 
            int userId)
        {
            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null)
                throw new Exception("Medical record not found");

            var filePath = await _fileService.UploadFileAsync(file, $"medical-records/{medicalRecordId}");

            var document = new MedicalDocument
            {
                MedicalRecordId = medicalRecordId,
                FileName = file.FileName,
                FilePath = filePath,
                FileType = GetFileType(file.ContentType),
                ContentType = file.ContentType,
                FileSize = file.Length,
                Description = description,
                UploadedBy = userId,
                UploadedAt = DateTime.Now
            };

            _context.MedicalDocuments.Add(document);
            await _context.SaveChangesAsync();

            return _mapper.Map<MedicalDocumentDto>(document);
        }

        public async Task<IEnumerable<MedicalDocumentDto>> GetDocumentsByRecordIdAsync(int medicalRecordId)
        {
            var documents = await _context.MedicalDocuments
                .Include(d => d.UploadedByUser)
                .Where(d => d.MedicalRecordId == medicalRecordId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MedicalDocumentDto>>(documents);
        }

        public async Task<byte[]> DownloadDocumentAsync(int documentId)
        {
            var document = await _context.MedicalDocuments.FindAsync(documentId);
            if (document == null)
                throw new Exception("Document not found");

            return await _fileService.GetFileAsync(document.FilePath);
        }

        public async Task DeleteDocumentAsync(int documentId)
        {
            var document = await _context.MedicalDocuments.FindAsync(documentId);
            if (document == null)
                throw new Exception("Document not found");

            await _fileService.DeleteFileAsync(document.FilePath);
            _context.MedicalDocuments.Remove(document);
            await _context.SaveChangesAsync();
        }

        private string GetFileType(string contentType)
        {
            if (contentType.StartsWith("image/"))
                return "Image";
            if (contentType == "application/pdf")
                return "PDF";
            return "Document";
        }
    }
}