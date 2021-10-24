// <copyright file="YesService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    internal static class YesService
    {
        /// <summary>
        /// Gets or sets the yes module itself.
        /// </summary>
        internal static YesModule? Module { get; set; } = null!;

        /// <summary>
        /// Gets or sets the yes module address resolver.
        /// </summary>
        internal static YesAddressResolver Address { get; set; } = null!;

        /// <summary>
        /// Gets or sets the yes module configuration.
        /// </summary>
        internal static YesConfiguration Configuration { get; set; } = null!;
    }
}
