using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rookie.AMO.Identity.Business.Interfaces
{
    public interface IWebAPIProvider
    {
        [Get("/Assignment/CheckRelation/{userId}")]
        Task<bool> CheckRelationToAssignment([Authorize("Bearer")] string token, Guid userId);
    }
}
