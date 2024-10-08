﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using GitFyle.Core.Api.Models.Foundations.Contributions;
using GitFyle.Core.Api.Models.Foundations.Contributions.Exceptions;
using Moq;

namespace GitFyle.Core.Api.Tests.Unit.Services.Foundations.Contributions
{
    public partial class ContributionServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRemoveByIdIfIdIsInvalidAndLogitAsync()
        {
            // given
            Guid someContributionId = Guid.Empty;

            var invalidContributionException =
                new InvalidContributionException(
                    message: "Contribution is invalid, fix the errors and try again.");

            invalidContributionException.AddData(
                key: nameof(Contribution.Id),
                values: "Id is invalid");

            var expectedContributionValidationException =
                new ContributionValidationException(
                    message: "Contribution validation error occurred, fix errors and try again.",
                    innerException: invalidContributionException);

            // when
            ValueTask<Contribution> removeContributionByIdTask =
                this.contributionService.RemoveContributionByIdAsync(someContributionId);

            ContributionValidationException actualContributionValidationException =
                await Assert.ThrowsAsync<ContributionValidationException>(
                    removeContributionByIdTask.AsTask);

            // then
            actualContributionValidationException.Should().BeEquivalentTo(
                expectedContributionValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedContributionValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertContributionAsync(It.IsAny<Contribution>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnRemoveByIdIfNotFoundAndLogItAsync()
        {
            //given
            Guid someContributionId = Guid.NewGuid();
            Contribution noContribution = null;

            var notFoundContributionException =
                new NotFoundContributionException(
                    message: $"Contribution not found with id: {someContributionId}");

            var expectedContributionValidationException =
                new ContributionValidationException(
                    message: "Contribution validation error occurred, fix errors and try again.",
                    innerException: notFoundContributionException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectContributionByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noContribution);

            //when
            ValueTask<Contribution> removeContributionByIdTask =
                this.contributionService.RemoveContributionByIdAsync(someContributionId);

            ContributionValidationException actualContributionValidationException =
                await Assert.ThrowsAsync<ContributionValidationException>(
                    removeContributionByIdTask.AsTask);

            //then
            actualContributionValidationException.Should().BeEquivalentTo(
                expectedContributionValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectContributionByIdAsync(someContributionId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedContributionValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteContributionAsync(It.IsAny<Contribution>()),
                    Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}