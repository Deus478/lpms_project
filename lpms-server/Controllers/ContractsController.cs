using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Linq;
using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using LegalCaseManagement.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace LegalCaseManagement.Controllers
{
    /// <summary>
    /// Controller for managing contracts in the LCMS system
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ContractsController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(LegalCaseDbContext context, IMapper mapper, ILogger<ContractsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all contracts
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetContracts()
        {
            var contracts = await _context.Contracts
                .Include(c => c.ContractDocuments)
                    .ThenInclude(cd => cd.Document)
                .Include(c => c.ContractApprovals)
                    .ThenInclude(a => a.ApproverUser)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<List<ContractDto>>(contracts));
        }

        /// <summary>
        /// Get a specific contract by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDto>> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.ContractDocuments)
                    .ThenInclude(cd => cd.Document)
                .Include(c => c.ContractApprovals)
                    .ThenInclude(a => a.ApproverUser)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null)
                return NotFound();

            return Ok(_mapper.Map<ContractDto>(contract));
        }

        /// <summary>
        /// Create a new contract request
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContractDto>> CreateContract(CreateContractDto createContractDto)
        {
            var contract = _mapper.Map<Contract>(createContractDto);
            contract.ContractNumber = GenerateContractNumber();

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            // Initialize approval workflow based on contract value and risk
            await InitializeApprovalWorkflow(contract);

            await _context.Entry(contract)
                .Collection(c => c.ContractDocuments)
                .Query()
                .Include(cd => cd.Document)
                .LoadAsync();

            return CreatedAtAction(nameof(GetContract), new { id = contract.ContractId }, _mapper.Map<ContractDto>(contract));
        }

        /// <summary>
        /// Update contract status
        /// </summary>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateContractStatus(int id, UpdateContractStatusDto updateDto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            contract.Status = updateDto.Status;
            contract.UpdatedAt = DateTime.UtcNow;

            if (updateDto.Status == "Executed")
            {
                contract.ExecutionDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Add document to contract
        /// </summary>
        [HttpPost("{id}/documents")]
        [ProducesResponseType(typeof(ContractDocumentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDocumentDto>> AddDocument(int id, AddContractDocumentDto addDocumentDto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            var document = await _context.Documents.FindAsync(addDocumentDto.DocumentId);
            if (document == null)
                return BadRequest("Document not found");

            var contractDocument = new ContractDocument
            {
                ContractId = id,
                DocumentId = addDocumentDto.DocumentId,
                DocumentType = addDocumentDto.DocumentType,
                Description = addDocumentDto.Description
            };

            _context.ContractDocuments.Add(contractDocument);
            await _context.SaveChangesAsync();

            await _context.Entry(contractDocument)
                .Reference(cd => cd.Contract)
                .LoadAsync();
            await _context.Entry(contractDocument)
                .Reference(cd => cd.Document)
                .LoadAsync();

            return CreatedAtAction(nameof(GetContract), new { id = id }, _mapper.Map<ContractDocumentDto>(contractDocument));
        }

        /// <summary>
        /// Submit contract for approval
        /// </summary>
        [HttpPost("{id}/approvals")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractApprovalDto>> SubmitForApproval(int id, SubmitForApprovalDto submitDto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            var approval = new ContractApproval
            {
                ContractId = id,
                ApproverUserId = submitDto.ApproverUserId,
                ApprovalLevel = submitDto.ApprovalLevel,
                Status = "Pending",
                Comments = submitDto.Comments
            };

            _context.ContractApprovals.Add(approval);
            await _context.SaveChangesAsync();

            await _context.Entry(approval)
                .Reference(a => a.Contract)
                .LoadAsync();
            await _context.Entry(approval)
                .Reference(a => a.ApproverUser)
                .LoadAsync();

            return CreatedAtAction(nameof(GetContract), new { id = id }, _mapper.Map<ContractApprovalDto>(approval));
        }

        /// <summary>
        /// Get contracts due for renewal
        /// </summary>
        [HttpGet("renewals/due")]
        [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetContractsDueForRenewal()
        {
            var warningPeriod = DateTime.UtcNow.AddDays(90); // 90 days warning

            var contracts = await _context.Contracts
                .Where(c => c.EndDate.HasValue &&
                           c.EndDate.Value <= warningPeriod &&
                           c.EndDate.Value > DateTime.UtcNow &&
                           c.Status == "Active")
                .Include(c => c.ContractDocuments)
                    .ThenInclude(cd => cd.Document)
                .ToListAsync();

            return Ok(_mapper.Map<List<ContractDto>>(contracts));
        }

        private string GenerateContractNumber()
        {
            var year = DateTime.UtcNow.Year;
            var sequence = _context.Contracts.Count() + 1;
            return $"CON-{year}-{sequence:D5}";
        }

        private async Task InitializeApprovalWorkflow(Contract contract)
        {
            // Determine approval levels based on contract value and risk
            var approvalLevels = new List<string>();

            if (contract.ContractValue >= 1000000) // > $1M
                approvalLevels.Add("Board");
            else if (contract.ContractValue >= 100000) // > $100K
                approvalLevels.Add("ExecutiveManagement");

            if (contract.RiskLevel == "High" || contract.RiskLevel == "Critical")
                approvalLevels.Add("HeadOfLegal");

            approvalLevels.Add("LegalOfficer"); // Always need legal review

            foreach (var level in approvalLevels.Distinct())
            {
                var approval = new ContractApproval
                {
                    ContractId = contract.ContractId,
                    ApprovalLevel = level,
                    Status = "Pending"
                };
                _context.ContractApprovals.Add(approval);
            }

            await _context.SaveChangesAsync();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
