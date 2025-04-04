﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using GitFyle.Core.Api.Brokers.DateTimes;
using GitFyle.Core.Api.Brokers.Loggings;
using GitFyle.Core.Api.Brokers.Storages;
using GitFyle.Core.Api.Models.Foundations.Contributors;

namespace GitFyle.Core.Api.Services.Foundations.Contributors
{
    internal partial class ContributorService : IContributorService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public ContributorService(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Contributor> AddContributorAsync(Contributor contributor) =>
        TryCatch(async () =>
        {
            await ValidateContributorOnAddAsync(contributor);

            return await this.storageBroker.InsertContributorAsync(contributor);
        });

        public ValueTask<IQueryable<Contributor>> RetrieveAllContributorsAsync() =>
        TryCatch(async () => await this.storageBroker.SelectAllContributorsAsync());

        public ValueTask<Contributor> RetrieveContributorByIdAsync(Guid contributorId) =>
        TryCatch(async () =>
        {
            ValidateContributorIdAsync(contributorId);

            Contributor maybeContributor =
                await this.storageBroker.SelectContributorByIdAsync(contributorId);

            ValidateStorageContributorAsync(maybeContributor, contributorId);

            return maybeContributor;
        });

        public ValueTask<Contributor> ModifyContributorAsync(Contributor contributor) =>
        TryCatch(async () =>
        {
            await ValidateContributorOnModifyAsync(contributor);

            Contributor maybeContributor =
                await this.storageBroker.SelectContributorByIdAsync(contributor.Id);

            ValidateStorageContributor(maybeContributor, contributor.Id);
            ValidateAgainstStorageContributorOnModify(contributor, maybeContributor);

            return await this.storageBroker.UpdateContributorAsync(contributor);
        });

        public ValueTask<Contributor> RemoveContributorByIdAsync(Guid contributorId) =>
        TryCatch(async () =>
        {
            ValidateContributorIdAsync(contributorId);
            Contributor maybeContributor =
                await this.storageBroker.SelectContributorByIdAsync(contributorId);

            ValidateStorageContributorAsync(maybeContributor, contributorId);

            return await this.storageBroker.DeleteContributorAsync(maybeContributor);
        });
    }
}