using System;
using System.Runtime.InteropServices;
using System.Threading;
using DataAttributor.Services;
using Microsoft.VisualStudio.Shell;
using ProtoAttributor.Commands.Context;
using ProtoAttributor.Parsers.DataContracts;
using ProtoAttributor.Parsers.ProtoContracts;
using ProtoAttributor.Services;
using ProtoAttributor.Settings;
using Task = System.Threading.Tasks.Task;

namespace ProtoAttributor
{
    /// <summary> This is the class that implements the package exposed by this assembly. </summary>
    /// <remarks>
    ///     <para>
    ///         The minimum requirement for a class to be considered a valid package for Visual Studio is to implement
    ///         the IVsPackage interface and register itself with the shell. This package uses the helper classes
    ///         defined inside the Managed Package Framework (MPF) to do it: it derives from the Package class that
    ///         provides the implementation of the IVsPackage interface and uses the registration attributes defined in
    ///         the framework to register itself and its components with the shell. These attributes tell the pkgdef
    ///         creation utility what data to put into .pkgdef file.
    ///     </para>
    ///     <para>
    ///         To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage"
    ///         ...&gt; in .vsixmanifest file.
    ///     </para>
    /// </remarks>
    [ProvideService(typeof(IProtoAttributeService), IsAsyncQueryable = true)]
    [ProvideService(typeof(IDataAnnoAttributeService), IsAsyncQueryable = true)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(VsixOptions.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ProtoAttributorPackage: AsyncPackage
    {
        /// <summary> ProtoAttributorPackage GUID string. </summary>
        

        #region Package Members

        /// <summary>
        ///     Initialization of the package; this method is called right after the package is sited, so this is the
        ///     place where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.
        /// </param>
        /// <param name="progress"> A provider for progress updates. </param>
        /// <returns>
        ///     A task representing the async work of package initialization, or an already completed task if there is
        ///     none. Do not return null from this method.
        /// </returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var protoCallback = new AsyncServiceCreatorCallback(async (IAsyncServiceContainer container, CancellationToken ct, Type serviceType) =>
            {
                if (typeof(IProtoAttributeService) == serviceType)
                    return new ProtoAttributeService(this, new ProtoAttributeAdder(), new ProtoAttributeRemover(), new ProtoAttributeRewriter());
                return null;
            });

            AddService(typeof(IProtoAttributeService), protoCallback, true);

            var datAnnoCallback = new AsyncServiceCreatorCallback(async (IAsyncServiceContainer container, CancellationToken ct, Type serviceType) =>
            {
                if (typeof(IDataAnnoAttributeService) == serviceType)
                    return new DataAnnoAttributeService(this, new DataAttributeAdder(), new DataAttributeRemover(), new DataAttributeRewriter());
                return null;
            });
            AddService(typeof(IDataAnnoAttributeService), datAnnoCallback, true);

            // When initialized asynchronously, the current thread may be a background thread at this point. Do any
            // initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await Task.WhenAll(
                    ProtoAddAttrCommand.InitializeAsync(this),
                    ProtoRenumberAttrCommand.InitializeAsync(this),
                    ProtoRemoveAttrCommand.InitializeAsync(this),
                    DataAnnoAddAttrCommand.InitializeAsync(this),
                    DataAnnoRenumberAttrCommand.InitializeAsync(this),
                    DataAnnoRemoveAttrCommand.InitializeAsync(this),

                    Commands.Menu.ProtoAddAttrCommand.InitializeAsync(this),
                    Commands.Menu.ProtoRenumberAttrCommand.InitializeAsync(this),
                    Commands.Menu.ProtoRemoveAttrCommand.InitializeAsync(this),
                    Commands.Menu.DataAnnoAddAttrCommand.InitializeAsync(this),
                    Commands.Menu.DataAnnoRenumberAttrCommand.InitializeAsync(this),
                    Commands.Menu.DataAnnoRemoveAttrCommand.InitializeAsync(this)
            );
        }
        #endregion Package Members
    }
}
