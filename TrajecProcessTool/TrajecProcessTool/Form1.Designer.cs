namespace TrajecProcessTool
{
    partial class MainForm
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
            this.Txt2SHP = new System.Windows.Forms.Button();
            this.poi2line = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Txt2SHP
            // 
            this.Txt2SHP.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.Txt2SHP.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Txt2SHP.Font = new System.Drawing.Font("Œ¢»Ì—≈∫⁄", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Txt2SHP.Location = new System.Drawing.Point(27, 29);
            this.Txt2SHP.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Txt2SHP.Name = "Txt2SHP";
            this.Txt2SHP.Size = new System.Drawing.Size(94, 38);
            this.Txt2SHP.TabIndex = 0;
            this.Txt2SHP.Text = "TxtToShp";
            this.Txt2SHP.UseVisualStyleBackColor = false;
            this.Txt2SHP.Click += new System.EventHandler(this.Txt2SHP_Click);
            // 
            // poi2line
            // 
            this.poi2line.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.poi2line.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.poi2line.Font = new System.Drawing.Font("Œ¢»Ì—≈∫⁄", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.poi2line.Location = new System.Drawing.Point(142, 30);
            this.poi2line.Name = "poi2line";
            this.poi2line.Size = new System.Drawing.Size(171, 37);
            this.poi2line.TabIndex = 1;
            this.poi2line.Text = "PointShpToLineShp";
            this.poi2line.UseVisualStyleBackColor = false;
            this.poi2line.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(457, 91);
            this.Controls.Add(this.poi2line);
            this.Controls.Add(this.Txt2SHP);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "πÏº£œﬂ¥¶¿Ìπ§æﬂºØ";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Txt2SHP;
        private System.Windows.Forms.Button poi2line;
    }
}

