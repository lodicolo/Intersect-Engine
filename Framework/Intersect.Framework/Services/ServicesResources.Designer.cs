﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Intersect.Framework.Services {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ServicesResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ServicesResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Intersect.Framework.Services.ServicesResources", typeof(ServicesResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} already started..
        /// </summary>
        internal static string BackgroundService_ExecuteAsync_ServiceAlreadyStarted {
            get {
                return ResourceManager.GetString("BackgroundService_ExecuteAsync_ServiceAlreadyStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Service shutdown has already begun..
        /// </summary>
        internal static string BackgroundService_ExecuteAsync_ServiceShutdownAlreadyBegun {
            get {
                return ResourceManager.GetString("BackgroundService_ExecuteAsync_ServiceShutdownAlreadyBegun", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reconfiguring {ServiceName} service due to configuration change....
        /// </summary>
        internal static string BackgroundService_ReconfiguringServiceDueToConfigurationChange {
            get {
                return ResourceManager.GetString("BackgroundService_ReconfiguringServiceDueToConfigurationChange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting {ServiceName} service....
        /// </summary>
        internal static string BackgroundService_StartingService {
            get {
                return ResourceManager.GetString("BackgroundService_StartingService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting {ServiceName} service due to configuration change....
        /// </summary>
        internal static string BackgroundService_StartingServiceDueToConfigurationChange {
            get {
                return ResourceManager.GetString("BackgroundService_StartingServiceDueToConfigurationChange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stopping {ServiceName} service....
        /// </summary>
        internal static string BackgroundService_StoppingService {
            get {
                return ResourceManager.GetString("BackgroundService_StoppingService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stopping {ServiceName} service due to configuration change....
        /// </summary>
        internal static string BackgroundService_StoppingServiceDueToConfigurationChange {
            get {
                return ResourceManager.GetString("BackgroundService_StoppingServiceDueToConfigurationChange", resourceCulture);
            }
        }
    }
}
