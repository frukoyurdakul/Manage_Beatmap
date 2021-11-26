namespace BeatmapManager
{
    partial class Metadata_manager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Metadata_manager));
            this.label1 = new System.Windows.Forms.Label();
            this.titleTextBox = new System.Windows.Forms.TextBox();
            this.romanisedTitleTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.artistTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.romanisedArtistTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.bookmarksTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.sourceTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tagsTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(118, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Title:";
            // 
            // titleTextBox
            // 
            this.titleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.titleTextBox.Location = new System.Drawing.Point(172, 12);
            this.titleTextBox.Name = "titleTextBox";
            this.titleTextBox.Size = new System.Drawing.Size(184, 26);
            this.titleTextBox.TabIndex = 0;
            this.titleTextBox.TextChanged += new System.EventHandler(this.titleTextBox_TextChanged);
            this.titleTextBox.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // romanisedTitleTextBox
            // 
            this.romanisedTitleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.romanisedTitleTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.romanisedTitleTextBox.Enabled = false;
            this.romanisedTitleTextBox.Location = new System.Drawing.Point(172, 44);
            this.romanisedTitleTextBox.Name = "romanisedTitleTextBox";
            this.romanisedTitleTextBox.Size = new System.Drawing.Size(184, 26);
            this.romanisedTitleTextBox.TabIndex = 1;
            this.romanisedTitleTextBox.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Romanised Title:";
            // 
            // artistTextBox
            // 
            this.artistTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.artistTextBox.Location = new System.Drawing.Point(172, 76);
            this.artistTextBox.Name = "artistTextBox";
            this.artistTextBox.Size = new System.Drawing.Size(184, 26);
            this.artistTextBox.TabIndex = 2;
            this.artistTextBox.TextChanged += new System.EventHandler(this.artistTextBox_TextChanged);
            this.artistTextBox.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(109, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Artist:";
            // 
            // romanisedArtistTextBox
            // 
            this.romanisedArtistTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.romanisedArtistTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.romanisedArtistTextBox.Enabled = false;
            this.romanisedArtistTextBox.Location = new System.Drawing.Point(172, 108);
            this.romanisedArtistTextBox.Name = "romanisedArtistTextBox";
            this.romanisedArtistTextBox.Size = new System.Drawing.Size(184, 26);
            this.romanisedArtistTextBox.TabIndex = 3;
            this.romanisedArtistTextBox.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(152, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Romanised Artist:";
            // 
            // bookmarksTextBox
            // 
            this.bookmarksTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bookmarksTextBox.Location = new System.Drawing.Point(172, 140);
            this.bookmarksTextBox.Name = "bookmarksTextBox";
            this.bookmarksTextBox.Size = new System.Drawing.Size(184, 26);
            this.bookmarksTextBox.TabIndex = 4;
            this.bookmarksTextBox.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(63, 143);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 20);
            this.label5.TabIndex = 8;
            this.label5.Text = "Bookmarks:";
            // 
            // sourceTextBox
            // 
            this.sourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceTextBox.Location = new System.Drawing.Point(172, 172);
            this.sourceTextBox.Name = "sourceTextBox";
            this.sourceTextBox.Size = new System.Drawing.Size(184, 26);
            this.sourceTextBox.TabIndex = 5;
            this.sourceTextBox.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(95, 175);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 20);
            this.label6.TabIndex = 10;
            this.label6.Text = "Source:";
            // 
            // tagsTextBox
            // 
            this.tagsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagsTextBox.Location = new System.Drawing.Point(172, 204);
            this.tagsTextBox.Name = "tagsTextBox";
            this.tagsTextBox.Size = new System.Drawing.Size(184, 26);
            this.tagsTextBox.TabIndex = 6;
            this.tagsTextBox.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(113, 207);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 20);
            this.label7.TabIndex = 12;
            this.label7.Text = "Tags:";
            // 
            // button
            // 
            this.button.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button.Location = new System.Drawing.Point(12, 249);
            this.button.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(344, 38);
            this.button.TabIndex = 7;
            this.button.Text = "Copy metadata and bookmarks";
            this.button.UseVisualStyleBackColor = false;
            this.button.Click += new System.EventHandler(this.button_Click);
            // 
            // Metadata_manager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(368, 300);
            this.Controls.Add(this.button);
            this.Controls.Add(this.tagsTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.sourceTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.bookmarksTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.romanisedArtistTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.artistTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.romanisedTitleTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.titleTextBox);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Metadata_manager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Metadata";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Metadata_manager_FormClosing);
            this.Load += new System.EventHandler(this.Metadata_manager_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox titleTextBox;
        private System.Windows.Forms.TextBox romanisedTitleTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox artistTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox romanisedArtistTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox bookmarksTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox sourceTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tagsTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button;
    }
}