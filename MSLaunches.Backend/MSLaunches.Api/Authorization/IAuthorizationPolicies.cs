﻿using Microsoft.AspNetCore.Authorization;
using System;

namespace MSLaunches.Api.Authorization
{
    /// <summary>
    /// Set of authorization policies defined for WebApiCoreMSLaunches
    /// </summary>
    public interface IAuthorizationPolicies
    {
        /// <summary> Authorization policy for actions available only to admins </summary>
        Action<AuthorizationPolicyBuilder> AdminOnly { get; }
    }
}
