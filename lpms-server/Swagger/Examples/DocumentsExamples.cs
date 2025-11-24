using Swashbuckle.AspNetCore.Filters;
using DocumentManagement.DTOs;
using System;

namespace lpms_server.Swagger.Examples
{
    public class DocumentResponseDtoExample : IExamplesProvider<DocumentResponseDto>
    {
        public DocumentResponseDto GetExamples() => new DocumentResponseDto
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            FileName = "contract.pdf",
            FileExtension = ".pdf",
            FileSizeBytes = 123456,
            Description = "Signed contract",
            UploadedBy = "user-123",
            UploadedDate = DateTime.UtcNow,
            IsArchived = false,
            DocumentType = "Agreement"
        };
    }
}


