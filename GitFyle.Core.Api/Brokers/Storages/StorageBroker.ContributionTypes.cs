// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using GitFyle.Core.Api.Models.Foundations.ContributionTypes;
using Microsoft.EntityFrameworkCore;

namespace GitFyle.Core.Api.Brokers.Storages
{
    internal partial class StorageBroker
    {
        public DbSet<ContributionType> ContributionTypes { get; set; }

        public async ValueTask<ContributionType> InsertContributionTypeAsync(ContributionType contributionType) =>
            await InsertAsync(contributionType);

        public async ValueTask<IQueryable<ContributionType>> SelectAllContributionTypesAsync() =>
            await SelectAllAsync<ContributionType>();

        public async ValueTask<ContributionType> SelectContributionTypeByIdAsync(Guid contributionTypeId) =>
            await SelectAsync<ContributionType>(contributionTypeId);

        public async ValueTask<ContributionType> UpdateContributionTypeAsync(ContributionType contributionType) =>
            await UpdateAsync(contributionType);

        public async ValueTask<ContributionType> DeleteContributionTypeAsync(ContributionType contributionType) =>
            await DeleteAsync(contributionType);
    }
}
