using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MIConvexHull;

namespace CalcConvexPolygon
{
    public partial class Form1 : Form
    {
        private Vertex[] _vertices;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _vertices = null;
            Invalidate();
        }

        private Vertex[] CreateVertices()
        {
            const int numberOfVertices = 100;
            const double size = 500;

            var r = new Random();
            var vertices = new Vertex[numberOfVertices];
            for (var i = 0; i < numberOfVertices; i++)
                vertices[i] = new Vertex(size * r.NextDouble(), size * r.NextDouble());
            return vertices;
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            g.TranslateTransform(10f, 10f);

            if (_vertices == null)
                _vertices = CreateVertices();

            //绘制原始随机点组成的线条
            for (var i = 1; i < _vertices.Length; i++)
                g.DrawLine(Pens.BlueViolet, (float) _vertices[i - 1].Position[0], (float) _vertices[i - 1].Position[1],
                    (float) _vertices[i].Position[0], (float) _vertices[i].Position[1]);

            //计算凸包点
            var result1 = ConvexHull.Create(_vertices);
            var convexHull = result1.Points;

            //对凸包点按该点与中心点组成的线与X轴的夹角进行排序，使凸包点列表组成一个多边形
            var edgePoints = new List<PointF>();
            edgePoints.AddRange(
                convexHull.Select(cond => new PointF((float) cond.Position[0], (float) cond.Position[1])));
            edgePoints.Sort(new AngleComparer(edgePoints));

            //绘制凸包多边形
            for (var i = 1; i <= edgePoints.Count; i++)
            {
                var pt1 = edgePoints[i - 1];
                var pt2 = i == edgePoints.Count ? edgePoints[0] : edgePoints[i];
                g.DrawLine(Pens.Green, pt1, pt2);
                g.DrawString(i.ToString(), SystemFonts.CaptionFont, Brushes.Green, pt1.X + 8f, pt1.Y);
            }
        }


        public class Vertex : IVertex
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Vertex" /> class.
            /// </summary>
            /// <param name="x"> The x position. </param>
            /// <param name="y"> The y position. </param>
            public Vertex(double x, double y)
            {
                Position = new[] {x, y};
            }

            public double[] Position { get; set; }
        }

        private class AngleComparer : IComparer<PointF>
        {
            private readonly PointF _center;

            public AngleComparer(List<PointF> list)
            {
                //计算重心
                var center = new PointF();
                var x = 0f;
                var y = 0f;
                foreach (var pt in list)
                {
                    x += pt.X;
                    y += pt.Y;
                }
                center.X = x / list.Count;
                center.Y = y / list.Count;
                _center = center;
            }

            public int Compare(PointF x, PointF y)
            {
                var angleX = GetAngle(_center, x);
                var angleY = GetAngle(_center, y);

                return Comparer<double>.Default.Compare(angleX, angleY);
            }

            private static double GetAngle(PointF p1, PointF p2)
            {
                if (p1.Equals(p2)) return 0.0;
                var l = GetDistance(p1, p2);
                if (p2.Y > p1.Y)
                    return Math.Acos((p2.X - p1.X) / l);
                return 2 * Math.PI - Math.Acos((p2.X - p1.X) / l);
            }

            private static double GetDistance(PointF p1, PointF p2)
            {
                var pt = new PointF(p2.X - p1.X, p2.Y - p1.Y);
                return Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
            }
        }
    }
}