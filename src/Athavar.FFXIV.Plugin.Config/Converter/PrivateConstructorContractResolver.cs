// <copyright file="PrivateConstructorContractResolver.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

internal sealed class PrivateConstructorContractResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object && jsonTypeInfo.CreateObject is null)
        {
            if (jsonTypeInfo.Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length == 0)
            {
                // The type doesn't have public constructors
                jsonTypeInfo.CreateObject = () => Activator.CreateInstance(jsonTypeInfo.Type, true);
            }
        }

        return jsonTypeInfo;
    }
}