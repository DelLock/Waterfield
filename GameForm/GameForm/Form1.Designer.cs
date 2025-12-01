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
            this.pnlPlayer = new System.Windows.Forms.Panel();
            this.lblPlayer = new System.Windows.Forms.Label();
            this.pnlOpponent = new System.Windows.Forms.Panel();
            this.lblOpponent = new System.Windows.Forms.Label();
            this.btnNewGame = new System.Windows.Forms.Button();
            this.chkTwoPlayers = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();

            // 
            // pnlPlayer
            // 
            this.pnlPlayer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPlayer.Location = new System.Drawing.Point(20, 40);
            this.pnlPlayer.Name = "pnlPlayer";
            this.pnlPlayer.Size = new System.Drawing.Size(300, 300);
            this.pnlPlayer.TabIndex = 0;

            // 
            // lblPlayer
            // 
            this.lblPlayer.AutoSize = true;
            this.lblPlayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPlayer.Location = new System.Drawing.Point(20, 20);
            this.lblPlayer.Name = "lblPlayer";
            this.lblPlayer.Size = new System.Drawing.Size(79, 15);
            this.lblPlayer.TabIndex = 1;
            this.lblPlayer.Text = "Ваше поле";

            // 
            // pnlOpponent
            // 
            this.pnlOpponent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOpponent.Location = new System.Drawing.Point(350, 40);
            this.pnlOpponent.Name = "pnlOpponent";
            this.pnlOpponent.Size = new System.Drawing.Size(300, 300);
            this.pnlOpponent.TabIndex = 2;

            // 
            // lblOpponent
            // 
            this.lblOpponent.AutoSize = true;
            this.lblOpponent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblOpponent.Location = new System.Drawing.Point(350, 20);
            this.lblOpponent.Name = "lblOpponent";
            this.lblOpponent.Size = new System.Drawing.Size(123, 15);
            this.lblOpponent.TabIndex = 3;
            this.lblOpponent.Text = "Поле противника";

            // 
            // btnNewGame
            // 
            this.btnNewGame.Location = new System.Drawing.Point(20, 360);
            this.btnNewGame.Name = "btnNewGame";
            this.btnNewGame.Size = new System.Drawing.Size(100, 30);
            this.btnNewGame.TabIndex = 4;
            this.btnNewGame.Text = "Новая игра";
            this.btnNewGame.UseVisualStyleBackColor = true;

            // 
            // chkTwoPlayers
            // 
            this.chkTwoPlayers.AutoSize = true;
            this.chkTwoPlayers.Location = new System.Drawing.Point(130, 368);
            this.chkTwoPlayers.Name = "chkTwoPlayers";
            this.chkTwoPlayers.Size = new System.Drawing.Size(85, 19);
            this.chkTwoPlayers.TabIndex = 5;
            this.chkTwoPlayers.Text = "2 игрока";
            this.chkTwoPlayers.UseVisualStyleBackColor = true;

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 410);
            this.Controls.Add(this.chkTwoPlayers);
            this.Controls.Add(this.btnNewGame);
            this.Controls.Add(this.lblOpponent);
            this.Controls.Add(this.pnlOpponent);
            this.Controls.Add(this.lblPlayer);
            this.Controls.Add(this.pnlPlayer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Морской бой";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlPlayer;
        private System.Windows.Forms.Label lblPlayer;
        private System.Windows.Forms.Panel pnlOpponent;
        private System.Windows.Forms.Label lblOpponent;
        private System.Windows.Forms.Button btnNewGame;
        private System.Windows.Forms.CheckBox chkTwoPlayers;
    }
}