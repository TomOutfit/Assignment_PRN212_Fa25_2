using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StudentNameWPF.Views.Charts
{
    public partial class OccupancyRateChart : UserControl
    {
        private double _occupancyRate = 0;
        
        public OccupancyRateChart()
        {
            InitializeComponent();
        }
        
        public void UpdateData(double occupancyRate)
        {
            _occupancyRate = occupancyRate;
            DrawChart();
        }
        
        private void DrawChart()
        {
            ChartCanvas.Children.Clear();
            
            var canvasWidth = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 400;
            var canvasHeight = ChartCanvas.ActualHeight > 0 ? ChartCanvas.ActualHeight : 300;
            
            if (canvasWidth <= 0 || canvasHeight <= 0) return;
            
            var centerX = canvasWidth / 2;
            var centerY = canvasHeight / 2;
            var radius = Math.Min(centerX, centerY) - 50;
            
            // Draw background circle
            var backgroundCircle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                StrokeThickness = 20,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(backgroundCircle, centerX - radius);
            Canvas.SetTop(backgroundCircle, centerY - radius);
            ChartCanvas.Children.Add(backgroundCircle);
            
            // Draw progress arc
            if (_occupancyRate > 0)
            {
                var progressAngle = _occupancyRate * 3.6; // Convert percentage to degrees
                var startAngle = -90; // Start from top
                
                var path = new Path
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    StrokeThickness = 20,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure
                {
                    StartPoint = new Point(centerX, centerY)
                };
                
                var arcSegment = new ArcSegment
                {
                    Size = new Size(radius, radius),
                    RotationAngle = 0,
                    IsLargeArc = progressAngle > 180,
                    SweepDirection = SweepDirection.Clockwise,
                    Point = new Point(
                        centerX + radius * Math.Cos((startAngle + progressAngle) * Math.PI / 180),
                        centerY + radius * Math.Sin((startAngle + progressAngle) * Math.PI / 180)
                    )
                };
                
                pathFigure.Segments.Add(new LineSegment(new Point(
                    centerX + radius * Math.Cos(startAngle * Math.PI / 180),
                    centerY + radius * Math.Sin(startAngle * Math.PI / 180)
                ), true));
                pathFigure.Segments.Add(arcSegment);
                
                pathGeometry.Figures.Add(pathFigure);
                path.Data = pathGeometry;
                ChartCanvas.Children.Add(path);
            }
            
            // Add percentage text
            var percentageText = new TextBlock
            {
                Text = $"{_occupancyRate:F1}%",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(percentageText, centerX - 50);
            Canvas.SetTop(percentageText, centerY - 15);
            ChartCanvas.Children.Add(percentageText);
            
            // Add label
            var labelText = new TextBlock
            {
                Text = "Occupancy Rate",
                FontSize = 14,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(labelText, centerX - 60);
            Canvas.SetTop(labelText, centerY + 20);
            ChartCanvas.Children.Add(labelText);
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DrawChart();
        }
    }
}
