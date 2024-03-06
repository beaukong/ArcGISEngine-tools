using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.SystemUI;

namespace LabelPlugin
{
    /// <summary>
    /// Summary description for LabelTool.
    /// </summary>
    [Guid("c9cd8efa-b43d-4a6c-b257-cf7efc96b580")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("LabelPlugin.LabelTool")]
    public sealed class LabelTool : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private IApplication m_application;
        private IHookHelper m_hookHelper = null;
        public IMap map = null;
        public IActiveView activeView;

        private INewEnvelopeFeedback newEnvelopeFb = null;
        SelectLayerFeatureForm selLyrFeaForm = null;
        public LabelTool()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "������������ע"; //localizable text 
            base.m_caption = "������������ע";  //localizable text 
            base.m_message = "������������ע";  //localizable text
            base.m_toolTip = "������������ע";  //localizable text
            base.m_name = "������������ע";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")            
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            m_application = hook as IApplication;
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                if (m_hookHelper.ActiveView == null)
                {
                    m_hookHelper = null;
                }
            }
            catch
            {
                m_hookHelper = null;
            }
            //Disable if it is not ArcMap
            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add BuildingBaseTool1.OnClick implementation
            if (m_application == null)
            {
                MessageBox.Show("IApplicationΪ�գ�");
                return;
            }
            if (selLyrFeaForm != null)
                selLyrFeaForm.Close();
            selLyrFeaForm = new SelectLayerFeatureForm(this.m_application);//ѡ���עͼ�㡢��ע�ֶΡ���ע�ֶ�ֵ�Ĵ���
            selLyrFeaForm.Visible = true;

            map = (m_application.Document as IMxDocument).ActiveView.FocusMap;
            activeView = map as IActiveView;
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add BuildingBaseTool1.OnMouseDown implementation
            if (activeView == null)
            {
                activeView = m_hookHelper.FocusMap as IActiveView;//������ɶ��
            }
            else
            {
                newEnvelopeFb = null;//��ѡ�ľ���
                newEnvelopeFb = new NewEnvelopeFeedbackClass();
                IPoint point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                newEnvelopeFb.Display = activeView.ScreenDisplay;
                newEnvelopeFb.Start(point);//���ƾ��ε����
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add BuildingBaseTool1.OnMouseMove implementation
            if (newEnvelopeFb != null)
            {
                if (activeView == null)
                {
                    activeView = m_hookHelper.FocusMap as IActiveView;
                }
                IPoint point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                newEnvelopeFb.Display = activeView.ScreenDisplay;
                newEnvelopeFb.MoveTo(point);//���ƾ����ƶ���·��
            }
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add BuildingBaseTool1.OnMouseUp implementation
            if (selLyrFeaForm.Visible == false)
            {
                MessageBox.Show("��ע��������ѹرգ�");
                newEnvelopeFb = null;
                return;
            }
            // �½�һ���༭��
            IEditor editor = m_application.FindExtensionByName("ESRI Object Editor") as IEditor;
            esriEditState editorState = editor.EditState;
            // ��������ڱ༭״̬���������ʾ
            if (editorState != esriEditState.esriStateEditing)
            {
                MessageBox.Show("δ�򿪱༭����");
                newEnvelopeFb = null;
                return;
                //activeView.Refresh();// ˢ��һ����Ļ
            }
            else
            {
                editor.EnableUndoRedo(true);
                editor.StartOperation();
                if (newEnvelopeFb != null)
                {
                    IEnvelope envelope = newEnvelopeFb.Stop();//��û��Ƶľ���
                    if (envelope.IsEmpty!=true)// != null)
                    {
                        try
                        {
                            //�ռ��ѯ������ͼ�㱻ѡ�е�Ҫ�أ�������ע�ֶθ�ֵ
                            SampleLabel(SelectLayerFeatureForm.selectFeatureClass, SelectLayerFeatureForm.selFldName, envelope, SelectLayerFeatureForm.selFldValue);
                            MessageBox.Show("��ע��ɣ�");
                        }
                        catch (Exception e)
                        {

                            MessageBox.Show(e.Message);
                            MessageBox.Show(e.ToString());
                            newEnvelopeFb = null;
                            //activeView.Refresh();
                            editor.StopOperation("������ע");
                            return;
                        }
                    }
                }
                newEnvelopeFb = null;
                //activeView.Refresh();
                editor.StopOperation("������ע");

            }
        }
        #endregion
        public void SampleLabel(IFeatureClass fCls, string fldName, IEnvelope envelope, string value)
        {
            //�ռ��ѯ
            ISpatialFilter sFilter = new SpatialFilterClass();
            sFilter.Geometry = envelope;
            sFilter.GeometryField = "SHAPE";//ShapeҲ����
            sFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;//��ѯ�Ŀռ��ϵ ΪContains��������

            IFeatureCursor featureCursor = fCls.Search(sFilter, false);//ִ�пռ��ѯ
            IFeature feature = null;
            while ((feature = featureCursor.NextFeature()) != null)//������ѯ����Ҫ��
            {
                IFields fields = feature.Fields;
                int attributeIndex = fields.FindField(fldName);// ("LabelCls");
                //���ֶθ�ֵ
                feature.set_Value(attributeIndex, value);
                feature.Store();
            }
            featureCursor.Flush();
            Marshal.FinalReleaseComObject(featureCursor);
        }
    }
}
