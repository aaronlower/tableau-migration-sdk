﻿// Copyright (c) 2023, Salesforce, Inc.
//  SPDX-License-Identifier: Apache-2
//  
//  Licensed under the Apache License, Version 2.0 (the ""License"") 
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an ""AS IS"" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Tableau.Migration.Api.Rest.Models;
using Tableau.Migration.Api.Rest.Models.Types;
using Tableau.Migration.Content;
using Tableau.Migration.Content.Permissions;
using Tableau.Migration.Engine.Endpoints.Search;
using Tableau.Migration.Engine.Hooks.Transformers.Default;
using Tableau.Migration.Engine.Pipelines;
using Tableau.Migration.Tests.Content.Permissions;
using Xunit;

namespace Tableau.Migration.Tests.Unit.Engine.Hooks.Transformers.Default
{
    public class PermissionsTransformerTests
    {
        public abstract class PermissionsTransformerTest : AutoFixtureTestBase
        {
            internal readonly IGranteeCapabilityComparer GranteeCapabilityComparer = new(false);

            protected readonly Mock<IMigrationPipeline> MockMigrationPipeline = new();
            protected readonly Mock<IMappedContentReferenceFinder<IUser>> MockUserContentFinder = new();
            protected readonly Mock<IMappedContentReferenceFinder<IGroup>> MockGroupContentFinder = new();

            protected readonly PermissionsTransformer Transformer;

            public PermissionsTransformerTest()
            {
                MockMigrationPipeline.Setup(p => p.CreateDestinationFinder<IUser>()).Returns(MockUserContentFinder.Object);
                MockMigrationPipeline.Setup(p => p.CreateDestinationFinder<IGroup>()).Returns(MockGroupContentFinder.Object);

                Transformer = new(MockMigrationPipeline.Object);
            }
        }

        public class ExecuteAsync : PermissionsTransformerTest
        {
            protected readonly Dictionary<Guid, Guid> _idMap;

            public ExecuteAsync()
            {
                _idMap = new();

                MockUserContentFinder
                    .Setup(f => f.FindDestinationReferenceAsync(It.IsAny<Guid>(), Cancel))
                    .ReturnsAsync((Guid id, CancellationToken cancel) =>
                    {
                        if (_idMap.TryGetValue(id, out var destinationId))
                            return CreateContentReference(destinationId);
                        else
                            return null;
                    });

                MockGroupContentFinder
                    .Setup(f => f.FindDestinationReferenceAsync(It.IsAny<Guid>(), Cancel))
                    .ReturnsAsync((Guid id, CancellationToken cancel) =>
                    {
                        if (_idMap.TryGetValue(id, out var destinationId))
                            return CreateContentReference(destinationId);
                        else
                            return null;
                    });
            }

            private IContentReference CreateContentReference(Guid? id = null)
            {
                var mockReference = Create<Mock<IContentReference>>();
                mockReference.SetupGet(r => r.Id).Returns(id ?? Create<Guid>());

                return mockReference.Object;
            }

            [Fact]
            public async Task Transforms()
            {
                var originalGranteeCapabilities = CreateMany<IGranteeCapability>(5).ToImmutableArray();

                foreach (var granteeCapability in originalGranteeCapabilities)
                {
                    _idMap.Add(granteeCapability.GranteeId, Create<Guid>());
                }

                var result = await Transformer.ExecuteAsync(originalGranteeCapabilities, Cancel);

                Assert.NotNull(result);

                foreach (var granteeCapability in result)
                {
                    var idMapEntry = _idMap.Single(kvp => kvp.Value == granteeCapability.GranteeId);
                    var sourceId = idMapEntry.Key;

                    if (granteeCapability.GranteeType is GranteeType.User)
                    {
                        MockUserContentFinder
                            .Verify(f => f.FindDestinationReferenceAsync(sourceId, Cancel), Times.Once);
                    }
                    else
                    {
                        MockGroupContentFinder
                            .Verify(f => f.FindDestinationReferenceAsync(sourceId, Cancel), Times.Once);
                    }
                }
            }

            [Fact]
            public async Task Resolves_duplicates()
            {
                var granteeCapabilities = CreateMany<IGranteeCapability>(5);

                var originalGranteeCapabilities = new List<IGranteeCapability>();
                originalGranteeCapabilities.AddRange(granteeCapabilities);
                originalGranteeCapabilities.AddRange(granteeCapabilities);

                foreach (var granteeCapability in originalGranteeCapabilities)
                {
                    if (!_idMap.ContainsKey(granteeCapability.GranteeId))
                    {
                        _idMap.Add(granteeCapability.GranteeId, Create<Guid>());
                    }
                }

                var result = await Transformer.ExecuteAsync(originalGranteeCapabilities.ToImmutableArray(), Cancel);

                Assert.NotNull(result);

                foreach (var granteeCapability in result)
                {
                    var idMapEntry = _idMap.Single(kvp => kvp.Value == granteeCapability.GranteeId);
                    var sourceId = idMapEntry.Key;

                    if (granteeCapability.GranteeType is GranteeType.User)
                    {
                        MockUserContentFinder
                            .Verify(f => f.FindDestinationReferenceAsync(sourceId, Cancel), Times.Once);
                    }
                    else
                    {
                        MockGroupContentFinder
                            .Verify(f => f.FindDestinationReferenceAsync(sourceId, Cancel), Times.Once);
                    }
                }
            }

