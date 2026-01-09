namespace TongbaoSwitchCalc
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
            this.comboBoxSquad = new System.Windows.Forms.ComboBox();
            this.lblSquad = new System.Windows.Forms.Label();
            this.checkBoxFortune = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listViewTongbao = new System.Windows.Forms.ListView();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxSquad
            // 
            this.comboBoxSquad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSquad.FormattingEnabled = true;
            this.comboBoxSquad.Location = new System.Drawing.Point(44, 20);
            this.comboBoxSquad.Name = "comboBoxSquad";
            this.comboBoxSquad.Size = new System.Drawing.Size(150, 20);
            this.comboBoxSquad.TabIndex = 0;
            this.comboBoxSquad.SelectedIndexChanged += new System.EventHandler(this.cbSquad_SelectedIndexChanged);
            // 
            // lblSquad
            // 
            this.lblSquad.AutoSize = true;
            this.lblSquad.Location = new System.Drawing.Point(9, 23);
            this.lblSquad.Name = "lblSquad";
            this.lblSquad.Size = new System.Drawing.Size(29, 12);
            this.lblSquad.TabIndex = 1;
            this.lblSquad.Text = "分队";
            // 
            // checkBoxFortune
            // 
            this.checkBoxFortune.AutoSize = true;
            this.checkBoxFortune.Location = new System.Drawing.Point(219, 22);
            this.checkBoxFortune.Name = "checkBoxFortune";
            this.checkBoxFortune.Size = new System.Drawing.Size(120, 16);
            this.checkBoxFortune.TabIndex = 2;
            this.checkBoxFortune.Text = "持有“福祸相依”";
            this.checkBoxFortune.UseVisualStyleBackColor = true;
            this.checkBoxFortune.CheckedChanged += new System.EventHandler(this.checkBoxFortune_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxSquad);
            this.groupBox1.Controls.Add(this.lblSquad);
            this.groupBox1.Controls.Add(this.checkBoxFortune);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(350, 139);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "初始设置";
            // 
            // listViewTongbao
            // 
            this.listViewTongbao.GridLines = true;
            this.listViewTongbao.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewTongbao.HideSelection = false;
            this.listViewTongbao.Location = new System.Drawing.Point(12, 178);
            this.listViewTongbao.MultiSelect = false;
            this.listViewTongbao.Name = "listViewTongbao";
            this.listViewTongbao.Scrollable = false;
            this.listViewTongbao.ShowItemToolTips = true;
            this.listViewTongbao.Size = new System.Drawing.Size(470, 260);
            this.listViewTongbao.TabIndex = 5;
            this.listViewTongbao.UseCompatibleStateImageBehavior = false;
            this.listViewTongbao.ItemActivate += new System.EventHandler(this.listViewTongbao_ItemActivate);
            this.listViewTongbao.SelectedIndexChanged += new System.EventHandler(this.listViewTongbao_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listViewTongbao);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "界园筹谋模拟器";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxSquad;
        private System.Windows.Forms.Label lblSquad;
        private System.Windows.Forms.CheckBox checkBoxFortune;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listViewTongbao;
    }
}