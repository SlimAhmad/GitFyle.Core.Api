﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using GitFyle.Core.Api.Models.Foundations.Configurations;
using GitFyle.Core.Api.Models.Foundations.Configurations.Exceptions;
using Moq;

namespace GitFyle.Core.Api.Tests.Unit.Services.Foundations.Configurations
{
    public partial class ConfigurationServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfConfigurationIsNullAndLogItAsync()
        {
            // given
            Configuration nullConfiguration = null;

            var nullConfigurationException =
                new NullConfigurationException(
                    message: "Configuration is null");

            ConfigurationValidationException expectedConfigurationValidationException =
                new ConfigurationValidationException(
                    message: "Configuration validation error occurred, fix the errors and try again.",
                    innerException: nullConfigurationException);

            // when
            ValueTask<Configuration> addConfigurationTask =
                this.configurationService.ModifyConfigurationAsync(nullConfiguration);

            ConfigurationValidationException actualConfigurationValidationException =
                await Assert.ThrowsAsync<ConfigurationValidationException>(
                    testCode: addConfigurationTask.AsTask);

            // then
            actualConfigurationValidationException.Should().BeEquivalentTo(
                expectedConfigurationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedConfigurationValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateConfigurationAsync(It.IsAny<Configuration>()),
                        Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.datetimeBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnModifyIfConfigurationIsInvalidAndLogItAsync(string invalidText)
        {
            // given
            Configuration invalidConfiguration = new Configuration
            {
                Id = Guid.Empty,
                Name = invalidText,
                Value = invalidText,
                CreatedBy = invalidText,
                CreatedDate = default,
                UpdatedBy = invalidText,
                UpdatedDate = default
            };

            var invalidConfigurationException =
                new InvalidConfigurationException(
                    message: "Configuration is invalid, fix the errors and try again.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.Id),
                values: "Id is invalid.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.Name),
                values: "Text is required.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.Value),
                values: "Text is required.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.CreatedBy),
                values: "Text is required.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.CreatedDate),
                values: "Date is invalid.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.UpdatedBy),
                values: "Text is required.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.UpdatedDate),
                values: new[]
                    {
                        "Date is invalid.",
                        $"Date is same as {nameof(Configuration.CreatedDate)}"
                    });

            ConfigurationValidationException expectedConfigurationValidationException =
                new ConfigurationValidationException(
                    message: "Configuration validation error occurred, fix the errors and try again.",
                    innerException: invalidConfigurationException);

            // when
            ValueTask<Configuration> modifyConfigurationTask =
                this.configurationService.ModifyConfigurationAsync(invalidConfiguration);

            // then
            ConfigurationValidationException actualConfigurationValidationException =
                await Assert.ThrowsAsync<ConfigurationValidationException>(
                    testCode: modifyConfigurationTask.AsTask);

            actualConfigurationValidationException.Should().BeEquivalentTo(
                expectedConfigurationValidationException);

            this.datetimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedConfigurationValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateConfigurationAsync(It.IsAny<Configuration>()),
                    Times.Never);

            this.datetimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfConfigHasInvalidLengthPropertiesAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Configuration randomConfiguration = CreateRandomModifyConfiguration(dateTimeOffset: randomDateTimeOffset);
            Configuration invalidConfiguration = randomConfiguration;
            invalidConfiguration.Name = GetRandomStringWithLengthOf(451);

            var invalidConfigurationException = new InvalidConfigurationException(
                message: "Configuration is invalid, fix the errors and try again.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.Name),
                values: $"Text exceed max length of {invalidConfiguration.Name.Length - 1} characters");

            ConfigurationValidationException expectedConfigurationValidationException =
                new ConfigurationValidationException(
                    message: "Configuration validation error occurred, fix the errors and try again.",
                    innerException: invalidConfigurationException);

            this.datetimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Configuration> modifyConfigurationTask =
                this.configurationService.ModifyConfigurationAsync(invalidConfiguration);

            ConfigurationValidationException actualConfigurationValidationException =
                await Assert.ThrowsAsync<ConfigurationValidationException>(
                    testCode: modifyConfigurationTask.AsTask);

            // then
            actualConfigurationValidationException.Should()
                .BeEquivalentTo(expectedConfigurationValidationException);

            this.datetimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConfigurationValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateConfigurationAsync(It.IsAny<Configuration>()),
                    Times.Never);

            this.datetimeBrokerMock.VerifyNoOtherCalls();
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
            Configuration randomConfiguration = CreateRandomConfiguration(randomDateTimeOffset);
            randomConfiguration.UpdatedDate = randomDateTimeOffset.AddSeconds(invalidSeconds);

            var invalidConfigurationException =
                new InvalidConfigurationException(
                message: "Configuration is invalid, fix the errors and try again.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.UpdatedDate),
                values: $"Date is not recent." +
                $" Expected a value between {startDate} and {endDate} but found {randomConfiguration.UpdatedDate}");

            var expectedConfigurationValidationException =
                new ConfigurationValidationException(
                    message: "Configuration validation error occurred, fix the errors and try again.",
                    innerException: invalidConfigurationException);

            this.datetimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Configuration> modifyConfigurationTask =
                this.configurationService.ModifyConfigurationAsync(randomConfiguration);

            ConfigurationValidationException actualConfigurationValidationException =
                await Assert.ThrowsAsync<ConfigurationValidationException>(
                    testCode: modifyConfigurationTask.AsTask);

