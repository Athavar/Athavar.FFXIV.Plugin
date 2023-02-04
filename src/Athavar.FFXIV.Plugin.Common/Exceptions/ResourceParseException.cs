// <copyright file="ResourceParseException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Exceptions;

public class ResourceParseException : AthavarPluginException
{
    public ResourceParseException(string message)
        : base(message)
    {
    }
}