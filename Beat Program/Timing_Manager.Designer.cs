namespace Manage_Beatmap
{
    partial class Timing_Manager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Timing_Manager));
            this.label1 = new System.Windows.Forms.Label();
            this.applyTimingButton = new System.Windows.Forms.Button();
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timingTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(181, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Timing Content";
            // 
            // applyTimingButton
            // 
            this.applyTimingButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.applyTimingButton.AutoSize = true;
            this.applyTimingButton.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.applyTimingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.applyTimingButton.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold);
            this.applyTimingButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.applyTimingButton.Location = new System.Drawing.Point(152, 214);
            this.applyTimingButton.Name = "applyTimingButton";
            this.applyTimingButton.Size = new System.Drawing.Size(183, 35);
            this.applyTimingButton.TabIndex = 3;
            this.applyTimingButton.Text = "Apply timing";
            this.applyTimingButton.UseVisualStyleBackColor = false;
            this.applyTimingButton.Click += new System.EventHandler(this.applyTimingButton_Click);
            // 
            // comboBox
            // 
            this.comboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.Items.AddRange(new object[] {
            "Single",
            "All"});
            this.comboBox.Location = new System.Drawing.Point(237, 166);
            this.comboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(109, 28);
            this.comboBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(112, 169);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 20);
            this.label2.TabIndex = 53;
            this.label2.Text = "Change type:";
            // 
            // timingTextBox
            // 
            this.timingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timingTextBox.Location = new System.Drawing.Point(11, 29);
            this.timingTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.timingTextBox.Name = "timingTextBox";
            this.timingTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.timingTextBox.Size = new System.Drawing.Size(462, 133);
            this.timingTextBox.TabIndex = 54;
            this.timingTextBox.Text = "";
            this.timingTextBox.Enter += new System.EventHandler(this.timingTextBox_Enter);
            // 
            // Timing_Manager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.timingTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox);
            this.Controls.Add(this.applyTimingButton);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "Timing_Manager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Timing Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Timing_Manager_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button applyTimingButton;
        public System.Windows.Forms.ComboBox comboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox timingTextBox;
    }
}