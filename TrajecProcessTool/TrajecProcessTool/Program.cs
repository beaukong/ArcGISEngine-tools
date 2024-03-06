using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;

namespace TrajecProcessTool
{
    static class Program
    {
        private static LicenseInitializer m_AOLicenseInitializer = new TrajecProcessTool.LicenseInitializer();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //ESRI License Initializer generated code.
            m_AOLicenseInitializer.InitializeApplication
            (new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeAdvanced },
            new esriLicenseExtensionCode[] { });
            Application.EnableVisualStyles();//kb启动应用程序的可视样式
            Application.SetCompatibleTextRenderingDefault(false);//kb rendering：显示。 新控件使用基于 GDI 的 System.Windows.Forms.TextRenderer 类。
            Application.Run(new MainForm());// MainForm是form1.在没有窗体的情况下，在当前线程上开始运行标准应用程序消息循环。
            //ESRI License Initializer generated code.
            //Do not make any call to ArcObjects after ShutDownApplication()
            m_AOLicenseInitializer.ShutdownApplication();
        }
    }
}