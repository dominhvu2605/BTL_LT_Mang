namespace Client
{
    partial class MenuForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form

        private void InitializeComponent()
        {
            this.singlePlayButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.multiPlayButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // singlePlayButton
            // 
            this.singlePlayButton.Location = new System.Drawing.Point(208, 109);
            this.singlePlayButton.Name = "singlePlayButton";
            this.singlePlayButton.Size = new System.Drawing.Size(86, 43);
            this.singlePlayButton.TabIndex = 0;
            this.singlePlayButton.Text = "Single";
            this.singlePlayButton.UseVisualStyleBackColor = true;
            this.singlePlayButton.Click += new System.EventHandler(this.singlePlayButton_Click);
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(208, 245);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(86, 43);
            this.exitButton.TabIndex = 1;
            this.exitButton.Text = "Quit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // multiPlayButton
            // 
            this.multiPlayButton.Location = new System.Drawing.Point(208, 178);
            this.multiPlayButton.Name = "multiPlayButton";
            this.multiPlayButton.Size = new System.Drawing.Size(86, 43);
            this.multiPlayButton.TabIndex = 2;
            this.multiPlayButton.Text = "PvP";
            this.multiPlayButton.UseVisualStyleBackColor = true;
            this.multiPlayButton.Click += new System.EventHandler(this.multiPlayButton_Click);
            // 
            // MenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 391);
            this.Controls.Add(this.multiPlayButton);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.singlePlayButton);
            this.Name = "MenuForm";
            this.Text = "Gomoku Client";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button singlePlayButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button multiPlayButton;
    }
}

