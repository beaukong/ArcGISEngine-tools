namespace LabelPlugin
{
    partial class SelectLayerFeatureForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox_SelLyr = new System.Windows.Forms.ComboBox();
            this.comboBox_SelFea = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_FeaValue = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBox_SelLyr
            // 
            this.comboBox_SelLyr.FormattingEnabled = true;
            this.comboBox_SelLyr.Location = new System.Drawing.Point(12, 45);
            this.comboBox_SelLyr.Name = "comboBox_SelLyr";
            this.comboBox_SelLyr.Size = new System.Drawing.Size(229, 23);
            this.comboBox_SelLyr.TabIndex = 0;
            this.comboBox_SelLyr.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelLyr_SelectedIndexChanged);
            this.comboBox_SelLyr.Click += new System.EventHandler(this.comboBox_SelLyr_Click);
            // 
            // comboBox_SelFea
            // 
            this.comboBox_SelFea.FormattingEnabled = true;
            this.comboBox_SelFea.Location = new System.Drawing.Point(260, 45);
            this.comboBox_SelFea.Name = "comboBox_SelFea";
            this.comboBox_SelFea.Size = new System.Drawing.Size(121, 23);
            this.comboBox_SelFea.TabIndex = 1;
            this.comboBox_SelFea.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelFea_SelectedIndexChanged);
            this.comboBox_SelFea.Click += new System.EventHandler(this.comboBox_SelFea_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "选择操作图层";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(259, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "选择标注字段";
            // 
            // comboBox_FeaValue
            // 
            this.comboBox_FeaValue.FormattingEnabled = true;
            this.comboBox_FeaValue.Location = new System.Drawing.Point(406, 44);
            this.comboBox_FeaValue.Name = "comboBox_FeaValue";
            this.comboBox_FeaValue.Size = new System.Drawing.Size(121, 23);
            this.comboBox_FeaValue.TabIndex = 5;
            this.comboBox_FeaValue.SelectedIndexChanged += new System.EventHandler(this.comboBox_FeaValue_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(405, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "设置标注字段值";
            // 
            // SelectLayerFeatureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 76);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_FeaValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_SelFea);
            this.Controls.Add(this.comboBox_SelLyr);
            this.Name = "SelectLayerFeatureForm";
            this.Text = "选择标注图层和字段";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.SelectLayerFeatureForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_SelLyr;
        private System.Windows.Forms.ComboBox comboBox_SelFea;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_FeaValue;
        private System.Windows.Forms.Label label3;
    }
}