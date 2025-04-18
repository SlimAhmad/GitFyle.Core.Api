﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using GitFyle.Core.Api.Models.Foundations.ContributionTypes;
using GitFyle.Core.Api.Models.Foundations.ContributionTypes.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace GitFyle.Core.Api.Tests.Unit.Controllers.ContributionTypes
{
    public partial class ContributionTypesControllerTests
    {
        [Theory]
        [MemberData(nameof(ValidationExceptions))]
        public async Task ShouldReturnBadRequestOnPutIfValidationErrorOccursAsync(
                Xeption validationException)
        {
            // given
            ContributionType someContributionType = CreateRandomContributionType();

            BadRequestObjectResult expectedBadRequestObjectResult =
                BadRequest(validationException.InnerException);

            var expectedActionResult =
                new ActionResult<ContributionType>(expectedBadRequestObjectResult);

            this.contributionTypeServiceMock.Setup(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<ContributionType> actualActionResult =
                await this.contributionTypesController.PutContributionTypeAsync(someContributionType);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.contributionTypeServiceMock.Verify(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()),
                    Times.Once);

            this.contributionTypeServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public async Task ShouldReturnInternalServerErrorOnPutIfServerErrorOccurredAsync(
                Xeption validationException)
        {
            // given
            ContributionType someContributionType = CreateRandomContributionType();

            InternalServerErrorObjectResult expectedBadRequestObjectResult =
                InternalServerError(validationException);

            var expectedActionResult =
                new ActionResult<ContributionType>(expectedBadRequestObjectResult);

            this.contributionTypeServiceMock.Setup(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()))
                    .ThrowsAsync(validationException);

            // when
            ActionResult<ContributionType> actualActionResult =
                await this.contributionTypesController.PutContributionTypeAsync(someContributionType);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.contributionTypeServiceMock.Verify(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()),
                    Times.Once);

            this.contributionTypeServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNotFoundOnPutIfItemDoesNotExistAsync()
        {
            // given
            ContributionType someContributionType = CreateRandomContributionType();
            string someMessage = GetRandomString();

            var notFoundContributionTypeException =
                new NotFoundContributionTypeException(
                    message: someMessage);

            var contributionTypeValidationException =
                new ContributionTypeValidationException(
                    message: someMessage,
                    innerException: notFoundContributionTypeException);

            NotFoundObjectResult expectedNotFoundObjectResult =
                NotFound(notFoundContributionTypeException);

            var expectedActionResult =
                new ActionResult<ContributionType>(expectedNotFoundObjectResult);

            this.contributionTypeServiceMock.Setup(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()))
                    .ThrowsAsync(contributionTypeValidationException);

            // when
            ActionResult<ContributionType> actualActionResult =
                await this.contributionTypesController.PutContributionTypeAsync(someContributionType);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.contributionTypeServiceMock.Verify(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()),
                    Times.Once);

            this.contributionTypeServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnConflictOnPutIfAlreadyExistsContributionTypeErrorOccursAsync()
        {
            // given
            ContributionType someContributionType = CreateRandomContributionType();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();
            var someDictionaryData = GetRandomDictionaryData();

            var alreadyExistsContributionTypeException =
                new AlreadyExistsContributionTypeException(
                    message: someMessage,
                    innerException: someInnerException,
                    data: someInnerException.Data);

            var contributionTypeDependencyValidationException =
                new ContributionTypeDependencyValidationException(
                    message: someMessage,
                    innerException: alreadyExistsContributionTypeException,
                    data: someDictionaryData);

            ConflictObjectResult expectedConflictObjectResult =
                Conflict(alreadyExistsContributionTypeException);

            var expectedActionResult =
                new ActionResult<ContributionType>(expectedConflictObjectResult);

            this.contributionTypeServiceMock.Setup(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()))
                    .ThrowsAsync(contributionTypeDependencyValidationException);

            // when
            ActionResult<ContributionType> actualActionResult =
                await this.contributionTypesController.PutContributionTypeAsync(someContributionType);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.contributionTypeServiceMock.Verify(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()),
                    Times.Once);

            this.contributionTypeServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnFailedDependencyOnPutIfReferenceErrorOccursAsync()
        {
            // given
            ContributionType someContributionType = CreateRandomContributionType();
            var someInnerException = new Exception();
            string someMessage = GetRandomString();
            var someDictionaryData = GetRandomDictionaryData();

            var invalidReferenceContributionTypeException =
                new InvalidReferenceContributionTypeException(
                    message: someMessage,
                    innerException: someInnerException,
                    data: someInnerException.Data);

            var contributionTypeDependencyValidationException =
                new ContributionTypeDependencyValidationException(
                    message: someMessage,
                    innerException: invalidReferenceContributionTypeException,
                    data: someDictionaryData);

            FailedDependencyObjectResult expectedConflictObjectResult = 
                    FailedDependency(invalidReferenceContributionTypeException);

            var expectedActionResult = 
                    new ActionResult<ContributionType>(expectedConflictObjectResult);

            this.contributionTypeServiceMock.Setup(service => 
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()))
                    .ThrowsAsync(contributionTypeDependencyValidationException);

            // when
            ActionResult<ContributionType> actualActionResult = 
                    await this.contributionTypesController.PutContributionTypeAsync(someContributionType);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.contributionTypeServiceMock.Verify(service =>
                service.ModifyContributionTypeAsync(It.IsAny<ContributionType>()),
                    Times.Once);

            this.contributionTypeServiceMock.VerifyNoOtherCalls();
        }
    }
}
