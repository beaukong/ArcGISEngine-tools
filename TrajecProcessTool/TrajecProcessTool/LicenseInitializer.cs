using System;
using ESRI.ArcGIS;

namespace TrajecProcessTool
{
    internal partial class LicenseInitializer
    {
        public LicenseInitializer()
        {
            //kb ί�С���ʾ�����������¼����ݵ��¼��ķ�����
            ResolveBindingEvent += new EventHandler(BindingArcGISRuntime);
        }

        void BindingArcGISRuntime(object sender, EventArgs e)
        {
            //
            // TODO: Modify ArcGIS runtime binding code as needed
            //
            //kb RuntimeManager.Bind ֻ�ǰ�һ����Ʒ��������Engine�Ļ�����Engine��Ʒ��Desktop�Ļ���Desktop��EngineOrDesktop�Ļ������Ȱ�Engine��û�а�װEngine�Ļ�����Desktop��
            if (!RuntimeManager.Bind(ProductCode.Desktop))
            {
                // Failed to bind, announce and force exit
                System.Windows.Forms.MessageBox.Show("Invalid ArcGIS runtime binding. Application will shut down.");
                System.Environment.Exit(0);
            }
        }
    }
}