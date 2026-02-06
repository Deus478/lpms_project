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
    /// Controller for managing case workflows and approvals
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WorkflowsController : ControllerBase
    {
        private readonly LegalCaseDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkflowsController> _logger;

        public WorkflowsController(LegalCaseDbContext context, IMapper mapper, ILogger<WorkflowsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get workflow templates
        /// </summary>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(IEnumerable<WorkflowTemplateDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkflowTemplateDto>>> GetWorkflowTemplates()
        {
            var templates = await _context.WorkflowTemplates
                .Where(t => t.IsActive)
                .Include(t => t.StepTemplates)
                .ToListAsync();

            // Order the step templates for each template
            foreach (var template in templates)
            {
                template.StepTemplates = template.StepTemplates.OrderBy(st => st.StepOrder).ToList();
            }

            return Ok(_mapper.Map<List<WorkflowTemplateDto>>(templates));
        }

        /// <summary>
        /// Create a new workflow template
        /// </summary>
        [HttpPost("templates")]
        [ProducesResponseType(typeof(WorkflowTemplateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WorkflowTemplateDto>> CreateWorkflowTemplate(CreateWorkflowTemplateDto createDto)
        {
            var template = _mapper.Map<WorkflowTemplate>(createDto);
            _context.WorkflowTemplates.Add(template);
            await _context.SaveChangesAsync();

            await _context.Entry(template)
                .Collection(t => t.StepTemplates)
                .LoadAsync();

            // Order the step templates
            template.StepTemplates = template.StepTemplates.OrderBy(st => st.StepOrder).ToList();

            return CreatedAtAction(nameof(GetWorkflowTemplates), new { }, _mapper.Map<WorkflowTemplateDto>(template));
        }

        /// <summary>
        /// Start workflow for a case
        /// </summary>
        [HttpPost("cases/{caseId}/start")]
        [ProducesResponseType(typeof(CaseWorkflowDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CaseWorkflowDto>> StartCaseWorkflow(int caseId, StartWorkflowDto startDto)
        {
            var caseEntity = await _context.Cases.FindAsync(caseId);
            if (caseEntity == null)
                return NotFound();

            var template = await _context.WorkflowTemplates
                .Include(t => t.StepTemplates)
                .FirstOrDefaultAsync(t => t.WorkflowTemplateId == startDto.WorkflowTemplateId);

            if (template == null)
                return BadRequest("Workflow template not found");

            // Order the step templates
            template.StepTemplates = template.StepTemplates.OrderBy(st => st.StepOrder).ToList();

            var workflow = new CaseWorkflow
            {
                CaseId = caseId,
                WorkflowTemplateId = startDto.WorkflowTemplateId,
                Status = "In Progress"
            };

            _context.CaseWorkflows.Add(workflow);
            await _context.SaveChangesAsync();

            // Create workflow steps
            foreach (var stepTemplate in template.StepTemplates)
            {
                var workflowStep = new CaseWorkflowStep
                {
                    CaseWorkflowId = workflow.CaseWorkflowId,
                    WorkflowStepTemplateId = stepTemplate.WorkflowStepTemplateId,
                    StepName = stepTemplate.StepName,
                    StepOrder = stepTemplate.StepOrder,
                    ApproverRole = stepTemplate.ApproverRole,
                    Status = "Pending",
                    DueDate = stepTemplate.TimeoutHours.HasValue ? 
                        DateTime.UtcNow.AddHours(stepTemplate.TimeoutHours.Value) : null
                };

                _context.CaseWorkflowSteps.Add(workflowStep);
            }

            await _context.SaveChangesAsync();

            // Start first step
            var firstStep = await _context.CaseWorkflowSteps
                .FirstOrDefaultAsync(s => s.CaseWorkflowId == workflow.CaseWorkflowId && s.StepOrder == 1);
            
            if (firstStep != null)
            {
                firstStep.Status = "In Progress";
                firstStep.StartedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await _context.Entry(workflow)
                .Collection(w => w.Steps)
                .Query()
                .OrderBy(ws => ws.StepOrder)
                .LoadAsync();

            return CreatedAtAction(nameof(GetCaseWorkflow), new { caseId = caseId }, _mapper.Map<CaseWorkflowDto>(workflow));
        }

        /// <summary>
        /// Get workflow for a case
        /// </summary>
        [HttpGet("cases/{caseId}")]
        [ProducesResponseType(typeof(CaseWorkflowDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CaseWorkflowDto>> GetCaseWorkflow(int caseId)
        {
            var workflow = await _context.CaseWorkflows
                .Include(w => w.WorkflowTemplate)
                .Include(w => w.Steps)
                    .ThenInclude(ws => ws.AssignedUser)
                .Include(w => w.Steps)
                    .ThenInclude(ws => ws.WorkflowStepTemplate)
                .FirstOrDefaultAsync(w => w.CaseId == caseId);

            if (workflow == null)
                return NotFound();

            return Ok(_mapper.Map<CaseWorkflowDto>(workflow));
        }

        /// <summary>
        /// Approve/reject workflow step
        /// </summary>
        [HttpPost("steps/{stepId}/approve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveWorkflowStep(int stepId, ApproveStepDto approveDto)
        {
            var step = await _context.CaseWorkflowSteps
                .Include(s => s.CaseWorkflow)
                .Include(s => s.AssignedUser)
                .FirstOrDefaultAsync(s => s.CaseWorkflowStepId == stepId);

            if (step == null)
                return NotFound();

            if (step.Status != "In Progress")
                return BadRequest("Step is not currently in progress");

            step.Status = approveDto.Action;
            step.Comments = approveDto.Comments;
            step.LegalOpinion = approveDto.LegalOpinion;
            step.CompletedAt = DateTime.UtcNow;

            // Move to next step if approved
            if (approveDto.Action == "Approved")
            {
                var nextStep = await _context.CaseWorkflowSteps
                    .FirstOrDefaultAsync(s => s.CaseWorkflowId == step.CaseWorkflowId && 
                                             s.StepOrder == step.StepOrder + 1);

                if (nextStep != null)
                {
                    nextStep.Status = "In Progress";
                    nextStep.StartedAt = DateTime.UtcNow;
                }
                else
                {
                    // Workflow completed
                    step.CaseWorkflow.Status = "Completed";
                    step.CaseWorkflow.CompletedAt = DateTime.UtcNow;
                }
            }
            else if (approveDto.Action == "Rejected")
            {
                step.CaseWorkflow.Status = "Rejected";
            }

            await _context.SaveChangesAsync();

            // Create notification
            await CreateWorkflowNotification(step, approveDto.Action);

            return NoContent();
        }

        /// <summary>
        /// Assign user to workflow step
        /// </summary>
        [HttpPost("steps/{stepId}/assign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignStepToUser(int stepId, AssignStepDto assignDto)
        {
            var step = await _context.CaseWorkflowSteps.FindAsync(stepId);
            if (step == null)
                return NotFound();

            step.AssignedUserId = assignDto.UserId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get pending tasks for a user
        /// </summary>
        [HttpGet("tasks/pending/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<WorkflowTaskDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkflowTaskDto>>> GetPendingTasks(int userId)
        {
            var tasks = await _context.CaseWorkflowSteps
                .Where(s => s.AssignedUserId == userId && s.Status == "In Progress")
                .Include(s => s.CaseWorkflow)
                    .ThenInclude(w => w.Case)
                .Include(s => s.AssignedUser)
                .OrderBy(s => s.DueDate)
                .ToListAsync();

            return Ok(_mapper.Map<List<WorkflowTaskDto>>(tasks));
        }

        private async Task CreateWorkflowNotification(CaseWorkflowStep step, string action)
        {
            var notification = new Notification
            {
                UserId = step.CaseWorkflow.Case.AssignedLawyerId,
                Title = $"Workflow Step {action}",
                Message = $"Step '{step.StepName}' for case {step.CaseWorkflow.Case.CaseNumber} has been {action.ToLower()}",
                Type = "ApprovalDelay",
                EntityType = "Case",
                EntityId = step.CaseWorkflow.CaseId,
                ActionUrl = $"/cases/{step.CaseWorkflow.CaseId}"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
