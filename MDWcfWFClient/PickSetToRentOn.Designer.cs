﻿namespace MDWcfWFClient
{
    partial class PickSetToRentOn
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listBoxCardsInSet = new System.Windows.Forms.ListBox();
            this.listBoxSet = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.listBoxCardsInHand = new System.Windows.Forms.ListBox();
            this.buttonRentDouble = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listBoxCardsInSet);
            this.groupBox1.Controls.Add(this.listBoxSet);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(377, 225);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pick Set to use rent card with";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // listBoxCardsInSet
            // 
            this.listBoxCardsInSet.FormattingEnabled = true;
            this.listBoxCardsInSet.Location = new System.Drawing.Point(6, 120);
            this.listBoxCardsInSet.Name = "listBoxCardsInSet";
            this.listBoxCardsInSet.Size = new System.Drawing.Size(364, 95);
            this.listBoxCardsInSet.TabIndex = 1;
            // 
            // listBoxSet
            // 
            this.listBoxSet.FormattingEnabled = true;
            this.listBoxSet.Location = new System.Drawing.Point(6, 19);
            this.listBoxSet.Name = "listBoxSet";
            this.listBoxSet.Size = new System.Drawing.Size(364, 95);
            this.listBoxSet.TabIndex = 0;
            this.listBoxSet.SelectedIndexChanged += new System.EventHandler(this.listBoxSet_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(18, 243);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(179, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Use Rent Card On Selected Set";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(203, 243);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(179, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // listBoxCardsInHand
            // 
            this.listBoxCardsInHand.FormattingEnabled = true;
            this.listBoxCardsInHand.Location = new System.Drawing.Point(18, 272);
            this.listBoxCardsInHand.Name = "listBoxCardsInHand";
            this.listBoxCardsInHand.Size = new System.Drawing.Size(364, 95);
            this.listBoxCardsInHand.TabIndex = 4;
            // 
            // buttonRentDouble
            // 
            this.buttonRentDouble.Location = new System.Drawing.Point(18, 373);
            this.buttonRentDouble.Name = "buttonRentDouble";
            this.buttonRentDouble.Size = new System.Drawing.Size(367, 23);
            this.buttonRentDouble.TabIndex = 5;
            this.buttonRentDouble.Text = "Rent on Selected Set with Double the Rent Card";
            this.buttonRentDouble.UseVisualStyleBackColor = true;
            this.buttonRentDouble.Click += new System.EventHandler(this.buttonRentDouble_Click);
            // 
            // PickSetToRentOn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 401);
            this.Controls.Add(this.buttonRentDouble);
            this.Controls.Add(this.listBoxCardsInHand);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Name = "PickSetToRentOn";
            this.Text = "PickSetToRentOn";
            this.Load += new System.EventHandler(this.PickSetToRentOn_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listBoxCardsInSet;
        private System.Windows.Forms.ListBox listBoxSet;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListBox listBoxCardsInHand;
        private System.Windows.Forms.Button buttonRentDouble;
    }
}