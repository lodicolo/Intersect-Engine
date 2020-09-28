using System;
using System.Diagnostics;
using System.Globalization;

namespace Intersect.Server.Prototype
{

    /// <summary>
    /// Please do not modify this without JC's approval! If namespaces are referenced that are not SYSTEM.* then the server won't run cross platform.
    /// If you want to add startup instructions see Classes/ServerStart.cs
    /// </summary>
    public static class PrototypeProgram
    {

        [STAThread]
        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            try
            {
                Type.GetType("Intersect.Server.Prototype.PrototypeServer")
                    ?.GetMethod("Start")
                    ?.Invoke(null, new object[] {args});
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                Debug.WriteLine(exception.StackTrace);
            }
        }

    }

}
