using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Pixel_Wall_E
{
    public partial class Form1 : Form
    {
        private WallEInterpreter interpreter;
        private Bitmap canvas;
        private Graphics graphics;
        private Point currentPosition;
        private Color currentColor = Color.Black;
        private int brushSize = 1;
        private RichTextBox lineNumbersTextBox;

        public Form1()
        {
            InitializeComponent();
            numericUpDownCanvasSize.Value = 600; 
            InitializeCanvas();
            InitializeLineNumbers();
            interpreter = new WallEInterpreter(this);
            UpdateStatusBar();
        }

        private void InitializeCanvas()
        {
            int size = (int)numericUpDownCanvasSize.Value;
            canvas = new Bitmap(size, size);
            graphics = Graphics.FromImage(canvas);
            graphics.Clear(Color.White);

            panelCanvas.Image = canvas;
            panelCanvas.Size = new Size(size, size);
            panelCanvas.Location = new Point(
                textBoxCode.Right + 20,  
                75  
            );

            currentPosition = Point.Empty;
        }

        private void InitializeLineNumbers()
        {
            lineNumbersTextBox = new RichTextBox
            {
                Width = 30,
                Height = textBoxCode.Height,
                Location = new Point(textBoxCode.Left - 35, textBoxCode.Top),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.None,
                Font = textBoxCode.Font,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.DarkGray,
                Multiline = true
            };

            this.Controls.Add(lineNumbersTextBox);
            lineNumbersTextBox.BringToFront();
            lineNumbersTextBox.TextChanged += (s, e) =>
            {
                lineNumbersTextBox.SelectionStart = lineNumbersTextBox.Text.Length;
                lineNumbersTextBox.ScrollToCaret();
            };
        }

        private void UpdateStatusBar()
        {
            toolStripStatusLabelPosition.Text = $"Posición: ({currentPosition.X}, {currentPosition.Y})";
            toolStripStatusLabelColor.Text = $"Color: {currentColor.Name}";
            toolStripStatusLabelBrush.Text = $"Pincel: {brushSize}px";
        }

        public void ExecuteSpawn(int x, int y)
        {
            if (x < 0 || y < 0 || x >= canvas.Width || y >= canvas.Height)
                throw new Exception($"Posición ({x}, {y}) fuera del canvas.");

            currentPosition = new Point(x, y);
            UpdateStatusBar();
        }


        public void SetColor(string colorName)
        {
            currentColor = colorName.ToLower() switch
            {
                "red" => Color.Red,
                "blue" => Color.Blue,
                "green" => Color.Green,
                "yellow" => Color.Yellow,
                "orange" => Color.Orange,
                "purple" => Color.Purple,
                "black" => Color.Black,
                "white" => Color.White,
                "transparent" => Color.Transparent,
                _ => throw new Exception($"Color no válido: '{colorName}'")
            };
            UpdateStatusBar();
        }

        public void SetBrushSize(int size)
        {
            brushSize = Math.Max(1, size);
            if (brushSize % 2 == 0) brushSize--;
            UpdateStatusBar();
        }

        public void DrawLine(int dirX, int dirY, int distance)
        {
            if (currentColor == Color.Transparent) return;
            if (distance <= 0) return;

            // Aumentar distancia para líneas más grandes
            distance = (int)(distance * 1.5);

            Pen pen = new Pen(currentColor, brushSize);
            Point end = new Point(
                currentPosition.X + dirX * distance,
                currentPosition.Y + dirY * distance
            );

            end.X = Math.Max(0, Math.Min(end.X, canvas.Width - 1));
            end.Y = Math.Max(0, Math.Min(end.Y, canvas.Height - 1));

            graphics.DrawLine(pen, currentPosition, end);
            currentPosition = end;
            panelCanvas.Refresh();
            UpdateStatusBar();
        }

        public void DrawCircle(int dirX, int dirY, int radius)
        {
            if (currentColor == Color.Transparent) return;
            if (radius <= 0) return;

            radius = (int)(radius * 1.8);

            Point center = new Point(
                currentPosition.X + dirX * radius,
                currentPosition.Y + dirY * radius
            );

            center.X = Math.Max(radius, Math.Min(center.X, canvas.Width - 1 - radius));
            center.Y = Math.Max(radius, Math.Min(center.Y, canvas.Height - 1 - radius));

            Rectangle rect = new Rectangle(
                center.X - radius,
                center.Y - radius,
                radius * 2,
                radius * 2
            );

            graphics.DrawEllipse(new Pen(currentColor, brushSize), rect);
            currentPosition = center;
            panelCanvas.Refresh();
            UpdateStatusBar();
        }

        public void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
        {
            if (currentColor == Color.Transparent) return;
            if (width <= 0 || height <= 0) return;

            width = (int)(width * 2.5);
            height = (int)(height * 2.5);

            Point center = new Point(
                currentPosition.X + dirX * distance,
                currentPosition.Y + dirY * distance
            );

            center.X = Math.Max(width / 2, Math.Min(center.X, canvas.Width - 1 - width / 2));
            center.Y = Math.Max(height / 2, Math.Min(center.Y, canvas.Height - 1 - height / 2));

            Point topLeft = new Point(
                center.X - width / 2,
                center.Y - height / 2
            );

            graphics.DrawRectangle(
                new Pen(currentColor, brushSize),
                topLeft.X,
                topLeft.Y,
                width,
                height
            );

            currentPosition = center;
            panelCanvas.Refresh();
            UpdateStatusBar();
        }

        public int GetActualX() => currentPosition.X;
        public int GetActualY() => currentPosition.Y;
        public int GetCanvasSize() => canvas.Width;

        public int IsBrushColor(string color)
        {
            Color targetColor = color.ToLower() switch
            {
                "red" => Color.Red,
                "blue" => Color.Blue,
                "green" => Color.Green,
                "yellow" => Color.Yellow,
                "orange" => Color.Orange,
                "purple" => Color.Purple,
                "black" => Color.Black,
                "white" => Color.White,
                "transparent" => Color.Transparent,
                _ => Color.Empty
            };
            return currentColor == targetColor ? 1 : 0;
        }

        public int IsBrushSize(int size) => brushSize == size ? 1 : 0;

        public int IsCanvasColor(string color, int x, int y)
        {
            if (x < 0 || y < 0 || x >= canvas.Width || y >= canvas.Height)
                return 0;

            Color targetColor = color.ToLower() switch
            {
                "red" => Color.Red,
                "blue" => Color.Blue,
                "green" => Color.Green,
                "yellow" => Color.Yellow,
                "orange" => Color.Orange,
                "purple" => Color.Purple,
                "black" => Color.Black,
                "white" => Color.White,
                "transparent" => Color.Transparent,
                _ => Color.Empty
            };

            return canvas.GetPixel(x, y) == targetColor ? 1 : 0;
        }

        public int GetColorCount(string color, int x1, int y1, int x2, int y2)
        {
            int minX = Math.Max(0, Math.Min(x1, x2));
            int maxX = Math.Min(canvas.Width - 1, Math.Max(x1, x2));
            int minY = Math.Max(0, Math.Min(y1, y2));
            int maxY = Math.Min(canvas.Height - 1, Math.Max(y1, y2));

            Color targetColor;
            try
            {
                targetColor = color.ToLower() switch
                {
                    "red" => Color.Red,
                    "blue" => Color.Blue,
                    "green" => Color.Green,
                    "yellow" => Color.Yellow,
                    "orange" => Color.Orange,
                    "purple" => Color.Purple,
                    "black" => Color.Black,
                    "white" => Color.White,
                    "transparent" => Color.Transparent,
                    _ => throw new Exception($"Color no válido: '{color}'")
                };
            }
            catch
            {
                return 0; 
            }

            int count = 0;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (canvas.GetPixel(x, y).ToArgb() == targetColor.ToArgb())
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public void Fill()
        {
            if (currentPosition.X < 0 || currentPosition.Y < 0 ||
                currentPosition.X >= canvas.Width || currentPosition.Y >= canvas.Height ||
                currentColor == Color.Transparent)
            {
                return;
            }

            Color targetColor = canvas.GetPixel(currentPosition.X, currentPosition.Y);

            if (targetColor.ToArgb() == currentColor.ToArgb())
            {
                return;
            }

            Queue<Point> pixels = new Queue<Point>();
            pixels.Enqueue(currentPosition);

            while (pixels.Count > 0)
            {
                Point p = pixels.Dequeue();

                if (p.X < 0 || p.Y < 0 || p.X >= canvas.Width || p.Y >= canvas.Height ||
                    canvas.GetPixel(p.X, p.Y).ToArgb() != targetColor.ToArgb())
                {
                    continue;
                }

                canvas.SetPixel(p.X, p.Y, currentColor);

                pixels.Enqueue(new Point(p.X - 1, p.Y)); // izquierda
                pixels.Enqueue(new Point(p.X + 1, p.Y)); // derecha
                pixels.Enqueue(new Point(p.X, p.Y - 1)); // arriba
                pixels.Enqueue(new Point(p.X, p.Y + 1)); // abajo
            }

            panelCanvas.Refresh();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                interpreter.ProcessLabels(textBoxCode.Lines);

                for (int i = 0; i < textBoxCode.Lines.Length; i++)
                {
                    try
                    {
                        string line = textBoxCode.Lines[i].Trim();
                        if (!string.IsNullOrEmpty(line) && !line.StartsWith("//"))
                        {
                            interpreter.ExecuteCommand(line, i + 1);
                        }
                    }
                    catch (WallEInterpreter.GotoException gotoEx)
                    {
                        i = gotoEx.TargetLine - 1;
                    }
                    catch (Exception ex)
                    {
                        textBoxCode.SelectionStart = textBoxCode.GetFirstCharIndexFromLine(i);
                        textBoxCode.SelectionLength = textBoxCode.Lines[i].Length;
                        textBoxCode.ScrollToCaret();

                        MessageBox.Show($"Error en línea {i + 1}:\n{ex.Message}",
                            "Error de ejecución",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar el código:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Filter = "Archivos Pixel Wall-E (*.pw)|*.pw|Todos los archivos (*.*)|*.*",
                Title = "Abrir archivo de código"
            };

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    textBoxCode.Text = File.ReadAllText(openFile.FileName);
                    this.Text = $"Pixel Wall-E - {openFile.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar el archivo:\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog
            {
                Filter = "Archivos Pixel Wall-E (*.pw)|*.pw|Todos los archivos (*.*)|*.*",
                Title = "Guardar archivo de código",
                DefaultExt = "pw"
            };

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFile.FileName, textBoxCode.Text);
                    this.Text = $"Pixel Wall-E - {saveFile.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar el archivo:\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnResizeCanvas_Click(object sender, EventArgs e)
        {
            try
            {
                if (canvas != null) canvas.Dispose();
                if (graphics != null) graphics.Dispose();

                int newSize = (int)numericUpDownCanvasSize.Value;
                canvas = new Bitmap(newSize, newSize);
                graphics = Graphics.FromImage(canvas);
                graphics.Clear(Color.White);

                panelCanvas.Image = canvas;
                panelCanvas.Size = new Size(newSize, newSize);
                panelCanvas.Location = new Point(
                    textBoxCode.Right + 20,
                    75
                );

                currentPosition = Point.Empty;
                panelCanvas.Invalidate();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al redimensionar: {ex.Message}");
            }
        }

        private void btnClearCanvas_Click(object sender, EventArgs e)
        {
            graphics.Clear(Color.White);
            panelCanvas.Invalidate();
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            try
            {
                if (canvas != null) canvas.Dispose();
                if (graphics != null) graphics.Dispose();

                int newSize = (int)numericUpDownCanvasSize.Value;
                canvas = new Bitmap(newSize, newSize);
                graphics = Graphics.FromImage(canvas);
                graphics.Clear(Color.White);

                panelCanvas.Image = canvas;
                panelCanvas.Size = new Size(newSize, newSize);
                currentPosition = Point.Empty; 

                panelCanvas.Invalidate();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al redimensionar el canvas:\n{ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            lineNumbersTextBox.Height = textBoxCode.Height;
            panelCanvas.Location = new Point(
                textBoxCode.Right + 20,
                75
            );
        }
    }
}
