namespace BeatmapManager
{
    partial class SV_Equalizer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SV_Equalizer));
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.bpmTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.svTextBox = new System.Windows.Forms.TextBox();
            this.startOffsetTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.endOffsetTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.applyFullyCheckBox = new System.Windows.Forms.CheckBox();
            this.applyTaikoMapsCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // bpmTextBox
            // 
            resources.ApplyResources(this.bpmTextBox, "bpmTextBox");
            this.bpmTextBox.Name = "bpmTextBox";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // svTextBox
            // 
            resources.ApplyResources(this.svTextBox, "svTextBox");
            this.svTextBox.Name = "svTextBox";
            this.svTextBox.Enter += new System.EventHandler(this.textBox2_Enter);
            // 
            // startOffsetTextBox
            // 
            resources.ApplyResources(this.startOffsetTextBox, "startOffsetTextBox");
            this.startOffsetTextBox.Name = "startOffsetTextBox";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // endOffsetTextBox
            // 
            resources.ApplyResources(this.endOffsetTextBox, "endOffsetTextBox");
            this.endOffsetTextBox.Name = "endOffsetTextBox";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // applyFullyCheckBox
            // 
            resources.ApplyResources(this.applyFullyCheckBox, "applyFullyCheckBox");
            this.applyFullyCheckBox.Name = "applyFullyCheckBox";
            this.applyFullyCheckBox.UseVisualStyleBackColor = true;
            this.applyFullyCheckBox.CheckedChanged += new System.EventHandler(this.applyFullyCheckBox_CheckedChanged);
            // 
            // applyTaikoMapsCheckBox
            // 
            resources.ApplyResources(this.applyTaikoMapsCheckBox, "applyTaikoMapsCheckBox");
            this.applyTaikoMapsCheckBox.Name = "applyTaikoMapsCheckBox";
            this.applyTaikoMapsCheckBox.UseVisualStyleBackColor = true;
            // 
            // SV_Equalizer
            // 
            this.AcceptButton = this.button1;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Controls.Add(this.applyTaikoMapsCheckBox);
            this.Controls.Add(this.applyFullyCheckBox);
            this.Controls.Add(this.endOffsetTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.startOffsetTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.svTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bpmTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SV_Equalizer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox bpmTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox svTextBox;
        private System.Windows.Forms.TextBox startOffsetTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox endOffsetTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox applyFullyCheckBox;
        private System.Windows.Forms.CheckBox applyTaikoMapsCheckBox;
    }
}