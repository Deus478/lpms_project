using Swashbuckle.AspNetCore.Filters;
using LegalCaseManagement.DTOs;
using System;
using System.Collections.Generic;

namespace lpms_server.Swagger.Examples
{
    public class CreateCaseDtoExample : IExamplesProvider<CreateCaseDto>
    {
        public CreateCaseDto GetExamples() => new CreateCaseDto
        {
            CaseNumber = "CASE-2025-001",
            Title = "Acme vs. Globex",
            Description = "Breach of contract",
            AssignedLawyerId = 1,
            CourtId = 1,
            DateFiled = DateTime.UtcNow.Date,
            Status = "Active",
            Parties = new List<CasePartyDto>
            {
                new CasePartyDto { PartyId = 1, Role = "Plaintiff" },
                new CasePartyDto { PartyId = 2, Role = "Defendant" }
            }
        };
    }

    public class UpdateCaseDtoExample : IExamplesProvider<UpdateCaseDto>
    {
        public UpdateCaseDto GetExamples() => new UpdateCaseDto
        {
            Title = "Acme vs. Globex (Amended)",
            Status = "Pending",
            Outcome = null
        };
    }
}


