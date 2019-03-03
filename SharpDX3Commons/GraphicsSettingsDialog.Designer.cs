namespace SharpDXCommons
{
	partial class GraphicsSettingsDialog
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
			this.AdapterSelect = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.OutputSelect = new System.Windows.Forms.ComboBox();
			this.ModeSelect = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.WindowedCheckBox = new System.Windows.Forms.CheckBox();
			this.FullscreenWindowCheckBox = new System.Windows.Forms.CheckBox();
			this.VSyncCheckBox = new System.Windows.Forms.CheckBox();
			this.TripleBufferingCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.LaunchButton = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.groupBox1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// AdapterSelect
			// 
			this.AdapterSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AdapterSelect.FormattingEnabled = true;
			this.AdapterSelect.Location = new System.Drawing.Point(92, 19);
			this.AdapterSelect.Name = "AdapterSelect";
			this.AdapterSelect.Size = new System.Drawing.Size(202, 21);
			this.AdapterSelect.TabIndex = 0;
			this.AdapterSelect.SelectedIndexChanged += new System.EventHandler(this.AdapterSelect_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Display adapter";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 49);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(77, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Adapter output";
			// 
			// OutputSelect
			// 
			this.OutputSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.OutputSelect.FormattingEnabled = true;
			this.OutputSelect.Location = new System.Drawing.Point(92, 46);
			this.OutputSelect.Name = "OutputSelect";
			this.OutputSelect.Size = new System.Drawing.Size(202, 21);
			this.OutputSelect.TabIndex = 3;
			this.OutputSelect.SelectedIndexChanged += new System.EventHandler(this.OutputSelect_SelectedIndexChanged);
			// 
			// ModeSelect
			// 
			this.ModeSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ModeSelect.FormattingEnabled = true;
			this.ModeSelect.Location = new System.Drawing.Point(92, 73);
			this.ModeSelect.Name = "ModeSelect";
			this.ModeSelect.Size = new System.Drawing.Size(202, 21);
			this.ModeSelect.TabIndex = 4;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(19, 76);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(70, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Display mode";
			// 
			// WindowedCheckBox
			// 
			this.WindowedCheckBox.AutoSize = true;
			this.WindowedCheckBox.Checked = true;
			this.WindowedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.WindowedCheckBox.Location = new System.Drawing.Point(14, 122);
			this.WindowedCheckBox.Name = "WindowedCheckBox";
			this.WindowedCheckBox.Size = new System.Drawing.Size(77, 17);
			this.WindowedCheckBox.TabIndex = 6;
			this.WindowedCheckBox.Text = "Windowed";
			this.WindowedCheckBox.UseVisualStyleBackColor = true;
			// 
			// FullscreenWindowCheckBox
			// 
			this.FullscreenWindowCheckBox.AutoSize = true;
			this.FullscreenWindowCheckBox.Location = new System.Drawing.Point(14, 145);
			this.FullscreenWindowCheckBox.Name = "FullscreenWindowCheckBox";
			this.FullscreenWindowCheckBox.Size = new System.Drawing.Size(113, 17);
			this.FullscreenWindowCheckBox.TabIndex = 7;
			this.FullscreenWindowCheckBox.Text = "Fullscreen window";
			this.FullscreenWindowCheckBox.UseVisualStyleBackColor = true;
			// 
			// VSyncCheckBox
			// 
			this.VSyncCheckBox.AutoSize = true;
			this.VSyncCheckBox.Location = new System.Drawing.Point(133, 122);
			this.VSyncCheckBox.Name = "VSyncCheckBox";
			this.VSyncCheckBox.Size = new System.Drawing.Size(57, 17);
			this.VSyncCheckBox.TabIndex = 8;
			this.VSyncCheckBox.Text = "VSync";
			this.VSyncCheckBox.UseVisualStyleBackColor = true;
			// 
			// TripleBufferingCheckBox
			// 
			this.TripleBufferingCheckBox.AutoSize = true;
			this.TripleBufferingCheckBox.Location = new System.Drawing.Point(133, 145);
			this.TripleBufferingCheckBox.Name = "TripleBufferingCheckBox";
			this.TripleBufferingCheckBox.Size = new System.Drawing.Size(96, 17);
			this.TripleBufferingCheckBox.TabIndex = 9;
			this.TripleBufferingCheckBox.Text = "Triple buffering";
			this.TripleBufferingCheckBox.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.AdapterSelect);
			this.groupBox1.Controls.Add(this.OutputSelect);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.ModeSelect);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Location = new System.Drawing.Point(14, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(300, 104);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Display";
			// 
			// LaunchButton
			// 
			this.LaunchButton.Location = new System.Drawing.Point(196, 168);
			this.LaunchButton.Name = "LaunchButton";
			this.LaunchButton.Size = new System.Drawing.Size(118, 50);
			this.LaunchButton.TabIndex = 0;
			this.LaunchButton.Text = "Launch";
			this.LaunchButton.UseVisualStyleBackColor = true;
			this.LaunchButton.Click += new System.EventHandler(this.LaunchButton_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.StatusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 232);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(326, 22);
			this.statusStrip1.SizingGrip = false;
			this.statusStrip1.TabIndex = 12;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// StatusLabel
			// 
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(185, 17);
			this.StatusLabel.Text = "Maximum supported feature level";
			// 
			// GraphicsSettingsDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(326, 254);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.LaunchButton);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.TripleBufferingCheckBox);
			this.Controls.Add(this.VSyncCheckBox);
			this.Controls.Add(this.FullscreenWindowCheckBox);
			this.Controls.Add(this.WindowedCheckBox);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "GraphicsSettingsDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Graphics Settings";
			this.Load += new System.EventHandler(this.GraphicsSettingsDialog_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox AdapterSelect;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox OutputSelect;
		private System.Windows.Forms.ComboBox ModeSelect;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox WindowedCheckBox;
		private System.Windows.Forms.CheckBox FullscreenWindowCheckBox;
		private System.Windows.Forms.CheckBox VSyncCheckBox;
		private System.Windows.Forms.CheckBox TripleBufferingCheckBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button LaunchButton;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel StatusLabel;

	}
}