using Swashbuckle.AspNetCore.Filters;
using LegalCaseManagement.DTOs;
using System;
using System.Collections.Generic;

namespace lpms_server.Swagger.Examples
{
    public class CreateMeetingDtoExample : IExamplesProvider<CreateMeetingDto>
    {
        public CreateMeetingDto GetExamples() => new CreateMeetingDto
        {
            BoardId = 1,
            CommitteeId = null,
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            Location = "Head Office Boardroom",
            Title = "Q4 Strategy Review",
            Agenda = "Budget, KPIs, Risk"
        };
    }

    public class RecordAttendanceDtoExample : IExamplesProvider<RecordAttendanceDto>
    {
        public RecordAttendanceDto GetExamples() => new RecordAttendanceDto
        {
            Items = new List<AttendanceItemDto>
            {
                new AttendanceItemDto { AttendeeName = "Jane Doe", AttendeeRole = "Director", Present = true },
                new AttendanceItemDto { AttendeeName = "John Smith", AttendeeRole = "Company Secretary", Present = true }
            }
        };
    }

    public class CreateMinuteDtoExample : IExamplesProvider<CreateMinuteDto>
    {
        public CreateMinuteDto GetExamples() => new CreateMinuteDto
        {
            Content = "The board discussed the quarterly performance and approved the budget.",
            DocumentId = null
        };
    }

    public class CreateResolutionDtoExample : IExamplesProvider<CreateResolutionDto>
    {
        public CreateResolutionDto GetExamples() => new CreateResolutionDto
        {
            Title = "Approve FY Budget",
            Description = "Approve the proposed FY budget of $10M",
            DueDate = DateTime.UtcNow.AddDays(30),
            ResponsibleParty = "CFO"
        };
    }

    public class UpdateResolutionStatusDtoExample : IExamplesProvider<UpdateResolutionStatusDto>
    {
        public UpdateResolutionStatusDto GetExamples() => new UpdateResolutionStatusDto
        {
            Status = "InProgress",
            CompletedAt = null
        };
    }
}


