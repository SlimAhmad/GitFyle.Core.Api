﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using GitFyle.Core.Api.Models.Foundations.ContributionTypes;
using GitFyle.Core.Api.Models.Foundations.ContributionTypes.Exceptions;
using Moq;

namespace GitFyle.Core.Api.Tests.Unit.Services.Foundations.ContributionTypes
{
    public partial class ContributionTypeServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfContributionTypeIsNullAndLogItAsync()
        {
            // given
            ContributionType nullContributionType = null;

            var nullContributionTypeException =
                new NullContributionTypeException(message: "ContributionType is null");

            var expectedContributionTypeValidationException =
                new ContributionTypeValidationException(
                    message: "ContributionType validation error occurred, fix errors and try again.",
                    innerException: nullContributionTypeException);

            // when
            ValueTask<ContributionType> addContributionTypeTask =
                this.contributionTypeService.ModifyContributionTypeAsync(nullContributionType);

            ContributionTypeValidationException actualContributionTypeValidationException =
                await Assert.ThrowsAsync<ContributionTypeValidationException>(
                    testCode: addContributionTypeTask.AsTask);

            // then
            actualContributionTypeValidationException.Should().BeEquivalentTo(
                expectedContributionTypeValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedContributionTypeValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertContributionTypeAsync(It.IsAny<ContributionType>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfContributionTypeIsInvalidAndLogItAsync(
        string invalidString)
        {
            // given
            var invalidContributionType = new ContributionType
            {
                Id = Guid.Empty,
                Name = invalidString,
                CreatedBy = invalidString,
                CreatedDate = default,
                UpdatedBy = invalidString,
                UpdatedDate = default,
            };

            var invalidContributionTypeException = new InvalidContributionTypeException(
                message: "ContributionType is invalid, fix the errors and try again.");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.Id),
                values: "Id is invalid");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.Name),
                values: "Text is required");

