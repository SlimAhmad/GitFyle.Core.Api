// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using GitFyle.Core.Api.Models.Foundations.Contributors;
using Microsoft.EntityFrameworkCore;

namespace GitFyle.Core.Api.Brokers.Storages
{
    internal partial class StorageBroker
    {
        public DbSet<Contributor> Contributors { get; set; }

        public async ValueTask<Contributor> InsertContributorAsync(Contributor contributor) =>
            await InsertAsync(contributor);

        public async ValueTask<IQueryable<Contributor>> SelectAllContributorsAsync() =>
            await SelectAllAsync<Contributor>();

        public async ValueTask<Contributor> SelectContributorByIdAsync(Guid contributorId) =>
            await SelectAsync<Contributor>(contributorId);

        public async ValueTask<Contributor> UpdateContributorAsync(Contributor contributor) =>
            await UpdateAsync(contributor);

        public async ValueTask<Contributor> DeleteContributorAsync(Contributor contributor) =>
            await DeleteAsync(contributor);
    }
}
