using System;
using ESRI.ArcGIS;

namespace TrajecProcessTool
{
    internal partial class LicenseInitializer
    {
        public LicenseInitializer()
        {
            //kb 委托。表示将处理不包含事件数据的事件的方法。
            ResolveBindingEvent += new EventHandler(BindingArcGISRuntime);
        }

        void BindingArcGISRuntime(object sender, EventArgs e)
        {
            //
            // TODO: Modify ArcGIS runtime binding code as needed
            //
            //kb RuntimeManager.Bind 只是绑定一个产品，参数是Engine的话即绑定Engine产品，Desktop的话绑定Desktop，EngineOrDesktop的话会优先绑定Engine，没有安装Engine的话，绑定Desktop。
            if (!RuntimeManager.Bind(ProductCode.Desktop))
            {
                // Failed to bind, announce and force exit
                System.Windows.Forms.MessageBox.Show("Invalid ArcGIS runtime binding. Application will shut down.");
                System.Environment.Exit(0);
            }
        }
    }
}