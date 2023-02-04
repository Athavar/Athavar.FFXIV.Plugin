// <copyright file="JsonParseException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Exceptions;

public class JsonParseException : AthavarPluginException
{
    public JsonParseException(string message)
        : base(message)
    {
    }
}