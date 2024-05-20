using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Bezier_curves
{
    public partial class Form1 : Form
    {
        public class BezierCurve
        {
            public List<PointF> Points { get; private set; }

            public BezierCurve()
            {
                Points = new List<PointF>();
            }

            public void AddPoint(PointF point)
            {
                Points.Add(point);
            }

            public void MovePoint(int index, PointF newPosition)
            {
                if (index >= 0 && index < Points.Count)
                {
                    Points[index] = newPosition;
                }
            }
        }

        private List<BezierCurve> bezierCurves = new List<BezierCurve>();
        private BezierCurve currentCurve = new BezierCurve();
        private int selectedPointIndex = -1;
        private bool addingMode = false;
        private Color curveColor = Color.Blue;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkedListBox1.SelectedIndexChanged += checkedListBox1_SelectedIndexChanged;
            checkedListBox1.ItemCheck += checkedListBox1_ItemCheck;
            editButton.Click += editButton_Click;
            deleteButton.Click += deleteButton_Click;
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            foreach (BezierCurve curve in bezierCurves)
            {
                if (curve.Points.Count == 4)
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Pen curvePen = new Pen(curveColor, 2);
                    e.Graphics.DrawBezier(curvePen, curve.Points[0], curve.Points[1], curve.Points[2], curve.Points[3]);
                    curvePen.Dispose();
                }
            }

            if (currentCurve.Points.Count == 4)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen curvePen = new Pen(Color.Blue, 2);
                e.Graphics.DrawBezier(curvePen, currentCurve.Points[0], currentCurve.Points[1], currentCurve.Points[2], currentCurve.Points[3]);
                curvePen.Dispose();
            }

            foreach (PointF point in currentCurve.Points)
            {
                e.Graphics.FillEllipse(Brushes.Blue, point.X - 3, point.Y - 3, 6, 6);
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < currentCurve.Points.Count; i++)
            {
                if (Math.Abs(currentCurve.Points[i].X - e.X) < 4 && Math.Abs(currentCurve.Points[i].Y - e.Y) < 4)
                {
                    selectedPointIndex = i;
                    break;
                }
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPointIndex != -1 && !addingMode)
            {
                currentCurve.MovePoint(selectedPointIndex, new PointF(e.X, e.Y));
                pictureBox.Invalidate();
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedPointIndex != -1 && e.Button == MouseButtons.Left && !addingMode)
            {
                selectedPointIndex = -1;
            }
        }

        private bool IsPointUnderMouse(Point mouseLocation)
        {
            foreach (PointF point in currentCurve.Points)
            {
                if (Math.Abs(point.X - mouseLocation.X) < 4 && Math.Abs(point.Y - mouseLocation.Y) < 4)
                {
                    return true;
                }
            }
            return false;
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && addingMode)
            {
                if (currentCurve.Points.Count == 4)
                {
                    addModeButton_Click(sender, e);
                    return;
                }
                currentCurve.AddPoint(new PointF(e.X, e.Y));
                pictureBox.Invalidate();
            }
            else if (e.Button == MouseButtons.Left && !addingMode)
            {
                return;
            }

        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            bezierCurves.Clear();
            UpdateCheckedListBox();
            currentCurve = new BezierCurve();
            pictureBox.Invalidate();
        }

        private void AddBezierCurve(BezierCurve curve)
        {
            bezierCurves.Add(curve);
            UpdateCheckedListBox();
            currentCurve = new BezierCurve();
            pictureBox.Invalidate();

            curveColor = Color.Red;
            selectedPointIndex = -1;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = checkedListBox1.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < bezierCurves.Count)
            {
                currentCurve = bezierCurves[selectedIndex];
                curveColor = Color.Blue;
                pictureBox.Invalidate();
            }
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (addingMode)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        private void addModeButton_Click(object sender, EventArgs e)
        {
            addingMode = !addingMode;
            if (!addingMode)
            {
                addModeButton.Text = "Add";
                if (currentCurve.Points.Count > 0 && currentCurve.Points.Count < 4)
                {
                    AddBezierCurve(currentCurve);
                }
            }
            else
            {
                addModeButton.Text = "Finish";
                if (currentCurve.Points.Count == 4)
                {
                    AddBezierCurve(currentCurve);
                }
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = checkedListBox1.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < bezierCurves.Count)
            {
                currentCurve = bezierCurves[selectedIndex];

                addingMode = true;
                addModeButton.Text = "Finish";

                pictureBox.Invalidate();
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = checkedListBox1.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < bezierCurves.Count)
            {
                bezierCurves.RemoveAt(selectedIndex);
                UpdateCheckedListBox();
                pictureBox.Invalidate();
            }
        }

        private void UpdateCheckedListBox()
        {   
            // Очищаем список
            checkedListBox1.Items.Clear();

            // Добавляем кривые в список
            for (int i = 0; i < bezierCurves.Count; i++)
            {
                checkedListBox1.Items.Add($"Line {i + 1}");
            }
        }
    }
}