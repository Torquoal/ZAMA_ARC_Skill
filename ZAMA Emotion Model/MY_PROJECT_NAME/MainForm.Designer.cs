namespace ZAMAEmotionModel {
  partial class MainForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
            this.tbKeyword = new System.Windows.Forms.TextBox();
            this.numValence = new System.Windows.Forms.TextBox();
            this.numArousal = new System.Windows.Forms.TextBox();
            this.tbTrigger = new System.Windows.Forms.TextBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.emotionLabel = new System.Windows.Forms.Label();
            this.valenceLabel = new System.Windows.Forms.Label();
            this.arousalLabel = new System.Windows.Forms.Label();
            this.btnTrigger = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.moodLabelTitle = new System.Windows.Forms.Label();
            this.moodValueLabel = new System.Windows.Forms.Label();
            this.personalityLabelTitle = new System.Windows.Forms.Label();
            this.personalityValueLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbKeyword
            // 
            this.tbKeyword.Location = new System.Drawing.Point(62, 21);
            this.tbKeyword.Name = "tbKeyword";
            this.tbKeyword.Size = new System.Drawing.Size(100, 20);
            this.tbKeyword.TabIndex = 0;
            // 
            // numValence
            // 
            this.numValence.Location = new System.Drawing.Point(203, 21);
            this.numValence.Name = "numValence";
            this.numValence.Size = new System.Drawing.Size(100, 20);
            this.numValence.TabIndex = 1;
            // 
            // numArousal
            // 
            this.numArousal.Location = new System.Drawing.Point(341, 21);
            this.numArousal.Name = "numArousal";
            this.numArousal.Size = new System.Drawing.Size(100, 20);
            this.numArousal.TabIndex = 2;
            // 
            // tbTrigger
            // 
            this.tbTrigger.Location = new System.Drawing.Point(341, 100);
            this.tbTrigger.Name = "tbTrigger";
            this.tbTrigger.Size = new System.Drawing.Size(100, 20);
            this.tbTrigger.TabIndex = 3;
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(488, 12);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(75, 23);
            this.btnRegister.TabIndex = 4;
            this.btnRegister.Text = "Register Event";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // emotionLabel
            // 
            this.emotionLabel.AutoSize = true;
            this.emotionLabel.Location = new System.Drawing.Point(79, 177);
            this.emotionLabel.Name = "emotionLabel";
            this.emotionLabel.Size = new System.Drawing.Size(45, 13);
            this.emotionLabel.TabIndex = 5;
            this.emotionLabel.Text = "Emotion";
            // 
            // valenceLabel
            // 
            this.valenceLabel.AutoSize = true;
            this.valenceLabel.Location = new System.Drawing.Point(211, 177);
            this.valenceLabel.Name = "valenceLabel";
            this.valenceLabel.Size = new System.Drawing.Size(46, 13);
            this.valenceLabel.TabIndex = 6;
            this.valenceLabel.Text = "Valence";
            // 
            // arousalLabel
            // 
            this.arousalLabel.AutoSize = true;
            this.arousalLabel.Location = new System.Drawing.Point(338, 177);
            this.arousalLabel.Name = "arousalLabel";
            this.arousalLabel.Size = new System.Drawing.Size(42, 13);
            this.arousalLabel.TabIndex = 7;
            this.arousalLabel.Text = "Arousal";
            // 
            // btnTrigger
            // 
            this.btnTrigger.Location = new System.Drawing.Point(476, 98);
            this.btnTrigger.Name = "btnTrigger";
            this.btnTrigger.Size = new System.Drawing.Size(112, 23);
            this.btnTrigger.TabIndex = 8;
            this.btnTrigger.Text = "Trigger Event";
            this.btnTrigger.UseVisualStyleBackColor = true;
            this.btnTrigger.Click += new System.EventHandler(this.btnTrigger_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(79, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Keyword";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(227, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Valence";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(366, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Arousal";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(366, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Test Trigger";
            // 
            // moodLabelTitle
            // 
            this.moodLabelTitle.AutoSize = true;
            this.moodLabelTitle.Location = new System.Drawing.Point(79, 210);
            this.moodLabelTitle.Name = "moodLabelTitle";
            this.moodLabelTitle.Size = new System.Drawing.Size(34, 13);
            this.moodLabelTitle.TabIndex = 13;
            this.moodLabelTitle.Text = "Mood";
            // 
            // moodValueLabel
            // 
            this.moodValueLabel.AutoSize = true;
            this.moodValueLabel.Location = new System.Drawing.Point(133, 210);
            this.moodValueLabel.Name = "moodValueLabel";
            this.moodValueLabel.Size = new System.Drawing.Size(74, 13);
            this.moodValueLabel.TabIndex = 14;
            this.moodValueLabel.Text = "Mood: Neutral";
            // 
            // personalityLabelTitle
            // 
            this.personalityLabelTitle.AutoSize = true;
            this.personalityLabelTitle.Location = new System.Drawing.Point(79, 236);
            this.personalityLabelTitle.Name = "personalityLabelTitle";
            this.personalityLabelTitle.Size = new System.Drawing.Size(58, 13);
            this.personalityLabelTitle.TabIndex = 15;
            this.personalityLabelTitle.Text = "Personality";
            // 
            // personalityValueLabel
            // 
            this.personalityValueLabel.AutoSize = true;
            this.personalityValueLabel.Location = new System.Drawing.Point(143, 236);
            this.personalityValueLabel.Name = "personalityValueLabel";
            this.personalityValueLabel.Size = new System.Drawing.Size(119, 13);
            this.personalityValueLabel.TabIndex = 16;
            this.personalityValueLabel.Text = "Temperament: Relaxed";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(600, 292);
            this.Controls.Add(this.personalityValueLabel);
            this.Controls.Add(this.personalityLabelTitle);
            this.Controls.Add(this.moodValueLabel);
            this.Controls.Add(this.moodLabelTitle);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnTrigger);
            this.Controls.Add(this.arousalLabel);
            this.Controls.Add(this.valenceLabel);
            this.Controls.Add(this.emotionLabel);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.tbTrigger);
            this.Controls.Add(this.numArousal);
            this.Controls.Add(this.numValence);
            this.Controls.Add(this.tbKeyword);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

        #endregion

        private System.Windows.Forms.TextBox tbKeyword;
        private System.Windows.Forms.TextBox numValence;
        private System.Windows.Forms.TextBox numArousal;
        private System.Windows.Forms.TextBox tbTrigger;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Label emotionLabel;
        private System.Windows.Forms.Label valenceLabel;
        private System.Windows.Forms.Label arousalLabel;
        private System.Windows.Forms.Button btnTrigger;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label moodLabelTitle;
        private System.Windows.Forms.Label moodValueLabel;
        private System.Windows.Forms.Label personalityLabelTitle;
        private System.Windows.Forms.Label personalityValueLabel;
    }
}