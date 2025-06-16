using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Policies
{
    public class EmailVerifiedRequirement : ClaimsAuthorizationRequirement
    {
        private string ClaimType { get; set; } = default!;
        private IEnumerable<string>? AllowedValues { get; set; } = default!;

        public EmailVerifiedRequirement(string claimType, IEnumerable<string>? allowedValues) : base(claimType, allowedValues)
        {
            this.ClaimType = claimType;
            this.AllowedValues = allowedValues;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClaimsAuthorizationRequirement requirement)
        {
            if(context.User.HasClaim(c => c.Type.Equals("EmailIsVerified")))
            {
                Claim EmailIsVerified = context.User.Claims.Where(c => c.Type.Equals("EmailIsVerified")).FirstOrDefault();

                if (EmailIsVerified != null && AllowedValues.Where(v => v.Equals(EmailIsVerified.Value)).Count() > 0)
                {
                    // EmailIsVerified claim exists and is true (the only allowed value)

                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            context.Fail();
            return Task.CompletedTask;
        }
    }
}