            // then
            actualConfigurationValidationException.Should()
                .BeEquivalentTo(expectedConfigurationValidationException);

            this.datetimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedConfigurationValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateConfigurationAsync(It.IsAny<Configuration>()),
                    Times.Never);

            this.datetimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageConfigurationDoesNotExistAndLogItAsync()
        {
            // given
            int randomNegative = CreateRandomNegativeNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Configuration randomConfiguration = CreateRandomConfiguration(randomDateTimeOffset);
            Configuration nonExistingConfiguration = randomConfiguration;
            nonExistingConfiguration.CreatedDate = randomDateTimeOffset.AddMinutes(randomNegative);
            Configuration nullConfiguration = null;

            var notFoundConfigurationException =
                new NotFoundConfigurationException(
                    message: $"Configuration not found with id: {nonExistingConfiguration.Id}");

            var expectedConfigurationValidationException =
                new ConfigurationValidationException(
                    message: "Configuration validation error occurred, fix the errors and try again.",
                    innerException: notFoundConfigurationException);

            this.datetimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConfigurationByIdAsync(nonExistingConfiguration.Id))
                    .ReturnsAsync(nullConfiguration);

            // when
            ValueTask<Configuration> modifyConfigurationTask =
                this.configurationService.ModifyConfigurationAsync(nonExistingConfiguration);

            ConfigurationValidationException actualConfigurationValidationException =
                await Assert.ThrowsAsync<ConfigurationValidationException>(
                    testCode: modifyConfigurationTask.AsTask);

            // then
            actualConfigurationValidationException.Should()
                .BeEquivalentTo(expectedConfigurationValidationException);

            this.datetimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConfigurationByIdAsync(nonExistingConfiguration.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConfigurationValidationException))),
                    Times.Once);

            this.datetimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfCreatedAuditInfoHasChangedAndLogItAsync()
        {
            //given
            int randomMinutes = CreateRandomNegativeNumber();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Configuration randomConfiguration = CreateRandomModifyConfiguration(randomDateTimeOffset);
            Configuration invalidConfiguration = randomConfiguration;
            Configuration storedConfiguration = randomConfiguration.DeepClone();
            storedConfiguration.CreatedBy = GetRandomString();
            storedConfiguration.CreatedDate = storedConfiguration.CreatedDate.AddMinutes(randomMinutes);
            storedConfiguration.UpdatedDate = storedConfiguration.UpdatedDate.AddMinutes(randomMinutes);
            Guid ConfigurationId = invalidConfiguration.Id;

            var invalidConfigurationException = new InvalidConfigurationException(
                message: "Configuration is invalid, fix the errors and try again.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.CreatedBy),
                values: $"Text is not same as {nameof(Configuration.CreatedBy)}");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.CreatedDate),
                values: $"Date is not same as {nameof(Configuration.CreatedDate)}");

            var expectedConfigurationValidationException = new ConfigurationValidationException(
                message: "Configuration validation error occurred, fix the errors and try again.",
                innerException: invalidConfigurationException);

            this.datetimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConfigurationByIdAsync(ConfigurationId))
                    .ReturnsAsync(storedConfiguration);

            // when
            ValueTask<Configuration> modifyConfigurationTask =
                this.configurationService.ModifyConfigurationAsync(invalidConfiguration);

            ConfigurationValidationException actualConfigurationValidationException =
                await Assert.ThrowsAsync<ConfigurationValidationException>(
                    testCode: modifyConfigurationTask.AsTask);

            // then
            actualConfigurationValidationException.Should().BeEquivalentTo(
                expectedConfigurationValidationException);

            this.datetimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConfigurationByIdAsync(invalidConfiguration.Id),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expectedConfigurationValidationException))),
                        Times.Once);

            this.datetimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnModifyIfStorageUpdatedDateSameAsUpdatedDateAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Configuration randomConfiguration = CreateRandomModifyConfiguration(randomDateTimeOffset);
            Configuration invalidConfiguration = randomConfiguration;

            Configuration storageConfiguration = randomConfiguration.DeepClone();
            invalidConfiguration.UpdatedDate = storageConfiguration.UpdatedDate;

            var invalidConfigurationException = new InvalidConfigurationException(
                message: "Configuration is invalid, fix the errors and try again.");

            invalidConfigurationException.AddData(
                key: nameof(Configuration.UpdatedDate),
                values: $"Date is same as {nameof(Configuration.UpdatedDate)}");

            var expectedConfigurationValidationException =
                new ConfigurationValidationException(
                    message: "Configuration validation error occurred, fix the errors and try again.",
                    innerException: invalidConfigurationException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectConfigurationByIdAsync(invalidConfiguration.Id))
                .ReturnsAsync(storageConfiguration);

            this.datetimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Configuration> modifyConfigurationTask =
                this.configurationService.ModifyConfigurationAsync(invalidConfiguration);

            ConfigurationValidationException actualConfigurationValidationException =
               await Assert.ThrowsAsync<ConfigurationValidationException>(
                   testCode: modifyConfigurationTask.AsTask);

            // then
            actualConfigurationValidationException.Should().BeEquivalentTo(
                expectedConfigurationValidationException);

            this.datetimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConfigurationValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectConfigurationByIdAsync(invalidConfiguration.Id),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.datetimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
