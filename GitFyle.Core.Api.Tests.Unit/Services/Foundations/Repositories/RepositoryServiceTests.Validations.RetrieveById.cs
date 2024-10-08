﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using GitFyle.Core.Api.Models.Foundations.Repositories;
using GitFyle.Core.Api.Models.Foundations.Repositories.Exceptions;
using Moq;

namespace GitFyle.Core.Api.Tests.Unit.Services.Foundations.Repositories
{
    public partial class RepositoryServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdWhenRepositoryIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidRepositoryId = Guid.Empty;

            var invalidRepositoryException =
                new InvalidRepositoryException(
                    message: "Repository is invalid, fix the errors and try again.");

            invalidRepositoryException.AddData(
                key: nameof(Repository.Id),
                values: "Id is invalid");

            var expectedRepositoryValidationException =
                new RepositoryValidationException(
                    message: "Repository validation error occurred, fix errors and try again.",
                    innerException: invalidRepositoryException);

            // when
            ValueTask<Repository> retrieveRepositoryByIdTask =
                this.repositoryService.RetrieveRepositoryByIdAsync(invalidRepositoryId);

            RepositoryValidationException actualRepositoryValidationException =
                await Assert.ThrowsAsync<RepositoryValidationException>(
                    testCode: retrieveRepositoryByIdTask.AsTask);

            // then
            actualRepositoryValidationException.Should().BeEquivalentTo(
                expectedRepositoryValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedRepositoryValidationException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectRepositoryByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfRepositoryIdNotFoundAndLogitAsync()
        {
            //given
            var someRepositoryId = Guid.NewGuid();
            Repository nullRepository = null;
            var innerException = new Exception();

            var notFoundRepositoryException =
                new NotFoundRepositoryException(
                    message: $"Repository not found with id: {someRepositoryId}");

            var expectedRepositoryValidationException =
                new RepositoryValidationException(
                    message: "Repository validation error occurred, fix errors and try again.",
                    innerException: notFoundRepositoryException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectRepositoryByIdAsync(someRepositoryId))
                    .ReturnsAsync(nullRepository);

            // when
            ValueTask<Repository> retrieveRepositoryByIdTask =
                this.repositoryService.RetrieveRepositoryByIdAsync(someRepositoryId);

            RepositoryValidationException actualRepositoryValidationException =
                await Assert.ThrowsAsync<RepositoryValidationException>(
                    testCode: retrieveRepositoryByIdTask.AsTask);

            // then
            actualRepositoryValidationException.Should().BeEquivalentTo(
                expectedRepositoryValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectRepositoryByIdAsync(someRepositoryId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedRepositoryValidationException))),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}