            [Fact]
            public async Task Resolves_conflicts()
            {
                var conflictingCapabilityName = Create<string>();

                var granteeCapabilities = CreateMany<IGranteeCapability>(5);

                // Add multiple conflicting grantee capabilities with the same group id
                var groupId = Create<Guid>();
                var originalGranteeCapabilities = new List<IGranteeCapability>()
                {
                    new GranteeCapability(
                        GranteeType.Group,
                        groupId,
                        new List<ICapability>() { new Capability(conflictingCapabilityName, PermissionsCapabilityModes.Allow) }),

                    new GranteeCapability(
                        GranteeType.Group,
                        groupId,
                        new List<ICapability>() { new Capability(conflictingCapabilityName, PermissionsCapabilityModes.Deny) })
                };

                originalGranteeCapabilities.AddRange(granteeCapabilities);

                foreach (var granteeCapability in originalGranteeCapabilities)
                {
                    if (!_idMap.ContainsKey(granteeCapability.GranteeId))
                    {
                        _idMap.Add(granteeCapability.GranteeId, Create<Guid>());
                    }
                }

                var result = await Transformer.ExecuteAsync(originalGranteeCapabilities.ToImmutableArray(), Cancel);

                Assert.NotNull(result);

                foreach (var granteeCapability in result)
                {
                    var idMapEntry = _idMap.Single(kvp => kvp.Value == granteeCapability.GranteeId);
                    var sourceId = idMapEntry.Key;

                    if (granteeCapability.GranteeType is GranteeType.User)
                    {
                        MockUserContentFinder
                            .Verify(f => f.FindDestinationReferenceAsync(sourceId, Cancel), Times.Once);
                    }
                    else
                    {
                        MockGroupContentFinder
                            .Verify(f => f.FindDestinationReferenceAsync(sourceId, Cancel), Times.Once);
                    }
                }

                // Check if the conflicting capability modes were resolved
                var resolvedGranteeCapability = result.Where(g => g.GranteeId == _idMap[groupId]);
                Assert.Single(resolvedGranteeCapability);

                // Check if the capability modes were resolved by preferring deny.
                var resolvedCapabilityModes = resolvedGranteeCapability
                    .SelectMany(g => g.Capabilities)
                    .Where(c => c.Name == conflictingCapabilityName)
                    .Select(c => c.Mode);

                Assert.Single(resolvedCapabilityModes);
                Assert.Equal(PermissionsCapabilityModes.Deny, resolvedCapabilityModes.FirstOrDefault());
            }

            [Fact]
            public async Task Excludes_unfound_source_grantees()
            {
                var originalGranteeCapabilities = CreateMany<IGranteeCapability>(5).ToImmutableArray();
                var foundGranteeCapabilities = originalGranteeCapabilities.SkipLast(1);

                foreach (var granteeCapability in foundGranteeCapabilities)
                {
                    _idMap.Add(granteeCapability.GranteeId, Guid.NewGuid());
                }

                var unfoundGranteeCapability = originalGranteeCapabilities[^1];

                var result = await Transformer.ExecuteAsync(originalGranteeCapabilities, Cancel);

                Assert.NotNull(result);

                Assert.DoesNotContain(unfoundGranteeCapability, result);
            }

            [Fact]
            public async Task Excludes_project_leader_deny()
            {
                var capabilities = CreateMany<ICapability>()
                    .Append(new Capability(new CapabilityType { Name = PermissionsCapabilityNames.ProjectLeader, Mode = PermissionsCapabilityModes.Allow }))
                    .Append(new Capability(new CapabilityType { Name = PermissionsCapabilityNames.ProjectLeader, Mode = PermissionsCapabilityModes.Deny }))
                    .ToImmutableArray();

                var grantee = new GranteeCapability(GranteeType.User, Guid.NewGuid(), capabilities);

                _idMap.Add(grantee.GranteeId, Guid.NewGuid());

                var result = await Transformer.ExecuteAsync(new IGranteeCapability[] { grantee }.ToImmutableList(), Cancel);

                Assert.NotNull(result);

                var resultGrantee = Assert.Single(result);

                Assert.Equal(capabilities.Length - 1, resultGrantee.Capabilities.Count);
                Assert.DoesNotContain(resultGrantee.Capabilities, c => c.Name == PermissionsCapabilityNames.ProjectLeader && c.Mode == PermissionsCapabilityModes.Deny);
            }

            [Fact]
            public async Task Excludes_inherited_project_leader()
            {
                var capabilities = CreateMany<ICapability>()
                    .Append(new Capability(new CapabilityType { Name = PermissionsCapabilityNames.InheritedProjectLeader, Mode = PermissionsCapabilityModes.Allow }))
                    .ToImmutableArray();

                var grantee = new GranteeCapability(GranteeType.User, Guid.NewGuid(), capabilities);

                _idMap.Add(grantee.GranteeId, Guid.NewGuid());

                var result = await Transformer.ExecuteAsync(new IGranteeCapability[] { grantee }.ToImmutableList(), Cancel);

                Assert.NotNull(result);

                var resultGrantee = Assert.Single(result);

                Assert.Equal(capabilities.Length - 1, resultGrantee.Capabilities.Count);
                Assert.DoesNotContain(resultGrantee.Capabilities, c => c.Name == PermissionsCapabilityNames.InheritedProjectLeader);
            }
        }
    }
}
