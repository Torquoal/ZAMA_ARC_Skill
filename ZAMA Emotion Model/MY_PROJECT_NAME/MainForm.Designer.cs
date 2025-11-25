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
            this.moodValueLabel = new System.Windows.Forms.Label();
            this.personalityValueLabel = new System.Windows.Forms.Label();
            this.chkRandomPersonality = new System.Windows.Forms.CheckBox();
            this.personalityValenceText = new System.Windows.Forms.TextBox();
            this.personalityArousalText = new System.Windows.Forms.TextBox();
            this.btnApplyPersonality = new System.Windows.Forms.Button();
            this.chkMoodShift = new System.Windows.Forms.CheckBox();
            this.chkPersonalityShift = new System.Windows.Forms.CheckBox();
            this.eventsGrid = new System.Windows.Forms.DataGridView();
            this.colKeyword = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValence = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colArousal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnLoadEvent = new System.Windows.Forms.Button();
            this.btnDeleteEvent = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.eventsGrid)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbKeyword
            // 
            this.tbKeyword.Location = new System.Drawing.Point(6, 45);
            this.tbKeyword.Name = "tbKeyword";
            this.tbKeyword.Size = new System.Drawing.Size(100, 20);
            this.tbKeyword.TabIndex = 0;
            // 
            // numValence
            // 
            this.numValence.Location = new System.Drawing.Point(144, 45);
            this.numValence.Name = "numValence";
            this.numValence.Size = new System.Drawing.Size(100, 20);
            this.numValence.TabIndex = 1;
            // 
            // numArousal
            // 
            this.numArousal.Location = new System.Drawing.Point(286, 45);
            this.numArousal.Name = "numArousal";
            this.numArousal.Size = new System.Drawing.Size(100, 20);
            this.numArousal.TabIndex = 2;
            // 
            // tbTrigger
            // 
            this.tbTrigger.Location = new System.Drawing.Point(325, 37);
            this.tbTrigger.Name = "tbTrigger";
            this.tbTrigger.Size = new System.Drawing.Size(120, 20);
            this.tbTrigger.TabIndex = 3;
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(418, 42);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(96, 23);
            this.btnRegister.TabIndex = 4;
            this.btnRegister.Text = "Register Event";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // emotionLabel
            // 
            this.emotionLabel.AutoSize = true;
            this.emotionLabel.Location = new System.Drawing.Point(6, 45);
            this.emotionLabel.Name = "emotionLabel";
            this.emotionLabel.Size = new System.Drawing.Size(45, 13);
            this.emotionLabel.TabIndex = 5;
            this.emotionLabel.Text = "Emotion";
            // 
            // valenceLabel
            // 
            this.valenceLabel.AutoSize = true;
            this.valenceLabel.Location = new System.Drawing.Point(109, 45);
            this.valenceLabel.Name = "valenceLabel";
            this.valenceLabel.Size = new System.Drawing.Size(46, 13);
            this.valenceLabel.TabIndex = 6;
            this.valenceLabel.Text = "Valence";
            // 
            // arousalLabel
            // 
            this.arousalLabel.AutoSize = true;
            this.arousalLabel.Location = new System.Drawing.Point(209, 45);
            this.arousalLabel.Name = "arousalLabel";
            this.arousalLabel.Size = new System.Drawing.Size(42, 13);
            this.arousalLabel.TabIndex = 7;
            this.arousalLabel.Text = "Arousal";
            // 
            // btnTrigger
            // 
            this.btnTrigger.Location = new System.Drawing.Point(460, 35);
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
            this.label1.Location = new System.Drawing.Point(32, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Event Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(141, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Set Valence (-10 to10)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(283, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Set Arousal (-10 to 10)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(350, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Event to Test";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // moodValueLabel
            // 
            this.moodValueLabel.AutoSize = true;
            this.moodValueLabel.Location = new System.Drawing.Point(120, 19);
            this.moodValueLabel.Name = "moodValueLabel";
            this.moodValueLabel.Size = new System.Drawing.Size(74, 13);
            this.moodValueLabel.TabIndex = 14;
            this.moodValueLabel.Text = "Mood: Neutral";
            // 
            // personalityValueLabel
            // 
            this.personalityValueLabel.AutoSize = true;
            this.personalityValueLabel.Location = new System.Drawing.Point(260, 19);
            this.personalityValueLabel.Name = "personalityValueLabel";
            this.personalityValueLabel.Size = new System.Drawing.Size(117, 13);
            this.personalityValueLabel.TabIndex = 16;
            this.personalityValueLabel.Text = "Temperament: Relaxed";
            // 
            // chkRandomPersonality
            // 
            this.chkRandomPersonality.AutoSize = true;
            this.chkRandomPersonality.Location = new System.Drawing.Point(368, 101);
            this.chkRandomPersonality.Name = "chkRandomPersonality";
            this.chkRandomPersonality.Size = new System.Drawing.Size(170, 17);
            this.chkRandomPersonality.TabIndex = 17;
            this.chkRandomPersonality.Text = "Randomize personality on start";
            this.chkRandomPersonality.UseVisualStyleBackColor = true;
            this.chkRandomPersonality.CheckedChanged += new System.EventHandler(this.chkRandomPersonality_CheckedChanged);
            // 
            // personalityValenceText
            // 
            this.personalityValenceText.Location = new System.Drawing.Point(30, 66);
            this.personalityValenceText.Name = "personalityValenceText";
            this.personalityValenceText.Size = new System.Drawing.Size(100, 20);
            this.personalityValenceText.TabIndex = 18;
            // 
            // personalityArousalText
            // 
            this.personalityArousalText.Location = new System.Drawing.Point(196, 66);
            this.personalityArousalText.Name = "personalityArousalText";
            this.personalityArousalText.Size = new System.Drawing.Size(100, 20);
            this.personalityArousalText.TabIndex = 19;
            // 
            // btnApplyPersonality
            // 
            this.btnApplyPersonality.Location = new System.Drawing.Point(356, 64);
            this.btnApplyPersonality.Name = "btnApplyPersonality";
            this.btnApplyPersonality.Size = new System.Drawing.Size(125, 23);
            this.btnApplyPersonality.TabIndex = 20;
            this.btnApplyPersonality.Text = "Apply Personality";
            this.btnApplyPersonality.UseVisualStyleBackColor = true;
            this.btnApplyPersonality.Click += new System.EventHandler(this.btnApplyPersonality_Click);
            // 
            // chkMoodShift
            // 
            this.chkMoodShift.AutoSize = true;
            this.chkMoodShift.Location = new System.Drawing.Point(10, 101);
            this.chkMoodShift.Name = "chkMoodShift";
            this.chkMoodShift.Size = new System.Drawing.Size(160, 17);
            this.chkMoodShift.TabIndex = 21;
            this.chkMoodShift.Text = "Allow mood shift from events";
            this.chkMoodShift.UseVisualStyleBackColor = true;
            this.chkMoodShift.CheckedChanged += new System.EventHandler(this.chkMoodShift_CheckedChanged);
            // 
            // chkPersonalityShift
            // 
            this.chkPersonalityShift.AutoSize = true;
            this.chkPersonalityShift.Location = new System.Drawing.Point(178, 101);
            this.chkPersonalityShift.Name = "chkPersonalityShift";
            this.chkPersonalityShift.Size = new System.Drawing.Size(184, 17);
            this.chkPersonalityShift.TabIndex = 22;
            this.chkPersonalityShift.Text = "Allow personality shift from events";
            this.chkPersonalityShift.UseVisualStyleBackColor = true;
            this.chkPersonalityShift.CheckedChanged += new System.EventHandler(this.chkPersonalityShift_CheckedChanged);
            // 
            // eventsGrid
            // 
            this.eventsGrid.AllowUserToAddRows = false;
            this.eventsGrid.AllowUserToDeleteRows = false;
            this.eventsGrid.AllowUserToResizeRows = false;
            this.eventsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.eventsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colKeyword,
            this.colValence,
            this.colArousal});
            this.eventsGrid.Location = new System.Drawing.Point(6, 71);
            this.eventsGrid.MultiSelect = false;
            this.eventsGrid.Name = "eventsGrid";
            this.eventsGrid.ReadOnly = true;
            this.eventsGrid.RowHeadersVisible = false;
            this.eventsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.eventsGrid.Size = new System.Drawing.Size(404, 213);
            this.eventsGrid.TabIndex = 23;
            this.eventsGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.eventsGrid_CellContentClick);
            // 
            // colKeyword
            // 
            this.colKeyword.HeaderText = "Keyword";
            this.colKeyword.Name = "colKeyword";
            this.colKeyword.ReadOnly = true;
            this.colKeyword.Width = 180;
            // 
            // colValence
            // 
            this.colValence.HeaderText = "Valence";
            this.colValence.Name = "colValence";
            this.colValence.ReadOnly = true;
            // 
            // colArousal
            // 
            this.colArousal.HeaderText = "Arousal";
            this.colArousal.Name = "colArousal";
            this.colArousal.ReadOnly = true;
            // 
            // btnLoadEvent
            // 
            this.btnLoadEvent.Location = new System.Drawing.Point(418, 112);
            this.btnLoadEvent.Name = "btnLoadEvent";
            this.btnLoadEvent.Size = new System.Drawing.Size(141, 23);
            this.btnLoadEvent.TabIndex = 24;
            this.btnLoadEvent.Text = "Load Selected Event";
            this.btnLoadEvent.UseVisualStyleBackColor = true;
            this.btnLoadEvent.Click += new System.EventHandler(this.btnLoadEvent_Click);
            // 
            // btnDeleteEvent
            // 
            this.btnDeleteEvent.Location = new System.Drawing.Point(418, 141);
            this.btnDeleteEvent.Name = "btnDeleteEvent";
            this.btnDeleteEvent.Size = new System.Drawing.Size(141, 23);
            this.btnDeleteEvent.TabIndex = 25;
            this.btnDeleteEvent.Text = "Delete Selected Event";
            this.btnDeleteEvent.UseVisualStyleBackColor = true;
            this.btnDeleteEvent.Click += new System.EventHandler(this.btnDeleteEvent_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(151, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Personality Valence (-10 to 10)";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(175, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(147, 13);
            this.label6.TabIndex = 27;
            this.label6.Text = "Personality Arousal (-10 to 10)";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.chkPersonalityShift);
            this.groupBox1.Controls.Add(this.chkMoodShift);
            this.groupBox1.Controls.Add(this.btnApplyPersonality);
            this.groupBox1.Controls.Add(this.personalityArousalText);
            this.groupBox1.Controls.Add(this.personalityValenceText);
            this.groupBox1.Controls.Add(this.chkRandomPersonality);
            this.groupBox1.Controls.Add(this.personalityValueLabel);
            this.groupBox1.Controls.Add(this.moodValueLabel);
            this.groupBox1.Location = new System.Drawing.Point(18, 402);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(592, 121);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Personality";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnDeleteEvent);
            this.groupBox2.Controls.Add(this.btnLoadEvent);
            this.groupBox2.Controls.Add(this.eventsGrid);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnRegister);
            this.groupBox2.Controls.Add(this.numArousal);
            this.groupBox2.Controls.Add(this.numValence);
            this.groupBox2.Controls.Add(this.tbKeyword);
            this.groupBox2.Location = new System.Drawing.Point(15, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(592, 290);
            this.groupBox2.TabIndex = 29;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Emotion Events";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.arousalLabel);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.valenceLabel);
            this.groupBox3.Controls.Add(this.emotionLabel);
            this.groupBox3.Controls.Add(this.btnTrigger);
            this.groupBox3.Controls.Add(this.tbTrigger);
            this.groupBox3.Location = new System.Drawing.Point(18, 311);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(588, 75);
            this.groupBox3.TabIndex = 30;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Emotion Response Result";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(29, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(150, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Emotional Response Outcome";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(630, 547);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.eventsGrid)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Label moodValueLabel;
        private System.Windows.Forms.Label personalityValueLabel;
        private System.Windows.Forms.CheckBox chkRandomPersonality;
        private System.Windows.Forms.TextBox personalityValenceText;
        private System.Windows.Forms.TextBox personalityArousalText;
        private System.Windows.Forms.Button btnApplyPersonality;
        private System.Windows.Forms.CheckBox chkMoodShift;
        private System.Windows.Forms.CheckBox chkPersonalityShift;
        private System.Windows.Forms.DataGridView eventsGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKeyword;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValence;
        private System.Windows.Forms.DataGridViewTextBoxColumn colArousal;
        private System.Windows.Forms.Button btnLoadEvent;
        private System.Windows.Forms.Button btnDeleteEvent;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label7;
    }
}