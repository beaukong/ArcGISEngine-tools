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
            Application.EnableVisualStyles();//kb����Ӧ�ó���Ŀ�����ʽ
            Application.SetCompatibleTextRenderingDefault(false);//kb rendering����ʾ�� �¿ؼ�ʹ�û��� GDI �� System.Windows.Forms.TextRenderer �ࡣ
            Application.Run(new MainForm());// MainForm��form1.��û�д��������£��ڵ�ǰ�߳��Ͽ�ʼ���б�׼Ӧ�ó�����Ϣѭ����
            //ESRI License Initializer generated code.
            //Do not make any call to ArcObjects after ShutDownApplication()
            m_AOLicenseInitializer.ShutdownApplication();
        }
    }
}