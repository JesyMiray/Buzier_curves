using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Bezier_curves
{
    public partial class Form1 : Form
    {
        public enum CurveType
        {
            Linear,
            Quadratic,
            Cubic
        }

        public class BezierCurve
        {
            public List<PointF> Points { get; private set; }
            public CurveType Type { get; set; }

            public BezierCurve()
            {
                Points = new List<PointF>();
            }

            public void AddPoint(PointF point)
            {
                if (Points.Count < GetMaxPointsForCurveType())
                {
                    Points.Add(point);
                }
            }

            public void MovePoint(int index, PointF newPosition)
            {
                if (index >= 0 && index < Points.Count)
                {
                    Points[index] = newPosition;
                }
            }

            public int GetMaxPointsForCurveType()
            {
                switch (Type)
                {
                    case CurveType.Linear:
                        return 2;
                    case CurveType.Quadratic:
                        return 3;
                    case CurveType.Cubic:
                        return 4;
                    default:
                        return 0;
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
            InitializeCurveTypeComboBox();
        }

        private void InitializeCurveTypeComboBox()
        {
            curveTypeComboBox.Items.AddRange(Enum.GetNames(typeof(CurveType)));
            curveTypeComboBox.SelectedIndex = 0;
            curveTypeComboBox.SelectedIndexChanged += CurveTypeComboBox_SelectedIndexChanged;
        }

        private void CurveTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentCurve.Type = (CurveType)curveTypeComboBox.SelectedIndex;
            pictureBox.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkedListBox1.SelectedIndexChanged += checkedListBox1_SelectedIndexChanged;
            checkedListBox1.ItemCheck += checkedListBox1_ItemCheck;
            editButton.Click += editButton_Click;
            deleteButton.Click += deleteButton_Click;
            addModeButton.Click += addModeButton_Click;
            clearButton.Click += clearButton_Click;
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            foreach (BezierCurve curve in bezierCurves)
            {
                DrawBezierCurve(e, curve);
            }

            DrawBezierCurve(e, currentCurve);

            foreach (PointF point in currentCurve.Points)
            {
                e.Graphics.FillEllipse(Brushes.Blue, point.X - 3, point.Y - 3, 6, 6);
            }
        }

        private void DrawQuadraticBezier(Graphics g, Pen pen, PointF p0, PointF p1, PointF p2)
        {
            List<PointF> bezierPoints = new List<PointF>();
            for (float t = 0; t <= 1; t += 0.01f)
            {
                float x = (1 - t) * (1 - t) * p0.X + 2 * (1 - t) * t * p1.X + t * t * p2.X;
                float y = (1 - t) * (1 - t) * p0.Y + 2 * (1 - t) * t * p1.Y + t * t * p2.Y;
                bezierPoints.Add(new PointF(x, y));
            }
            g.DrawLines(pen, bezierPoints.ToArray());
        }

        private void DrawBezierCurve(PaintEventArgs e, BezierCurve curve)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Pen curvePen = new Pen(curveColor, 2);

            switch (curve.Type)
            {
                case CurveType.Linear:
                    if (curve.Points.Count == 2)
                    {
                        e.Graphics.DrawLine(curvePen, curve.Points[0], curve.Points[1]);
                    }
                    break;
                case CurveType.Quadratic:
                    if (curve.Points.Count == 3)
                    {
                        DrawQuadraticBezier(e.Graphics, curvePen, curve.Points[0], curve.Points[1], curve.Points[2]);
                    }
                    break;
                case CurveType.Cubic:
                    if (curve.Points.Count == 4)
                    {
                        e.Graphics.DrawBezier(curvePen, curve.Points[0], curve.Points[1], curve.Points[2], curve.Points[3]);
                    }
                    break;
            }

            curvePen.Dispose();
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (addingMode)
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
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (addingMode && selectedPointIndex != -1)
            {
                currentCurve.MovePoint(selectedPointIndex, new PointF(e.X, e.Y));
                pictureBox.Invalidate();
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (addingMode && selectedPointIndex != -1)
            {
                selectedPointIndex = -1;
            }
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && addingMode)
            {
                if (currentCurve.Points.Count < currentCurve.GetMaxPointsForCurveType())
                {
                    currentCurve.AddPoint(new PointF(e.X, e.Y));
                    pictureBox.Invalidate();
                }
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
            addModeButton.Text = addingMode ? "Finish" : "Add";
            if (!addingMode && currentCurve.Points.Count > 0)
            {
                AddBezierCurve(currentCurve);
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
            checkedListBox1.Items.Clear();
            for (int i = 0; i < bezierCurves.Count; i++)
            {
                checkedListBox1.Items.Add($"Curve {i + 1}");
            }
        }
    }
}
