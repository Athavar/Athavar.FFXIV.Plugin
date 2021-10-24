// <copyright file="HuntLinkModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.HuntLink
{
    using System;

    /// <summary>
    /// Implements the hunt link mmodule.
    /// </summary>
    internal class HuntLinkModule : IModule
    {
        private readonly HuntLinkTab tab;

        /// <summary>
        /// Initializes a new instance of the <see cref="HuntLinkModule"/> class.
        /// </summary>
        /// <param name="modules">The other <see cref="Modules"/>.</param>
        public HuntLinkModule(Modules modules)
        {
            this.tab = new();
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            this.tab.DrawTab();
        }
    }
}
