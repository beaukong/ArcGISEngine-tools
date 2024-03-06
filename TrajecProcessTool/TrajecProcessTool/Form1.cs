using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TrajecProcessTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        //打开txt转点shp窗体
        private void Txt2SHP_Click(object sender, EventArgs e)
        {
            ParameterSetting paraset = new ParameterSetting(1);
            paraset.Show();          
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
        //打开点shp转线窗体
        private void button1_Click(object sender, EventArgs e)
        {
            ParameterSetting paraset = new ParameterSetting(2);
            paraset.Show();
        }
    }
}