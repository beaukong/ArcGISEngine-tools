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
        //��txtת��shp����
        private void Txt2SHP_Click(object sender, EventArgs e)
        {
            ParameterSetting paraset = new ParameterSetting(1);
            paraset.Show();          
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
        //�򿪵�shpת�ߴ���
        private void button1_Click(object sender, EventArgs e)
        {
            ParameterSetting paraset = new ParameterSetting(2);
            paraset.Show();
        }
    }
}