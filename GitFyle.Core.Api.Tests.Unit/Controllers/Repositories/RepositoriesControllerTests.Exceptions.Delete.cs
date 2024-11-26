﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using GitFyle.Core.Api.Models.Foundations.Repositories;
using GitFyle.Core.Api.Models.Foundations.Repositories.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESTFulSense.Clients.Extensions;
using RESTFulSense.Models;
using Xeptions;

namespace GitFyle.Core.Api.Tests.Unit.Controllers.Repositories
{
    public partial class RepositoriesControllerTests
    {
        [Fact]
        public async Task ShouldReturnNotFoundOnDeleteIfItemDoesNotExistAsync()
        {
            // given
            Guid someId = Guid.NewGuid();
            string someMessage = GetRandomString();

            var notFoundRepositoryException =
                new NotFoundRepositoryException(
                    message: someMessage);

            var repositoryValidationException =
                new RepositoryValidationException(
                    message: someMessage,
                    innerException: notFoundRepositoryException);

            NotFoundObjectResult expectedNotFoundObjectResult =
                NotFound(notFoundRepositoryException);

            var expectedActionResult =
                new ActionResult<Repository>(expectedNotFoundObjectResult);

            this.repositoryServiceMock.Setup(service =>
                service.RemoveRepositoryByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(repositoryValidationException);

            // when
            ActionResult<Repository> actualActionResult =
                await this.repositoriesController.DeleteRepositoryByIdAsync(someId);

            // then
            actualActionResult.ShouldBeEquivalentTo(expectedActionResult);

            this.repositoryServiceMock.Verify(service =>
                service.RemoveRepositoryByIdAsync(It.IsAny<Guid>()),
                    Times.Once);

            this.repositoryServiceMock.VerifyNoOtherCalls();
        }
    }
}