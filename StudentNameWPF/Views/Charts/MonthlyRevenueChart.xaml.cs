using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace StudentNameWPF.Views.Charts
{
    public partial class MonthlyRevenueChart : UserControl
    {
        private List<MonthlyRevenueData> _data = new();
        
        public MonthlyRevenueChart()
        {
            InitializeComponent();
        }
        
        public void UpdateData(List<MonthlyRevenueData> data)
        {
            _data = data ?? new List<MonthlyRevenueData>();
            System.Diagnostics.Debug.WriteLine($"MonthlyRevenueChart: Updating with {_data.Count} data points");
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
                System.Diagnostics.Debug.WriteLine("MonthlyRevenueChart: No data to display");
                return;
            }
            
            NoDataText.Visibility = Visibility.Collapsed;
            System.Diagnostics.Debug.WriteLine($"MonthlyRevenueChart: Drawing chart with {_data.Count} data points");
            
            var margin = 40;
            var chartWidth = canvasWidth - 2 * margin;
            var chartHeight = canvasHeight - 2 * margin;
            
            var maxRevenue = _data.Max(d => d.Revenue);
            if (maxRevenue <= 0) maxRevenue = 1;
            
            // Draw axes
            var xAxis = new Line
            {
                X1 = margin,
                Y1 = margin + chartHeight,
                X2 = margin + chartWidth,
                Y2 = margin + chartHeight,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            ChartCanvas.Children.Add(xAxis);
            
            var yAxis = new Line
            {
                X1 = margin,
                Y1 = margin,
                X2 = margin,
                Y2 = margin + chartHeight,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            ChartCanvas.Children.Add(yAxis);
            
            // Draw data points and lines
            var points = new List<Point>();
            var barWidth = chartWidth / _data.Count * 0.8;
            var barSpacing = chartWidth / _data.Count;
            
            for (int i = 0; i < _data.Count; i++)
            {
                var item = _data[i];
                var x = margin + (i * barSpacing) + (barSpacing - barWidth) / 2;
                var height = (double)(item.Revenue / maxRevenue) * chartHeight;
                var y = margin + chartHeight - height;
                
                points.Add(new Point(x + barWidth / 2, y));
                
                // Draw bar
                var bar = new Rectangle
                {
                    Width = barWidth,
                    Height = height,
                    Fill = new SolidColorBrush(Color.FromRgb(75, 192, 192)),
                    Stroke = new SolidColorBrush(Color.FromRgb(54, 162, 235)),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(bar, x);
                Canvas.SetTop(bar, y);
                ChartCanvas.Children.Add(bar);
                
                // Add value label
                var valueLabel = new TextBlock
                {
                    Text = $"${item.Revenue:N0}",
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(valueLabel, x);
                Canvas.SetTop(valueLabel, y - 20);
                ChartCanvas.Children.Add(valueLabel);
                
                // Add month label
                var monthLabel = new TextBlock
                {
                    Text = item.MonthName,
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(monthLabel, x);
                Canvas.SetTop(monthLabel, margin + chartHeight + 5);
                ChartCanvas.Children.Add(monthLabel);
            }
            
            // Draw line connecting points
            if (points.Count > 1)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    var line = new Line
                    {
                        X1 = points[i].X,
                        Y1 = points[i].Y,
                        X2 = points[i + 1].X,
                        Y2 = points[i + 1].Y,
                        Stroke = new SolidColorBrush(Color.FromRgb(255, 99, 132)),
                        StrokeThickness = 3
                    };
                    ChartCanvas.Children.Add(line);
                }
            }
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DrawChart();
        }
        
        public void ForceRedraw()
        {
            System.Diagnostics.Debug.WriteLine("MonthlyRevenueChart: Force redraw requested");
            DrawChart();
        }
    }
}
