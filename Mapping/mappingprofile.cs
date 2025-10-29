using AutoMapper;
using LegalCaseManagement.Models;
using LegalCaseManagement.DTOs;

namespace LegalCaseManagement.Mapping
{
    /// <summary>
    /// AutoMapper profile for mapping between entities and DTOs
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Case mappings
            CreateMap<Case, CaseSummaryDto>()
                .ForMember(dest => dest.LawyerName, opt => opt.MapFrom(src => src.AssignedLawyer.FullName))
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Court.Name));

            CreateMap<Case, CaseDetailDto>()
                .ForMember(dest => dest.Parties, opt => opt.MapFrom(src => src.CaseParties));

            CreateMap<CreateCaseDto, Case>()
                .ForMember(dest => dest.CaseId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.AssignedLawyer, opt => opt.Ignore())
                .ForMember(dest => dest.Court, opt => opt.Ignore())
                .ForMember(dest => dest.Hearings, opt => opt.Ignore())
                .ForMember(dest => dest.Deadlines, opt => opt.Ignore())
                .ForMember(dest => dest.CaseParties, opt => opt.Ignore());

            // Lawyer mappings
            CreateMap<Lawyer, LawyerDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

            CreateMap<CreateLawyerDto, Lawyer>()
                .ForMember(dest => dest.LawyerId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Cases, opt => opt.Ignore());

            // Court mappings
            CreateMap<Court, CourtDto>();
            CreateMap<CreateCourtDto, Court>()
                .ForMember(dest => dest.CourtId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Cases, opt => opt.Ignore())
                .ForMember(dest => dest.Hearings, opt => opt.Ignore());

            // Party mappings
            CreateMap<Party, PartyDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

            CreateMap<CreatePartyDto, Party>()
                .ForMember(dest => dest.PartyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CaseParties, opt => opt.Ignore());

            // CaseParty mappings
            CreateMap<CaseParty, CasePartyDetailDto>();
            CreateMap<CasePartyDto, CaseParty>()
                .ForMember(dest => dest.CasePartyId, opt => opt.Ignore())
                .ForMember(dest => dest.CaseId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Case, opt => opt.Ignore())
                .ForMember(dest => dest.Party, opt => opt.Ignore());

            // Hearing mappings
            CreateMap<Hearing, HearingDto>();
            CreateMap<CreateHearingDto, Hearing>()
                .ForMember(dest => dest.HearingId, opt => opt.Ignore())
                .ForMember(dest => dest.CaseId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Scheduled"))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Case, opt => opt.Ignore())
                .ForMember(dest => dest.Court, opt => opt.Ignore());

            // Deadline mappings
            CreateMap<Deadline, DeadlineDto>();
            CreateMap<CreateDeadlineDto, Deadline>()
                .ForMember(dest => dest.DeadlineId, opt => opt.Ignore())
                .ForMember(dest => dest.CaseId, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Case, opt => opt.Ignore());

            // Report mappings
            CreateMap<Case, CaseReportItemDto>()
                .ForMember(dest => dest.LawyerName, opt => opt.MapFrom(src => src.AssignedLawyer.FullName))
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Court.Name))
                .ForMember(dest => dest.TotalHearings, opt => opt.MapFrom(src => src.Hearings.Count))
                .ForMember(dest => dest.CompletedHearings, opt => opt.MapFrom(src => src.Hearings.Count(h => h.Status == "Completed")))
                .ForMember(dest => dest.TotalDeadlines, opt => opt.MapFrom(src => src.Deadlines.Count))
                .ForMember(dest => dest.CompletedDeadlines, opt => opt.MapFrom(src => src.Deadlines.Count(d => d.IsCompleted)))
                .ForMember(dest => dest.OverdueDeadlines, opt => opt.MapFrom(src => src.Deadlines.Count(d => !d.IsCompleted && d.DueDate < DateTime.UtcNow)))
                .ForMember(dest => dest.NextHearingDate, opt => opt.MapFrom(src => src.Hearings
                    .Where(h => h.Status == "Scheduled" && h.Date > DateTime.UtcNow)
                    .OrderBy(h => h.Date)
                    .Select(h => h.Date)
                    .FirstOrDefault()))
                .ForMember(dest => dest.NextDeadlineDate, opt => opt.MapFrom(src => src.Deadlines
                    .Where(d => !d.IsCompleted && d.DueDate > DateTime.UtcNow)
                    .OrderBy(d => d.DueDate)
                    .Select(d => d.DueDate)
                    .FirstOrDefault()));

            CreateMap<Lawyer, LawyerCaseloadDto>()
                .ForMember(dest => dest.LawyerName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization ?? "General"))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.IsActive)))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.IsActive && c.Status == "Active")))
                .ForMember(dest => dest.ClosedCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.IsActive && c.Status == "Closed")))
                .ForMember(dest => dest.UpcomingHearings, opt => opt.MapFrom(src => src.Cases
                    .SelectMany(c => c.Hearings)
                    .Count(h => h.Status == "Scheduled" && h.Date > DateTime.UtcNow && h.Date <= DateTime.UtcNow.AddDays(30))))
                .ForMember(dest => dest.OverdueDeadlines, opt => opt.MapFrom(src => src.Cases
                    .SelectMany(c => c.Deadlines)
                    .Count(d => !d.IsCompleted && d.DueDate < DateTime.UtcNow)));

            CreateMap<Court, CourtCaseloadDto>()
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CourtType, opt => opt.MapFrom(src => src.Type ?? "General"))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.IsActive)))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.IsActive && c.Status == "Active")))
                .ForMember(dest => dest.UpcomingHearings, opt => opt.MapFrom(src => src.Hearings
                    .Count(h => h.Status == "Scheduled" && h.Date > DateTime.UtcNow && h.Date <= DateTime.UtcNow.AddDays(30))));
        }
    }
}