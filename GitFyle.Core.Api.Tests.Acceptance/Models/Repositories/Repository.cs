﻿// ----------------------------------------------------------------------------------
// Copyright (c) The Standard Organization: A coalition of the Good-Hearted Engineers
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using GitFyle.Core.Api.Models.Foundations.Contributions;
using GitFyle.Core.Api.Tests.Acceptance.Models.Sources;

namespace GitFyle.Core.Api.Tests.Acceptance.Models.Repositories
{
    public class Repository
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string ExternalId { get; set; }
        public Guid SourceId { get; set; }
        public bool IsOrganization { get; set; }
        public bool IsPrivate { get; set; }
        public string Token { get; set; }
        public DateTimeOffset TokenExpireAt { get; set; }
        public string Description { get; set; }
        public DateTimeOffset ExternalCreatedAt { get; set; }
        public DateTimeOffset ExternalUpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public Source Source { get; set; }
        public IEnumerable<Contribution> Contributions { get; set; }
    }
}
