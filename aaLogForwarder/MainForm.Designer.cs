/*
 * Created by SharpDevelop.
 * User: administrator
 * Date: 4/19/2014
 * Time: 8:30 AM
 * 
 * To change this template use Tools | aaLogReaderOptionsStruct | Coding | Edit Standard Headers.
 */
namespace aaLogForwarder
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ForwardLogsCheckBox = new System.Windows.Forms.CheckBox();
            this.HostLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.HostTextBox = new System.Windows.Forms.TextBox();
            this.PortLabel = new System.Windows.Forms.Label();
            this.PortTextBox = new System.Windows.Forms.MaskedTextBox();
            this.StopButton = new System.Windows.Forms.Button();
            this.AutoScrollCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 56);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(1457, 605);
            this.txtLog.TabIndex = 1;
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(484, 4);
            this.StartButton.Margin = new System.Windows.Forms.Padding(4);
            this.StartButton.Name = "StartButton";
            this.tableLayoutPanel1.SetRowSpan(this.StartButton, 2);
            this.StartButton.Size = new System.Drawing.Size(107, 43);
            this.StartButton.TabIndex = 5;
            this.StartButton.Text = "&Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ForwardLogsCheckBox
            // 
            this.ForwardLogsCheckBox.AutoSize = true;
            this.ForwardLogsCheckBox.Location = new System.Drawing.Point(246, 3);
            this.ForwardLogsCheckBox.Name = "ForwardLogsCheckBox";
            this.ForwardLogsCheckBox.Size = new System.Drawing.Size(231, 21);
            this.ForwardLogsCheckBox.TabIndex = 4;
            this.ForwardLogsCheckBox.Text = "&Forward Logs to Remote Server";
            this.ForwardLogsCheckBox.UseVisualStyleBackColor = true;
            // 
            // HostLabel
            // 
            this.HostLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.HostLabel.AutoSize = true;
            this.HostLabel.Location = new System.Drawing.Point(3, 5);
            this.HostLabel.Name = "HostLabel";
            this.HostLabel.Size = new System.Drawing.Size(37, 17);
            this.HostLabel.TabIndex = 0;
            this.HostLabel.Text = "&Host";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.HostLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.StartButton, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.ForwardLogsCheckBox, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.HostTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.PortLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.PortTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.StopButton, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.AutoScrollCheckBox, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1457, 56);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // HostTextBox
            // 
            this.HostTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.HostTextBox.Location = new System.Drawing.Point(46, 3);
            this.HostTextBox.Name = "HostTextBox";
            this.HostTextBox.Size = new System.Drawing.Size(194, 22);
            this.HostTextBox.TabIndex = 1;
            this.HostTextBox.Text = "localhost";
            // 
            // PortLabel
            // 
            this.PortLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.PortLabel.AutoSize = true;
            this.PortLabel.Location = new System.Drawing.Point(3, 33);
            this.PortLabel.Name = "PortLabel";
            this.PortLabel.Size = new System.Drawing.Size(34, 17);
            this.PortLabel.TabIndex = 2;
            this.PortLabel.Text = "&Port";
            // 
            // PortTextBox
            // 
            this.PortTextBox.Location = new System.Drawing.Point(46, 31);
            this.PortTextBox.Mask = "00000";
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(89, 22);
            this.PortTextBox.TabIndex = 3;
            this.PortTextBox.Text = "14500";
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(599, 4);
            this.StopButton.Margin = new System.Windows.Forms.Padding(4);
            this.StopButton.Name = "StopButton";
            this.tableLayoutPanel1.SetRowSpan(this.StopButton, 2);
            this.StopButton.Size = new System.Drawing.Size(107, 43);
            this.StopButton.TabIndex = 5;
            this.StopButton.Text = "S&top";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // AutoScrollCheckBox
            // 
            this.AutoScrollCheckBox.AutoSize = true;
            this.AutoScrollCheckBox.Checked = true;
            this.AutoScrollCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoScrollCheckBox.Location = new System.Drawing.Point(246, 31);
            this.AutoScrollCheckBox.Name = "AutoScrollCheckBox";
            this.AutoScrollCheckBox.Size = new System.Drawing.Size(146, 21);
            this.AutoScrollCheckBox.TabIndex = 4;
            this.AutoScrollCheckBox.Text = "&Auto-scroll log text";
            this.AutoScrollCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1457, 661);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "aaLogForwarder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox ForwardLogsCheckBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label HostLabel;
        private System.Windows.Forms.TextBox HostTextBox;
        private System.Windows.Forms.Label PortLabel;
        private System.Windows.Forms.MaskedTextBox PortTextBox;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.CheckBox AutoScrollCheckBox;
    }
}
