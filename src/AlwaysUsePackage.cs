// <copyright file="AlwaysUsePackage.cs" company="Matt Lacey Limited">
// Copyright (c) Matt Lacey Limited. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace AlwaysUse
{
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(AlwaysUsePackage.PackageGuidString)]
    [InstalledProductRegistration("#110", "#112", "1.2", IconResourceID = 400)] // Info on this package for Help/About
    public sealed class AlwaysUsePackage : AsyncPackage
    {
        /// <summary>
        /// AlwaysUsePackage GUID string.
        /// </summary>
        public const string PackageGuidString = "6d2c8859-71cc-473c-a72b-dfb9b993b96c";

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await this.SetUpRunningDocumentTableEventsAsync(cancellationToken);
        }

        private async Task SetUpRunningDocumentTableEventsAsync(CancellationToken cancellationToken)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var runningDocumentTable = new RunningDocumentTable(this);

            var plugin = new AlwaysUseRunningDocTableEvents(this, runningDocumentTable);

            runningDocumentTable.Advise(plugin);
        }
    }
}
