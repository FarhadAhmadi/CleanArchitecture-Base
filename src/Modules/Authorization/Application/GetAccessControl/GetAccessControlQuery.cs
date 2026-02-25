using Application.Abstractions.Messaging;

namespace Application.Authorization.GetAccessControl;

public sealed record GetAccessControlQuery : IQuery<AccessControlResponse>;
