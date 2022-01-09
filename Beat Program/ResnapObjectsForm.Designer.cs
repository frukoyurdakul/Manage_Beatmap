
namespace Manage_Beatmap
{
    partial class ResnapObjectsForm
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
            this.components = new System.ComponentModel.Container();
            this.wholeMapCheckBox = new System.Windows.Forms.CheckBox();
            this.allTaikoDiffsCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.startOffsetTextBox = new System.Windows.Forms.TextBox();
            this.endOffsetTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.applyButton = new System.Windows.Forms.Button();
            this.snapGreenLinesCheckBox = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // wholeMapCheckBox
            // 
            this.wholeMapCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.wholeMapCheckBox.AutoSize = true;
            this.wholeMapCheckBox.Checked = true;
            this.wholeMapCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wholeMapCheckBox.Location = new System.Drawing.Point(93, 43);
            this.wholeMapCheckBox.Margin = new System.Windows.Forms.Padding(5);
            this.wholeMapCheckBox.Name = "wholeMapCheckBox";
            this.wholeMapCheckBox.Size = new System.Drawing.Size(183, 24);
            this.wholeMapCheckBox.TabIndex = 0;
            this.wholeMapCheckBox.Text = "Apply to whole map";
            this.toolTip1.SetToolTip(this.wholeMapCheckBox, "Allows you to resnap everything based\r\non the settings to the whole map.\r\n\r\nIf yo" +
        "u want to resnap only a section,\r\nuncheck this and enter start and end\r\noffsets." +
        "");
            this.wholeMapCheckBox.UseVisualStyleBackColor = true;
            this.wholeMapCheckBox.CheckedChanged += new System.EventHandler(this.wholeMapCheckBox_CheckedChanged);
            // 
            // allTaikoDiffsCheckBox
            // 
            this.allTaikoDiffsCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.allTaikoDiffsCheckBox.AutoSize = true;
            this.allTaikoDiffsCheckBox.Location = new System.Drawing.Point(61, 77);
            this.allTaikoDiffsCheckBox.Margin = new System.Windows.Forms.Padding(5);
            this.allTaikoDiffsCheckBox.Name = "allTaikoDiffsCheckBox";
            this.allTaikoDiffsCheckBox.Size = new System.Drawing.Size(247, 24);
            this.allTaikoDiffsCheckBox.TabIndex = 1;
            this.allTaikoDiffsCheckBox.Text = "Apply to all taiko difficulties";
            this.toolTip1.SetToolTip(this.allTaikoDiffsCheckBox, "Applies the selected functions to all\r\ntaiko difficulties. Other mode diffs\r\nare " +
        "untouched.");
            this.allTaikoDiffsCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Start offset:";
            // 
            // startOffsetTextBox
            // 
            this.startOffsetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.startOffsetTextBox.Enabled = false;
            this.startOffsetTextBox.Location = new System.Drawing.Point(123, 121);
            this.startOffsetTextBox.Name = "startOffsetTextBox";
            this.startOffsetTextBox.Size = new System.Drawing.Size(231, 26);
            this.startOffsetTextBox.TabIndex = 3;
            this.toolTip1.SetToolTip(this.startOffsetTextBox, "The start offset to begin resnapping from.");
            // 
            // endOffsetTextBox
            // 
            this.endOffsetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.endOffsetTextBox.Enabled = false;
            this.endOffsetTextBox.Location = new System.Drawing.Point(123, 153);
            this.endOffsetTextBox.Name = "endOffsetTextBox";
            this.endOffsetTextBox.Size = new System.Drawing.Size(231, 26);
            this.endOffsetTextBox.TabIndex = 5;
            this.toolTip1.SetToolTip(this.endOffsetTextBox, "The end offset to end resnappings.");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "End offset:";
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.applyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.applyButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.applyButton.Location = new System.Drawing.Point(12, 195);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(344, 35);
            this.applyButton.TabIndex = 9;
            this.applyButton.Text = "Apply";
            this.toolTip1.SetToolTip(this.applyButton, "plz support me on patreon");
            this.applyButton.UseVisualStyleBackColor = false;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // snapGreenLinesCheckBox
            // 
            this.snapGreenLinesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.snapGreenLinesCheckBox.AutoSize = true;
            this.snapGreenLinesCheckBox.Checked = true;
            this.snapGreenLinesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.snapGreenLinesCheckBox.Location = new System.Drawing.Point(41, 9);
            this.snapGreenLinesCheckBox.Margin = new System.Windows.Forms.Padding(5);
            this.snapGreenLinesCheckBox.Name = "snapGreenLinesCheckBox";
            this.snapGreenLinesCheckBox.Size = new System.Drawing.Size(287, 24);
            this.snapGreenLinesCheckBox.TabIndex = 10;
            this.snapGreenLinesCheckBox.Text = "Snap green lines if behind notes";
            this.toolTip1.SetToolTip(this.snapGreenLinesCheckBox, "Snaps the green lines with -1ms offset\r\nif the result causes the green lines\r\nto " +
        "be after the notes by a 10ms margin.\r\n\r\nEnabling this will not move green lines\r" +
        "\nthat change kiai state.");
            this.snapGreenLinesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ResnapObjectsForm
            // 
            this.AcceptButton = this.applyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(368, 239);
            this.Controls.Add(this.snapGreenLinesCheckBox);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.endOffsetTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.startOffsetTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.allTaikoDiffsCheckBox);
            this.Controls.Add(this.wholeMapCheckBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ResnapObjectsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ResnapObjectsForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox wholeMapCheckBox;
        private System.Windows.Forms.CheckBox allTaikoDiffsCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox startOffsetTextBox;
        private System.Windows.Forms.TextBox endOffsetTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox snapGreenLinesCheckBox;
    }
}