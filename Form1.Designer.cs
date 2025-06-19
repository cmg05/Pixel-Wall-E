namespace Pixel_Wall_E
{
    partial class Form1
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelCanvas = new PictureBox();
            textBoxCode = new RichTextBox();
            numericUpDownCanvasSize = new NumericUpDown();
            btnResize = new Button();
            btnExecute = new Button();
            btnLoad = new Button();
            btnSave = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabelPosition = new ToolStripStatusLabel();
            toolStripStatusLabelColor = new ToolStripStatusLabel();
            toolStripStatusLabelBrush = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)panelCanvas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownCanvasSize).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // panelCanvas
            // 
            panelCanvas.BackColor = Color.White;
            panelCanvas.BorderStyle = BorderStyle.FixedSingle;
            panelCanvas.Location = new Point(367, 75);
            panelCanvas.Name = "panelCanvas";
            panelCanvas.Size = new Size(320, 320);
            panelCanvas.TabIndex = 0;
            panelCanvas.TabStop = false;
            // 
            // textBoxCode
            // 
            textBoxCode.Font = new Font("Consolas", 10F);
            textBoxCode.Location = new Point(20, 75);
            textBoxCode.Name = "textBoxCode";
            textBoxCode.Size = new Size(300, 320);
            textBoxCode.TabIndex = 1;
            textBoxCode.Text = "";
            // 
            // numericUpDownCanvasSize
            // 
            numericUpDownCanvasSize.Location = new Point(20, 460);
            numericUpDownCanvasSize.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numericUpDownCanvasSize.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownCanvasSize.Name = "numericUpDownCanvasSize";
            numericUpDownCanvasSize.Size = new Size(120, 27);
            numericUpDownCanvasSize.TabIndex = 2;
            numericUpDownCanvasSize.Value = new decimal(new int[] { 320, 0, 0, 0 });
            // 
            // btnResize
            // 
            btnResize.Location = new Point(150, 460);
            btnResize.Name = "btnResize";
            btnResize.Size = new Size(120, 27);
            btnResize.TabIndex = 3;
            btnResize.Text = "Redimensionar";
            btnResize.UseVisualStyleBackColor = true;
            btnResize.Click += btnResize_Click;
            // 
            // btnExecute
            // 
            btnExecute.Location = new Point(20, 420);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new Size(120, 27);
            btnExecute.TabIndex = 4;
            btnExecute.Text = "Ejecutar";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(150, 420);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(120, 27);
            btnLoad.TabIndex = 5;
            btnLoad.Text = "Cargar";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(20, 503);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 27);
            btnSave.TabIndex = 6;
            btnSave.Text = "Guardar";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelPosition, toolStripStatusLabelColor, toolStripStatusLabelBrush });
            statusStrip1.Location = new Point(0, 570);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 30);
            statusStrip1.TabIndex = 7;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelPosition
            // 
            toolStripStatusLabelPosition.BorderSides = ToolStripStatusLabelBorderSides.Right;
            toolStripStatusLabelPosition.BorderStyle = Border3DStyle.Etched;
            toolStripStatusLabelPosition.ForeColor = SystemColors.Control;
            toolStripStatusLabelPosition.Name = "toolStripStatusLabelPosition";
            toolStripStatusLabelPosition.Size = new Size(107, 24);
            toolStripStatusLabelPosition.Text = "Posición: (0, 0)";
            // 
            // toolStripStatusLabelColor
            // 
            toolStripStatusLabelColor.BorderSides = ToolStripStatusLabelBorderSides.Right;
            toolStripStatusLabelColor.BorderStyle = Border3DStyle.Etched;
            toolStripStatusLabelColor.ForeColor = SystemColors.ButtonFace;
            toolStripStatusLabelColor.Name = "toolStripStatusLabelColor";
            toolStripStatusLabelColor.Size = new Size(91, 24);
            toolStripStatusLabelColor.Text = "Color: Black";
            // 
            // toolStripStatusLabelBrush
            // 
            toolStripStatusLabelBrush.ForeColor = SystemColors.ButtonFace;
            toolStripStatusLabelBrush.Name = "toolStripStatusLabelBrush";
            toolStripStatusLabelBrush.Size = new Size(79, 24);
            toolStripStatusLabelBrush.Text = "Pincel: 1px";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.MidnightBlue;
            ClientSize = new Size(800, 600);
            Controls.Add(statusStrip1);
            Controls.Add(btnSave);
            Controls.Add(btnLoad);
            Controls.Add(btnExecute);
            Controls.Add(btnResize);
            Controls.Add(numericUpDownCanvasSize);
            Controls.Add(textBoxCode);
            Controls.Add(panelCanvas);
            Name = "Form1";
            Text = "Pixel Wall-E";
            ((System.ComponentModel.ISupportInitialize)panelCanvas).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownCanvasSize).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox panelCanvas;
        private RichTextBox textBoxCode;
        private NumericUpDown numericUpDownCanvasSize;
        private Button btnResize;
        private Button btnExecute;
        private Button btnLoad;
        private Button btnSave;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabelPosition;
        private ToolStripStatusLabel toolStripStatusLabelColor;
        private ToolStripStatusLabel toolStripStatusLabelBrush;
    }
}