            invalidContributionTypeException.AddData(
               key: nameof(ContributionType.CreatedBy),
               values: "Text is required");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.UpdatedBy),
                values: "Text is required");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.CreatedDate),
                values: "Date is invalid");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.UpdatedDate),
                values:
                    new[]
                    {
                      "Date is invalid",
                      $"Date is the same as {nameof(ContributionType.CreatedDate)}"
                    });

            var expectedContributionTypeValidationException =
                new ContributionTypeValidationException(
                    message: "ContributionType validation error occurred, fix errors and try again.",
                    innerException: invalidContributionTypeException);

            // when
            ValueTask<ContributionType> modifyContributionTypeTask =
                this.contributionTypeService.ModifyContributionTypeAsync(invalidContributionType);

            ContributionTypeValidationException actualContributionTypeValidationException =
                await Assert.ThrowsAsync<ContributionTypeValidationException>(
                    testCode: modifyContributionTypeTask.AsTask);

            // then
            actualContributionTypeValidationException.Should().BeEquivalentTo(
                expectedContributionTypeValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedContributionTypeValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertContributionTypeAsync(It.IsAny<ContributionType>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfContributionTypeHasInvalidLengthPropertiesAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            var invalidContributionType = CreateRandomContributionType(dateTimeOffset: randomDateTimeOffset);
            invalidContributionType.Name = GetRandomStringWithLengthOf(256);

            var invalidContributionTypeException =
                new InvalidContributionTypeException(
                    message: "ContributionType is invalid, fix the errors and try again.");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.Name),
                values: $"Text exceeds max length of {invalidContributionType.Name.Length - 1} characters");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.UpdatedDate),
                values: $"Date is the same as {nameof(ContributionType.CreatedDate)}");

            var expectedContributionTypeValidationException =
                new ContributionTypeValidationException(
                    message: "ContributionType validation error occurred, fix errors and try again.",
                    innerException: invalidContributionTypeException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<ContributionType> modifyContributionTypeTask =
                this.contributionTypeService.ModifyContributionTypeAsync(invalidContributionType);

            ContributionTypeValidationException actualContributionTypeValidationException =
                await Assert.ThrowsAsync<ContributionTypeValidationException>(
                    testCode: modifyContributionTypeTask.AsTask);

            // then
            actualContributionTypeValidationException.Should()
                .BeEquivalentTo(expectedContributionTypeValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedContributionTypeValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertContributionTypeAsync(It.IsAny<ContributionType>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsSameAsCreatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            ContributionType randomContributionType = CreateRandomContributionType(randomDateTimeOffset);
            ContributionType invalidContributionType = randomContributionType;

            var invalidContributionTypeException = new InvalidContributionTypeException(
                message: "ContributionType is invalid, fix the errors and try again.");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.UpdatedDate),
                values: $"Date is the same as {nameof(ContributionType.CreatedDate)}");

            var expectedContributionTypeValidationException = new ContributionTypeValidationException(
                message: "ContributionType validation error occurred, fix errors and try again.",
                innerException: invalidContributionTypeException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<ContributionType> modifyContributionTypeTask =
                this.contributionTypeService.ModifyContributionTypeAsync(invalidContributionType);

            ContributionTypeValidationException actualContributionTypeValidationException =
                await Assert.ThrowsAsync<ContributionTypeValidationException>(
                    testCode: modifyContributionTypeTask.AsTask);

            // then
            actualContributionTypeValidationException.Should().BeEquivalentTo(
                expectedContributionTypeValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedContributionTypeValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectContributionTypeByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-61)]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedDateIsNotRecentAndLogItAsync(
             int invalidSeconds)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            DateTimeOffset now = randomDateTimeOffset;
            DateTimeOffset startDate = now.AddSeconds(-60);
            DateTimeOffset endDate = now.AddSeconds(0);
            ContributionType randomContributionType = CreateRandomContributionType(randomDateTimeOffset);
            randomContributionType.UpdatedDate = randomDateTimeOffset.AddSeconds(invalidSeconds);

            var invalidContributionTypeException = new InvalidContributionTypeException(
                message: "ContributionType is invalid, fix the errors and try again.");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.UpdatedDate),
                values:
                [
                    $"Date is not recent." +
                  $" Expected a value between {startDate} and {endDate} but found {randomContributionType.UpdatedDate}"
                ]);

            var expectedContributionTypeValidationException = new ContributionTypeValidationException(
                message: "ContributionType validation error occurred, fix errors and try again.",
                innerException: invalidContributionTypeException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<ContributionType> modifyContributionTypeTask =
                this.contributionTypeService.ModifyContributionTypeAsync(randomContributionType);

            ContributionTypeValidationException actualContributionTypeValidationException =
                await Assert.ThrowsAsync<ContributionTypeValidationException>(
                    testCode: modifyContributionTypeTask.AsTask);

            // then
            actualContributionTypeValidationException.Should().BeEquivalentTo(
                expectedContributionTypeValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedContributionTypeValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectContributionTypeByIdAsync(It.IsAny<Guid>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageContributionTypeDoesNotExistAndLogItAsync()
        {
            // given
            int randomNegative = GetRandomNegativeNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            ContributionType randomContributionType = CreateRandomContributionType(randomDateTimeOffset);
            ContributionType nonExistingContributionType = randomContributionType;
            nonExistingContributionType.CreatedDate = randomDateTimeOffset.AddMinutes(randomNegative);
            ContributionType nullContributionType = null;

            var notFoundContributionTypeException =
                new NotFoundContributionTypeException(
                    message: $"ContributionType not found with id: {nonExistingContributionType.Id}");

            var expectedContributionTypeValidationException =
                new ContributionTypeValidationException(
                    message: "ContributionType validation error occurred, fix errors and try again.",
                    innerException: notFoundContributionTypeException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectContributionTypeByIdAsync(nonExistingContributionType.Id))
                    .ReturnsAsync(nullContributionType);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<ContributionType> modifyContributionTypeTask =
                this.contributionTypeService.ModifyContributionTypeAsync(nonExistingContributionType);

            ContributionTypeValidationException actualContributionTypeValidationException =
                await Assert.ThrowsAsync<ContributionTypeValidationException>(
                    testCode: modifyContributionTypeTask.AsTask);

            // then
            actualContributionTypeValidationException.Should().BeEquivalentTo(
                expectedContributionTypeValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectContributionTypeByIdAsync(nonExistingContributionType.Id),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedContributionTypeValidationException))),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedAuditInfoHasChangedAndLogItAsync()
        {
            // given
            int randomMinutes = GetRandomNegativeNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            ContributionType randomContributionType = CreateRandomModifyContributionType(randomDateTimeOffset);
            ContributionType invalidContributionType = randomContributionType;
            ContributionType storedContributionType = randomContributionType.DeepClone();
            storedContributionType.CreatedBy = GetRandomString();
            storedContributionType.CreatedDate = storedContributionType.CreatedDate.AddMinutes(randomMinutes);
            storedContributionType.UpdatedDate = storedContributionType.UpdatedDate.AddMinutes(randomMinutes);
            Guid ContributionTypeId = invalidContributionType.Id;

            var invalidContributionTypeException = new InvalidContributionTypeException(
                message: "ContributionType is invalid, fix the errors and try again.");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.CreatedBy),
                values: $"Text is not the same as {nameof(ContributionType.CreatedBy)}");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.CreatedDate),
                values: $"Date is not the same as {nameof(ContributionType.CreatedDate)}");

            var expectedContributionTypeValidationException = new ContributionTypeValidationException(
                message: "ContributionType validation error occurred, fix errors and try again.",
                innerException: invalidContributionTypeException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectContributionTypeByIdAsync(ContributionTypeId))
                    .ReturnsAsync(storedContributionType);

            // when
            ValueTask<ContributionType> modifyContributionTypeTask =
                this.contributionTypeService.ModifyContributionTypeAsync(invalidContributionType);

            ContributionTypeValidationException actualContributionTypeValidationException =
                await Assert.ThrowsAsync<ContributionTypeValidationException>(
                    testCode: modifyContributionTypeTask.AsTask);

            // then
            actualContributionTypeValidationException.Should().BeEquivalentTo(
                expectedContributionTypeValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectContributionTypeByIdAsync(invalidContributionType.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedContributionTypeValidationException))),
                        Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageUpdatedDateSameAsUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            ContributionType randomContributionType = CreateRandomModifyContributionType(randomDateTimeOffset);
            ContributionType invalidContributionType = randomContributionType;

            ContributionType storageContributionType = randomContributionType.DeepClone();
            invalidContributionType.UpdatedDate = storageContributionType.UpdatedDate;

            var invalidContributionTypeException = new InvalidContributionTypeException(
                message: "ContributionType is invalid, fix the errors and try again.");

            invalidContributionTypeException.AddData(
                key: nameof(ContributionType.UpdatedDate),
                values: $"Date is the same as {nameof(ContributionType.UpdatedDate)}");

            var expectedContributionTypeValidationException =
                new ContributionTypeValidationException(
                    message: "ContributionType validation error occurred, fix errors and try again.",
                    innerException: invalidContributionTypeException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectContributionTypeByIdAsync(invalidContributionType.Id))
                .ReturnsAsync(storageContributionType);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<ContributionType> modifyContributionTypeTask =
                this.contributionTypeService.ModifyContributionTypeAsync(invalidContributionType);

            ContributionTypeValidationException actualContributionTypeValidationException =
               await Assert.ThrowsAsync<ContributionTypeValidationException>(
                   testCode: modifyContributionTypeTask.AsTask);

            // then
            actualContributionTypeValidationException.Should().BeEquivalentTo(
                expectedContributionTypeValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedContributionTypeValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectContributionTypeByIdAsync(invalidContributionType.Id),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}