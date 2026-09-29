using HexArch.API.Controllers.IdentityAccess.Requests;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApproveSellerApplication;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexArch.API.Controllers.IdentityAccess
{
    /// <summary>
    /// Administrators decide seller applications here. Pending ones are listed through
    /// GET api/users?sellerApplicationStatus=Pending, since each user's latest application is part of
    /// the user view model.
    /// </summary>
    [ApiController]
    [Route("api/seller-applications")]
    [Authorize(Roles = RoleNames.Admin)]
    public class SellerApplicationsController : ControllerBase
    {
        private readonly IApproveSellerApplicationCommandHandler approveSellerApplicationCommandHandler;
        private readonly IRejectSellerApplicationCommandHandler rejectSellerApplicationCommandHandler;

        public SellerApplicationsController(
            IApproveSellerApplicationCommandHandler approveSellerApplicationCommandHandler,
            IRejectSellerApplicationCommandHandler rejectSellerApplicationCommandHandler)
        {
            this.approveSellerApplicationCommandHandler = approveSellerApplicationCommandHandler;
            this.rejectSellerApplicationCommandHandler = rejectSellerApplicationCommandHandler;
        }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
            var result = await approveSellerApplicationCommandHandler.Handle(new ApproveSellerApplicationCommand(id), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { applicationId = result.ApplicationId });
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, RejectSellerApplicationRequest request, CancellationToken cancellationToken)
        {
            var result = await rejectSellerApplicationCommandHandler.Handle(
                new RejectSellerApplicationCommand(id, request.Reason), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { applicationId = result.ApplicationId });
        }
    }
}
