using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StudentNameWPF.Views.Charts
{
    public partial class CustomerDistributionChart : UserControl
    {
        private List<CustomerDistributionData> _data = new();
        private readonly Color[] _colors = {
            Color.FromRgb(255, 99, 132),   // Red
            Color.FromRgb(54, 162, 235),  // Blue
            Color.FromRgb(255, 205, 86),  // Yellow
            Color.FromRgb(75, 192, 192),  // Teal
            Color.FromRgb(153, 102, 255), // Purple
            Color.FromRgb(255, 159, 64),  // Orange
            Color.FromRgb(199, 199, 199), // Gray
            Color.FromRgb(83, 102, 255),  // Indigo
            Color.FromRgb(255, 99, 255),  // Pink
            Color.FromRgb(99, 255, 132)   // Green
        };
        
        public CustomerDistributionChart()
        {
            InitializeComponent();
        }
        
        public void UpdateData(List<CustomerDistributionData> data)
        {
            _data = data ?? new List<CustomerDistributionData>();
            System.Diagnostics.Debug.WriteLine($"CustomerDistributionChart: Updating with {_data.Count} data points");
            DrawChart();
        }
        
        private void DrawChart()
        {
            ChartCanvas.Children.Clear();
            
            var canvasWidth = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 400;
            var canvasHeight = ChartCanvas.ActualHeight > 0 ? ChartCanvas.ActualHeight : 300;
            
            if (canvasWidth <= 0 || canvasHeight <= 0) return;
            
            if (!_data.Any())
            {
                NoDataText.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine("CustomerDistributionChart: No data to display");
                return;
            }
            
            NoDataText.Visibility = Visibility.Collapsed;
            System.Diagnostics.Debug.WriteLine($"CustomerDistributionChart: Drawing chart with {_data.Count} data points");
            
            var centerX = canvasWidth / 2;
            var centerY = canvasHeight / 2;
            var radius = Math.Min(centerX, centerY) - 50;
            
            var totalBookings = _data.Sum(d => d.BookingCount);
            if (totalBookings <= 0) return;
            
            double currentAngle = -90; // Start from top
            
            for (int i = 0; i < _data.Count; i++)
            {
                var item = _data[i];
                var percentage = (double)item.BookingCount / totalBookings;
                var sweepAngle = percentage * 360;
                
                var color = _colors[i % _colors.Length];
                
                // Draw pie slice
                var path = new Path
                {
                    Fill = new SolidColorBrush(color),
                    Stroke = Brushes.White,
                    StrokeThickness = 2
                };
                
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure
                {
                    StartPoint = new Point(centerX, centerY)
                };
                
                // Add arc
                var arcSegment = new ArcSegment
                {
                    Size = new Size(radius, radius),
                    RotationAngle = 0,
                    IsLargeArc = sweepAngle > 180,
                    SweepDirection = SweepDirection.Clockwise,
                    Point = new Point(
                        centerX + radius * Math.Cos((currentAngle + sweepAngle) * Math.PI / 180),
                        centerY + radius * Math.Sin((currentAngle + sweepAngle) * Math.PI / 180)
                    )
                };
                
                pathFigure.Segments.Add(new LineSegment(new Point(
                    centerX + radius * Math.Cos(currentAngle * Math.PI / 180),
                    centerY + radius * Math.Sin(currentAngle * Math.PI / 180)
                ), true));
                pathFigure.Segments.Add(arcSegment);
                pathFigure.Segments.Add(new LineSegment(new Point(centerX, centerY), true));
                
                pathGeometry.Figures.Add(pathFigure);
                path.Data = pathGeometry;
                ChartCanvas.Children.Add(path);
                
                // Add label
                var labelAngle = currentAngle + sweepAngle / 2;
                var labelX = centerX + (radius + 30) * Math.Cos(labelAngle * Math.PI / 180);
                var labelY = centerY + (radius + 30) * Math.Sin(labelAngle * Math.PI / 180);
                
                var label = new TextBlock
                {
                    Text = $"{item.CustomerName}\n({item.BookingCount} bookings)",
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(label, labelX - 50);
                Canvas.SetTop(label, labelY - 10);
                ChartCanvas.Children.Add(label);
                
                currentAngle += sweepAngle;
            }
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DrawChart();
        }
        
        public void ForceRedraw()
        {
            System.Diagnostics.Debug.WriteLine("CustomerDistributionChart: Force redraw requested");
            DrawChart();
        }
    }
}
