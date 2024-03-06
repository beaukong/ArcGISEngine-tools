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
    public partial class SelectLayerFeatureForm : Form
    {
        public ILayer selectLayer = null;
        private IApplication m_application=null;
        private Dictionary<string, ILayer> dclayers;
        IMap map = null;
        public static IFeatureClass selectFeatureClass = null;
        public static  string selFldName = null;
        public List<string> feaValueLst = null;
        public static string selFldValue = null;
        public SelectLayerFeatureForm()
        {
            InitializeComponent();
        }
        public SelectLayerFeatureForm(IApplication _m_application)
        {
            InitializeComponent();
            this.m_application = _m_application;
            if (m_application != null)
            {
                map = (m_application.Document as IMxDocument).ActiveView.FocusMap;
            }
            else 
            {
                MessageBox.Show("IApplication为空！");
                return;
            }
            feaValueLst = new List<string>() { "1", "2", "3", "4", "5" };
        }
        //选择图层下拉框的 click事件
        private void comboBox_SelLyr_Click(object sender, EventArgs e)
        {
            if (m_application != null)
            {
                IEnumLayer layers = map.get_Layers();
                ILayer layer = null;
                layers.Reset();//将迭代器重置为集合中的第一层。
                dclayers = new Dictionary<string, ILayer>();
                this.comboBox_SelLyr.Items.Clear();
                while ((layer = layers.Next()) != null)
                {
                    if (!layer.Visible)
                        continue;
                    if (!dclayers.ContainsKey(layer.Name))
                    {
                        dclayers.Add(layer.Name, layer);
                        this.comboBox_SelLyr.Items.Add(layer.Name);//向下拉框中添加地图中的图层
                    }
                }
                if (this.comboBox_SelLyr.Items.Count > 0)
                {
                    this.comboBox_SelLyr.SelectedIndex = 0;
                    getAttri();
                }
            }
        }
        //选择图层 下拉框的选中index改变事件
        private void comboBox_SelLyr_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_SelLyr.SelectedItem!=null)
            {
                this.selectLayer = dclayers[this.comboBox_SelLyr.SelectedItem.ToString()] as ILayer;//获得下拉框选中的图层Layer
                getAttri();//获取该图层的字段列表，添加到选择字段 下拉框中
            }
        }
        //获取该图层的字段列表，添加到选择字段 下拉框中
        private void getAttri()
        {
            selectFeatureClass = (this.selectLayer as IFeatureLayer).FeatureClass;
            if (m_application != null && selectFeatureClass != null)
            {
                this.comboBox_SelFea.Items.Clear();
                IFields fields = selectFeatureClass.Fields;
                for (int i = 0; i < fields.FieldCount; i++)
                {
                    IField fld = fields.get_Field(i);
                    this.comboBox_SelFea.Items.Add(fld.Name);//向下拉框中添加图层的字段
                }
            }
        }
        //选择字段下拉框的click事件
        private void comboBox_SelFea_Click(object sender, EventArgs e)
        {
            if (this.comboBox_SelFea.Items.Count > 0)
            {
                this.comboBox_SelFea.SelectedIndex = 4;
            }
        }
        //选择字段下拉框的选中的index改变事件
        private void comboBox_SelFea_SelectedIndexChanged(object sender, EventArgs e)
        {
            selFldName = this.comboBox_SelFea.SelectedItem.ToString();//获得下拉框选中的字段名字   
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //设置字段值 下拉框的index改变事件
        private void comboBox_FeaValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            selFldValue = this.comboBox_FeaValue.SelectedItem.ToString();//获得选择的标注字段值
        }
        //窗体首次显示后的事件
        private void SelectLayerFeatureForm_Shown(object sender, EventArgs e)
        {
            this.comboBox_FeaValue.Items.Clear();
            foreach (var item in feaValueLst)
            {
                this.comboBox_FeaValue.Items.Add(item);//向标注字段值的下拉框中添加所有种类的标注值
            }
            
        }
    }
}
