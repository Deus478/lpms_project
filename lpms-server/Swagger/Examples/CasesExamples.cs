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
            PlaintiffLawyerId = 1,
            DefendantLawyerId = 2,
            CourtId = 1,
            DateFiled = DateTime.UtcNow.Date,
            Parties = new List<CasePartyDto>
            {
                new CasePartyDto { PartyId = 1, Role = "Plaintiff", Side = "Accuser" },
                new CasePartyDto { PartyId = 2, Role = "Defendant", Side = "Accused" }
            },
            AdditionalLawyerIds = new List<int>
            {
                3
            }
        };
    }

    public class UpdateCaseDtoExample : IExamplesProvider<UpdateCaseDto>
    {
        public UpdateCaseDto GetExamples() => new UpdateCaseDto
        {
            Title = "Acme vs. Globex (Amended)",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(1),
            Outcome = null
        };
    }
}


