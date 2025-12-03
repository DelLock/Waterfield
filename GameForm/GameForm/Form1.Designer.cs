namespace Battleship
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pnlPlayer = new Battleship.GradientPanel();
            this.lblPlayer = new System.Windows.Forms.Label();
            this.pnlOpponent = new Battleship.GradientPanel();
            this.lblOpponent = new System.Windows.Forms.Label();
            this.btnNewGame = new Battleship.RoundedButton();
            this.statusLabel = new System.Windows.Forms.Label();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelShipsInfo = new System.Windows.Forms.Panel();
            this.lblShipsInfo = new System.Windows.Forms.Label();
            this.panelTitle.SuspendLayout();
            this.panelShipsInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlPlayer
            // 
            this.pnlPlayer.BackColor = System.Drawing.Color.White;
            this.pnlPlayer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPlayer.ColorBottom = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.pnlPlayer.ColorTop = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.pnlPlayer.Location = new System.Drawing.Point(20, 100);
            this.pnlPlayer.Name = "pnlPlayer";
            this.pnlPlayer.Size = new System.Drawing.Size(320, 320);
            this.pnlPlayer.TabIndex = 0;
            // 
            // lblPlayer
            // 
            this.lblPlayer.AutoSize = true;
            this.lblPlayer.BackColor = System.Drawing.Color.Transparent;
            this.lblPlayer.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPlayer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(112)))));
            this.lblPlayer.Location = new System.Drawing.Point(20, 75);
            this.lblPlayer.Name = "lblPlayer";
            this.lblPlayer.Size = new System.Drawing.Size(115, 20);
            this.lblPlayer.TabIndex = 1;
            this.lblPlayer.Text = "🚢 Ваше поле";
            // 
            // pnlOpponent
            // 
            this.pnlOpponent.BackColor = System.Drawing.Color.White;
            this.pnlOpponent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOpponent.ColorBottom = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.pnlOpponent.ColorTop = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.pnlOpponent.Location = new System.Drawing.Point(370, 100);
            this.pnlOpponent.Name = "pnlOpponent";
            this.pnlOpponent.Size = new System.Drawing.Size(320, 320);
            this.pnlOpponent.TabIndex = 2;
            // 
            // lblOpponent
            // 
            this.lblOpponent.AutoSize = true;
            this.lblOpponent.BackColor = System.Drawing.Color.Transparent;
            this.lblOpponent.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblOpponent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(112)))));
            this.lblOpponent.Location = new System.Drawing.Point(370, 75);
            this.lblOpponent.Name = "lblOpponent";
            this.lblOpponent.Size = new System.Drawing.Size(155, 20);
            this.lblOpponent.TabIndex = 3;
            this.lblOpponent.Text = "🎯 Поле противника";
            // 
            // btnNewGame
            // 
            this.btnNewGame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(144)))), ((int)(((byte)(255)))));
            this.btnNewGame.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.btnNewGame.BorderRadius = 15;
            this.btnNewGame.BorderSize = 2;
            this.btnNewGame.FlatAppearance.BorderSize = 0;
            this.btnNewGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewGame.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNewGame.ForeColor = System.Drawing.Color.White;
            this.btnNewGame.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(191)))), ((int)(((byte)(255)))));
            this.btnNewGame.Location = new System.Drawing.Point(20, 430);
            this.btnNewGame.Name = "btnNewGame";
            this.btnNewGame.Size = new System.Drawing.Size(140, 40);
            this.btnNewGame.TabIndex = 4;
            this.btnNewGame.Text = "🔄 Новая игра";
            this.btnNewGame.UseVisualStyleBackColor = false;
            // 
            // statusLabel
            // 
            this.statusLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(112)))));
            this.statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.statusLabel.ForeColor = System.Drawing.Color.White;
            this.statusLabel.Location = new System.Drawing.Point(0, 480);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(710, 35);
            this.statusLabel.TabIndex = 5;
            this.statusLabel.Text = "🚢 Готов к игре! Разместите ваши корабли.";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(112)))));
            this.panelTitle.Controls.Add(this.lblTitle);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(0, 0);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(710, 60);
            this.panelTitle.TabIndex = 6;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(710, 60);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "🌊 МОРСКОЙ БОЙ";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelShipsInfo
            // 
            this.panelShipsInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(224)))), ((int)(((byte)(230)))));
            this.panelShipsInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelShipsInfo.Controls.Add(this.lblShipsInfo);
            this.panelShipsInfo.Location = new System.Drawing.Point(180, 430);
            this.panelShipsInfo.Name = "panelShipsInfo";
            this.panelShipsInfo.Size = new System.Drawing.Size(510, 40);
            this.panelShipsInfo.TabIndex = 7;
            // 
            // lblShipsInfo
            // 
            this.lblShipsInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblShipsInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblShipsInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(112)))));
            this.lblShipsInfo.Location = new System.Drawing.Point(0, 0);
            this.lblShipsInfo.Name = "lblShipsInfo";
            this.lblShipsInfo.Size = new System.Drawing.Size(508, 38);
            this.lblShipsInfo.TabIndex = 0;
            this.lblShipsInfo.Text = "Корабли: 4-х палубный (1), 3-х палубные (2), 2-х палубные (3), 1-палубные (4)";
            this.lblShipsInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(710, 515);
            this.Controls.Add(this.panelShipsInfo);
            this.Controls.Add(this.panelTitle);
            this.Controls.Add(this.btnNewGame);
            this.Controls.Add(this.lblOpponent);
            this.Controls.Add(this.pnlOpponent);
            this.Controls.Add(this.lblPlayer);
            this.Controls.Add(this.pnlPlayer);
            this.Controls.Add(this.statusLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Морской бой";
            this.panelTitle.ResumeLayout(false);
            this.panelShipsInfo.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private GradientPanel pnlPlayer;
        private System.Windows.Forms.Label lblPlayer;
        private GradientPanel pnlOpponent;
        private System.Windows.Forms.Label lblOpponent;
        private RoundedButton btnNewGame;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelShipsInfo;
        private System.Windows.Forms.Label lblShipsInfo;
    }
}
