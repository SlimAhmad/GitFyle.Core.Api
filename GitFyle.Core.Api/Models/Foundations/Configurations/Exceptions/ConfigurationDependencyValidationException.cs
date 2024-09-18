﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using Xeptions;

namespace GitFyle.Core.Api.Models.Foundations.Configurations.Exceptions
{
    public class ConfigurationDependencyValidationException : Xeption
    {
        public ConfigurationDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}